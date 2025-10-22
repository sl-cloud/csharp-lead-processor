using LeadProcessor.Infrastructure.Services;

namespace LeadProcessor.UnitTests.Infrastructure.Services;

/// <summary>
/// Unit tests for <see cref="DbConnectionStringProvider"/>.
/// </summary>
public sealed class DbConnectionStringProviderTests
{
    #region Initialization Tests

    [Fact]
    public void Initialize_WithValidConnectionString_StoresConnectionString()
    {
        // Arrange
        var provider = new DbConnectionStringProvider();
        const string connectionString = "Server=localhost;Port=3306;Database=test;User=root;Password=password";

        // Act
        provider.Initialize(connectionString);
        var result = provider.GetConnectionString();

        // Assert
        Assert.Equal(connectionString, result);
    }

    [Fact]
    public void Initialize_WithNullConnectionString_ThrowsArgumentNullException()
    {
        // Arrange
        var provider = new DbConnectionStringProvider();

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => provider.Initialize(null!));
        Assert.Equal("connectionString", exception.ParamName);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t")]
    [InlineData("\n")]
    public void Initialize_WithEmptyOrWhitespaceConnectionString_ThrowsArgumentException(string invalidConnectionString)
    {
        // Arrange
        var provider = new DbConnectionStringProvider();

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => provider.Initialize(invalidConnectionString));
        Assert.Equal("connectionString", exception.ParamName);
        Assert.Contains("cannot be empty or whitespace", exception.Message);
    }

    [Fact]
    public void Initialize_CalledMultipleTimes_OverwritesPreviousValue()
    {
        // Arrange
        var provider = new DbConnectionStringProvider();
        const string firstConnectionString = "Server=localhost;Port=3306;Database=test1;User=root;Password=password1";
        const string secondConnectionString = "Server=localhost;Port=3306;Database=test2;User=root;Password=password2";

        // Act
        provider.Initialize(firstConnectionString);
        provider.Initialize(secondConnectionString);
        var result = provider.GetConnectionString();

        // Assert
        Assert.Equal(secondConnectionString, result);
    }

    #endregion

    #region GetConnectionString Tests

    [Fact]
    public void GetConnectionString_BeforeInitialization_ThrowsInvalidOperationException()
    {
        // Arrange
        var provider = new DbConnectionStringProvider();

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => provider.GetConnectionString());
        Assert.Contains("not initialized", exception.Message);
        Assert.Contains("Initialize()", exception.Message);
    }

    [Fact]
    public void GetConnectionString_AfterInitialization_ReturnsConnectionString()
    {
        // Arrange
        var provider = new DbConnectionStringProvider();
        const string connectionString = "Server=localhost;Port=3306;Database=test;User=root;Password=password";
        provider.Initialize(connectionString);

        // Act
        var result = provider.GetConnectionString();

        // Assert
        Assert.Equal(connectionString, result);
    }

    [Fact]
    public void GetConnectionString_CalledMultipleTimes_ReturnsConsistentValue()
    {
        // Arrange
        var provider = new DbConnectionStringProvider();
        const string connectionString = "Server=localhost;Port=3306;Database=test;User=root;Password=password";
        provider.Initialize(connectionString);

        // Act
        var result1 = provider.GetConnectionString();
        var result2 = provider.GetConnectionString();
        var result3 = provider.GetConnectionString();

        // Assert
        Assert.Equal(connectionString, result1);
        Assert.Equal(connectionString, result2);
        Assert.Equal(connectionString, result3);
    }

    #endregion

    #region Thread Safety Tests

    [Fact]
    public void Initialize_CalledConcurrently_IsThreadSafe()
    {
        // Arrange
        var provider = new DbConnectionStringProvider();
        const int threadCount = 10;
        var tasks = new Task[threadCount];
        var connectionStrings = Enumerable.Range(0, threadCount)
            .Select(i => $"Server=localhost;Port=3306;Database=test{i};User=root;Password=password{i}")
            .ToArray();

        // Act
        for (int i = 0; i < threadCount; i++)
        {
            int index = i;
            tasks[i] = Task.Run(() => provider.Initialize(connectionStrings[index]));
        }

        Task.WaitAll(tasks);

        // Assert - Should not throw and should contain one of the connection strings
        var result = provider.GetConnectionString();
        Assert.NotNull(result);
        Assert.Contains(result, connectionStrings);
    }

    [Fact]
    public void GetConnectionString_CalledConcurrently_IsThreadSafe()
    {
        // Arrange
        var provider = new DbConnectionStringProvider();
        const string connectionString = "Server=localhost;Port=3306;Database=test;User=root;Password=password";
        provider.Initialize(connectionString);

        const int threadCount = 20;
        var tasks = new Task<string>[threadCount];

        // Act
        for (int i = 0; i < threadCount; i++)
        {
            tasks[i] = Task.Run(() => provider.GetConnectionString());
        }

        Task.WaitAll(tasks);

        // Assert - All threads should get the same value
        Assert.All(tasks, task => Assert.Equal(connectionString, task.Result));
    }

    [Fact]
    public void InitializeAndGetConnectionString_CalledConcurrently_IsThreadSafe()
    {
        // Arrange
        var provider = new DbConnectionStringProvider();
        const string initialConnectionString = "Server=localhost;Port=3306;Database=test;User=root;Password=password";
        provider.Initialize(initialConnectionString);

        const int readThreadCount = 10;
        const int writeThreadCount = 5;
        var allTasks = new List<Task>();

        // Act - Mix of reads and writes
        for (int i = 0; i < writeThreadCount; i++)
        {
            int index = i;
            allTasks.Add(Task.Run(() =>
            {
                var newConnectionString = $"Server=localhost;Port=3306;Database=test{index};User=root;Password=password{index}";
                provider.Initialize(newConnectionString);
            }));
        }

        for (int i = 0; i < readThreadCount; i++)
        {
            allTasks.Add(Task.Run(() =>
            {
                var result = provider.GetConnectionString();
                Assert.NotNull(result);
                Assert.NotEmpty(result);
            }));
        }

        // Wait for all operations to complete
        Task.WaitAll(allTasks.ToArray());

        // Assert - Should not throw and should have a valid connection string
        var finalResult = provider.GetConnectionString();
        Assert.NotNull(finalResult);
        Assert.NotEmpty(finalResult);
    }

    #endregion

    #region Edge Cases Tests

    [Fact]
    public void Initialize_WithLongConnectionString_StoresCorrectly()
    {
        // Arrange
        var provider = new DbConnectionStringProvider();
        var longConnectionString = "Server=very-long-hostname-that-could-be-in-aws-rds.c9akciq32.us-east-1.rds.amazonaws.com;" +
                                    "Port=3306;" +
                                    "Database=leadprocessor_production_database;" +
                                    "User=application_user_with_long_name;" +
                                    "Password=very_long_and_complex_password_with_special_chars_!@#$%^&*()";

        // Act
        provider.Initialize(longConnectionString);
        var result = provider.GetConnectionString();

        // Assert
        Assert.Equal(longConnectionString, result);
    }

    [Fact]
    public void Initialize_WithSpecialCharacters_StoresCorrectly()
    {
        // Arrange
        var provider = new DbConnectionStringProvider();
        const string connectionString = "Server=localhost;Port=3306;Database=test;User=root;Password=p@$$w0rd!#;";

        // Act
        provider.Initialize(connectionString);
        var result = provider.GetConnectionString();

        // Assert
        Assert.Equal(connectionString, result);
    }

    [Fact]
    public void Initialize_WithMinimalConnectionString_StoresCorrectly()
    {
        // Arrange
        var provider = new DbConnectionStringProvider();
        const string connectionString = "Server=localhost";

        // Act
        provider.Initialize(connectionString);
        var result = provider.GetConnectionString();

        // Assert
        Assert.Equal(connectionString, result);
    }

    #endregion

    #region Lifecycle Tests

    [Fact]
    public void Provider_InitializeGetReInitializeGet_WorksCorrectly()
    {
        // Arrange
        var provider = new DbConnectionStringProvider();
        const string connectionString1 = "Server=localhost;Port=3306;Database=test1;User=root;Password=password1";
        const string connectionString2 = "Server=localhost;Port=3306;Database=test2;User=root;Password=password2";

        // Act & Assert - First initialization
        provider.Initialize(connectionString1);
        Assert.Equal(connectionString1, provider.GetConnectionString());

        // Act & Assert - Re-initialization
        provider.Initialize(connectionString2);
        Assert.Equal(connectionString2, provider.GetConnectionString());

        // Act & Assert - Verify it stays with the latest value
        Assert.Equal(connectionString2, provider.GetConnectionString());
    }

    [Fact]
    public void MultipleProviders_AreIndependent()
    {
        // Arrange
        var provider1 = new DbConnectionStringProvider();
        var provider2 = new DbConnectionStringProvider();
        const string connectionString1 = "Server=localhost;Port=3306;Database=test1;User=root;Password=password1";
        const string connectionString2 = "Server=localhost;Port=3306;Database=test2;User=root;Password=password2";

        // Act
        provider1.Initialize(connectionString1);
        provider2.Initialize(connectionString2);

        // Assert
        Assert.Equal(connectionString1, provider1.GetConnectionString());
        Assert.Equal(connectionString2, provider2.GetConnectionString());
    }

    #endregion
}

