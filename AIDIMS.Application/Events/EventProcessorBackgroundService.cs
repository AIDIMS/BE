using System.Threading.Channels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AIDIMS.Application.Events;

/// <summary>
/// Background Service xử lý events từ channel queue
/// </summary>
public class EventProcessorBackgroundService : BackgroundService
{
    private readonly ChannelReader<object> _channelReader;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<EventProcessorBackgroundService> _logger;
    private readonly Dictionary<Type, Type> _eventHandlers;

    public EventProcessorBackgroundService(
        ChannelReader<object> channelReader,
        IServiceProvider serviceProvider,
        ILogger<EventProcessorBackgroundService> logger)
    {
        _channelReader = channelReader;
        _serviceProvider = serviceProvider;
        _logger = logger;
        _eventHandlers = new Dictionary<Type, Type>();

        // Register event handlers
        RegisterEventHandlers();
    }

    private void RegisterEventHandlers()
    {
        // Tự động discover và register handlers từ assembly
        var handlerTypes = typeof(EventProcessorBackgroundService).Assembly
            .GetTypes()
            .Where(t => 
                t.IsClass && 
                !t.IsAbstract &&
                t.GetInterfaces()
                    .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEventHandler<>)))
            .ToList();

        foreach (var handlerType in handlerTypes)
        {
            var eventType = handlerType.GetInterfaces()
                .First(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEventHandler<>))
                .GetGenericArguments()[0];

            _eventHandlers[eventType] = handlerType;
            _logger.LogInformation("Registered event handler: {HandlerType} for event: {EventType}", 
                handlerType.Name, eventType.Name);
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Event Processor Background Service started. Registered {HandlerCount} event handlers", _eventHandlers.Count);

        await foreach (var eventData in _channelReader.ReadAllAsync(stoppingToken))
        {
            try
            {
                _logger.LogDebug("Received event from queue: {EventType}", eventData.GetType().Name);
                await ProcessEventAsync(eventData, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing event: {EventType}. Error: {ErrorMessage}", 
                    eventData.GetType().Name, ex.Message);
            }
        }

        _logger.LogInformation("Event Processor Background Service stopped");
    }

    private async Task ProcessEventAsync(object eventData, CancellationToken cancellationToken)
    {
        var eventType = eventData.GetType();

        if (!_eventHandlers.TryGetValue(eventType, out var handlerType))
        {
            _logger.LogWarning("No handler found for event type: {EventType}. Available handlers: {Handlers}", 
                eventType.Name, string.Join(", ", _eventHandlers.Keys.Select(t => t.Name)));
            return;
        }

        _logger.LogInformation("Processing event {EventType} with handler {HandlerType}", eventType.Name, handlerType.Name);

        // Tạo scope mới cho mỗi event để tránh disposed service issues
        using var scope = _serviceProvider.CreateScope();
        
        // Resolve handler bằng interface type (IEventHandler<T>) thay vì concrete type
        var handlerInterfaceType = typeof(IEventHandler<>).MakeGenericType(eventType);
        var handler = scope.ServiceProvider.GetService(handlerInterfaceType);

        if (handler == null)
        {
            _logger.LogError("Handler {HandlerType} (interface: {InterfaceType}) not registered in DI container", 
                handlerType.Name, handlerInterfaceType.Name);
            return;
        }

        // Gọi HandleAsync method qua reflection
        var handleMethod = handlerType.GetMethod("HandleAsync");
        if (handleMethod == null)
        {
            _logger.LogError("HandleAsync method not found in handler: {HandlerType}", handlerType.Name);
            return;
        }

        try
        {
            var task = (Task?)handleMethod.Invoke(handler, new[] { eventData, cancellationToken });
            if (task != null)
            {
                await task;
            }

            _logger.LogInformation("Event {EventType} processed successfully by {HandlerType}", eventType.Name, handlerType.Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error invoking handler {HandlerType} for event {EventType}. Error: {ErrorMessage}", 
                handlerType.Name, eventType.Name, ex.Message);
            throw;
        }
    }
}

