namespace LeadProcessor.Domain.Services;

/// <summary>
/// Abstraction for providing current date and time.
/// This allows for deterministic testing and UTC time enforcement.
/// </summary>
public interface IDateTimeProvider
{
    /// <summary>
    /// Gets the current UTC date and time as a DateTimeOffset.
    /// </summary>
    DateTimeOffset UtcNow { get; }
}

