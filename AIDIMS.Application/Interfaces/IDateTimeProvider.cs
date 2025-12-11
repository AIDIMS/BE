namespace AIDIMS.Application.Interfaces;

public interface IDateTimeProvider
{
    /// <summary>
    /// Gets current time in Vietnam timezone (UTC+7)
    /// </summary>
    DateTime Now { get; }

    /// <summary>
    /// Gets current date in Vietnam timezone (UTC+7)
    /// </summary>
    DateTime Today { get; }
}
