namespace AIDIMS.Application.Events;

/// <summary>
/// Interface để publish domain events
/// </summary>
public interface IEventPublisher
{
    /// <summary>
    /// Publish event để xử lý bất đồng bộ
    /// </summary>
    Task PublishAsync<T>(T eventData, CancellationToken cancellationToken = default) where T : class;
}

