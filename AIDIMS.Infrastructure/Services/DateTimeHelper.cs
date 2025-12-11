namespace AIDIMS.Infrastructure.Services;

public static class DateTimeHelper
{
    private static readonly TimeZoneInfo VietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");

    /// <summary>
    /// Gets current time in Vietnam timezone (UTC+7)
    /// </summary>
    public static DateTime GetVietnamTime()
    {
        return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, VietnamTimeZone);
    }

    /// <summary>
    /// Converts UTC time to Vietnam time
    /// </summary>
    public static DateTime ToVietnamTime(DateTime utcTime)
    {
        if (utcTime.Kind != DateTimeKind.Utc)
        {
            utcTime = DateTime.SpecifyKind(utcTime, DateTimeKind.Utc);
        }
        return TimeZoneInfo.ConvertTimeFromUtc(utcTime, VietnamTimeZone);
    }

    /// <summary>
    /// Converts Vietnam time to UTC
    /// </summary>
    public static DateTime ToUtc(DateTime vietnamTime)
    {
        return TimeZoneInfo.ConvertTimeToUtc(vietnamTime, VietnamTimeZone);
    }
}
