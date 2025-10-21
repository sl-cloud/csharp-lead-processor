using LeadProcessor.Domain.Services;

namespace LeadProcessor.Infrastructure.Services;

/// <summary>
/// System implementation of IDateTimeProvider that returns the actual system time.
/// </summary>
public class SystemDateTimeProvider : IDateTimeProvider
{
    /// <summary>
    /// Gets the current UTC date and time from the system clock.
    /// </summary>
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}

