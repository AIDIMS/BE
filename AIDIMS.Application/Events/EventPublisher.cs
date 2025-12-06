using System.Threading.Channels;
using Microsoft.Extensions.Logging;

namespace AIDIMS.Application.Events;

/// <summary>
/// Event Publisher sử dụng Channel để queue events
/// </summary>
public class EventPublisher : IEventPublisher
{
    private readonly ChannelWriter<object> _channelWriter;
    private readonly ILogger<EventPublisher> _logger;

    public EventPublisher(
        ChannelWriter<object> channelWriter,
        ILogger<EventPublisher> logger)
    {
        _channelWriter = channelWriter;
        _logger = logger;
    }

    public async Task PublishAsync<T>(T eventData, CancellationToken cancellationToken = default) where T : class
    {
        try
        {
            await _channelWriter.WriteAsync(eventData, cancellationToken);
            _logger.LogDebug("Event published: {EventType}", typeof(T).Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish event: {EventType}", typeof(T).Name);
            throw;
        }
    }
}

