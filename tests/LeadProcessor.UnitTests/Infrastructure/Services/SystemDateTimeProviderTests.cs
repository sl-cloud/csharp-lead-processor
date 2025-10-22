using LeadProcessor.Infrastructure.Services;

namespace LeadProcessor.UnitTests.Infrastructure.Services;

/// <summary>
/// Unit tests for <see cref="SystemDateTimeProvider"/>.
/// </summary>
public class SystemDateTimeProviderTests
{
    [Fact]
    public void UtcNow_ReturnsCurrentUtcTime()
    {
        // Arrange
        var provider = new SystemDateTimeProvider();
        var beforeCall = DateTimeOffset.UtcNow;

        // Act
        var result = provider.UtcNow;

        // Assert
        var afterCall = DateTimeOffset.UtcNow;
        Assert.InRange(result, beforeCall.AddMilliseconds(-10), afterCall.AddMilliseconds(10));
        Assert.Equal(TimeSpan.Zero, result.Offset); // Verify UTC offset
    }

    [Fact]
    public void UtcNow_ReturnsUtcOffset()
    {
        // Arrange
        var provider = new SystemDateTimeProvider();

        // Act
        var result = provider.UtcNow;

        // Assert
        Assert.Equal(TimeSpan.Zero, result.Offset);
    }

    [Fact]
    public void UtcNow_CalledMultipleTimes_ReturnsProgressingTime()
    {
        // Arrange
        var provider = new SystemDateTimeProvider();

        // Act
        var first = provider.UtcNow;
        Thread.Sleep(50); // Small delay to ensure time progresses
        var second = provider.UtcNow;

        // Assert
        Assert.True(second >= first, "Second call should return time >= first call");
    }

    [Fact]
    public void UtcNow_IsThreadSafe()
    {
        // Arrange
        var provider = new SystemDateTimeProvider();
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
        Assert.All(results, r => Assert.Equal(TimeSpan.Zero, r.Offset));
    }
}

