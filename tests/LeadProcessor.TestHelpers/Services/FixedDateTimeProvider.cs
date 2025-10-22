namespace LeadProcessor.TestHelpers.Services;

using LeadProcessor.Domain.Services;

/// <summary>
/// Test implementation of <see cref="IDateTimeProvider"/> that returns a fixed, predetermined datetime.
/// </summary>
/// <remarks>
/// This implementation is ideal for unit and integration tests where deterministic, repeatable
/// time values are required. The fixed datetime is set at construction and never changes,
/// ensuring consistent test results regardless of when the test runs.
/// Thread-safe and immutable after construction.
/// </remarks>
/// <param name="fixedDateTime">The fixed UTC datetime to return for all calls to <see cref="UtcNow"/>.</param>
public sealed class FixedDateTimeProvider(DateTimeOffset fixedDateTime) : IDateTimeProvider
{
    private readonly DateTimeOffset _fixedDateTime = fixedDateTime;

    /// <summary>
    /// Gets the fixed UTC date and time that was set at construction.
    /// </summary>
    /// <remarks>
    /// This property always returns the same datetime value that was provided to the constructor,
    /// enabling deterministic testing of time-dependent logic.
    /// Thread-safe - can be called concurrently from multiple threads.
    /// </remarks>
    public DateTimeOffset UtcNow => _fixedDateTime;
}

