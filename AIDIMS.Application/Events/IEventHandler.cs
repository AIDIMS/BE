namespace AIDIMS.Application.Events;

/// <summary>
/// Interface cho event handlers
/// </summary>
public interface IEventHandler<in T> where T : class
{
    Task HandleAsync(T eventData, CancellationToken cancellationToken = default);
}

