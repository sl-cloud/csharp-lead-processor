namespace LeadProcessor.Infrastructure.Services;

using LeadProcessor.Domain.Services;

/// <summary>
/// Production implementation of <see cref="IDateTimeProvider"/> that returns the current system UTC time.
/// </summary>
/// <remarks>
/// This implementation is thread-safe and stateless, making it suitable for singleton registration in DI.
/// Always returns the current UTC time via <see cref="DateTimeOffset.UtcNow"/>.
/// </remarks>
public sealed class SystemDateTimeProvider : IDateTimeProvider
{
    /// <summary>
    /// Gets the current UTC date and time from the system clock.
    /// </summary>
    /// <remarks>
    /// This property is thread-safe and can be called concurrently from multiple threads.
    /// Each access returns the current UTC time at the moment of the call.
    /// </remarks>
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
