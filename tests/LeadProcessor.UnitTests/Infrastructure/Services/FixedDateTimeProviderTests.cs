using LeadProcessor.TestHelpers.Services;

namespace LeadProcessor.UnitTests.Infrastructure.Services;

/// <summary>
/// Unit tests for <see cref="FixedDateTimeProvider"/>.
/// </summary>
public class FixedDateTimeProviderTests
{
    [Fact]
    public void UtcNow_ReturnsFixedDateTime()
    {
        // Arrange
        var fixedTime = new DateTimeOffset(2025, 1, 15, 10, 30, 45, TimeSpan.Zero);
        var provider = new FixedDateTimeProvider(fixedTime);

        // Act
        var result = provider.UtcNow;

        // Assert
        Assert.Equal(fixedTime, result);
    }

    [Fact]
    public void UtcNow_CalledMultipleTimes_ReturnsConsistentValue()
    {
        // Arrange
        var fixedTime = new DateTimeOffset(2025, 1, 15, 10, 30, 45, TimeSpan.Zero);
        var provider = new FixedDateTimeProvider(fixedTime);

        // Act
        var first = provider.UtcNow;
        Thread.Sleep(50); // Ensure time would have progressed if not fixed
        var second = provider.UtcNow;
        var third = provider.UtcNow;

        // Assert
        Assert.Equal(fixedTime, first);
        Assert.Equal(fixedTime, second);
        Assert.Equal(fixedTime, third);
        Assert.Equal(first, second);
        Assert.Equal(second, third);
    }

    [Fact]
    public void UtcNow_PreservesUtcOffset()
    {
        // Arrange
        var fixedTime = new DateTimeOffset(2025, 1, 15, 10, 30, 45, TimeSpan.Zero);
        var provider = new FixedDateTimeProvider(fixedTime);

        // Act
        var result = provider.UtcNow;

        // Assert
        Assert.Equal(TimeSpan.Zero, result.Offset);
    }

    [Fact]
    public void Constructor_WithNonUtcOffset_StillReturnsProvidedValue()
    {
        // Arrange - Test that provider accepts any offset (though UTC is recommended)
        var fixedTime = new DateTimeOffset(2025, 1, 15, 10, 30, 45, TimeSpan.FromHours(5));
        var provider = new FixedDateTimeProvider(fixedTime);

        // Act
        var result = provider.UtcNow;

        // Assert
        Assert.Equal(fixedTime, result);
        Assert.Equal(TimeSpan.FromHours(5), result.Offset);
    }

    [Fact]
    public void UtcNow_IsThreadSafe()
    {
        // Arrange
        var fixedTime = new DateTimeOffset(2025, 1, 15, 10, 30, 45, TimeSpan.Zero);
        var provider = new FixedDateTimeProvider(fixedTime);
        var results = new List<DateTimeOffset>();
        var tasks = new List<Task>();
        var lockObject = new object();

        // Act - Call from multiple threads concurrently
        for (int i = 0; i < 10; i++)
        {
            tasks.Add(Task.Run(() =>
            {
                var result = provider.UtcNow;
                lock (lockObject)
                {
                    results.Add(result);
                }
            }));
        }

        Task.WaitAll(tasks.ToArray());

        // Assert
        Assert.Equal(10, results.Count);
        Assert.All(results, r => Assert.Equal(fixedTime, r));
    }

    [Fact]
    public void Constructor_DifferentInstances_ReturnDifferentFixedTimes()
    {
        // Arrange
        var firstTime = new DateTimeOffset(2025, 1, 15, 10, 30, 45, TimeSpan.Zero);
        var secondTime = new DateTimeOffset(2025, 2, 20, 14, 15, 30, TimeSpan.Zero);
        var firstProvider = new FixedDateTimeProvider(firstTime);
        var secondProvider = new FixedDateTimeProvider(secondTime);

        // Act
        var firstResult = firstProvider.UtcNow;
        var secondResult = secondProvider.UtcNow;

        // Assert
        Assert.Equal(firstTime, firstResult);
        Assert.Equal(secondTime, secondResult);
        Assert.NotEqual(firstResult, secondResult);
    }
}

