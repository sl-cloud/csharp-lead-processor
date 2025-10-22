using LeadProcessor.Infrastructure.Configuration;

namespace LeadProcessor.UnitTests.Infrastructure.Configuration;

/// <summary>
/// Unit tests for <see cref="DatabaseSettings"/>.
/// </summary>
public sealed class DatabaseSettingsTests
{
    #region Valid Settings Tests

    [Fact]
    public void ValidSettings_ReturnsNoValidationErrors()
    {
        // Arrange
        var settings = new DatabaseSettings
        {
            ConnectionString = "Server=localhost;Port=3306;Database=leadprocessor;User=root;Password=password",
            Server = "localhost",
            Port = 3306,
            Database = "leadprocessor",
            User = "root",
            Password = "password"
        };

        // Act
        var errors = settings.Validate().ToList();

        // Assert
        Assert.Empty(errors);
    }

    [Fact]
    public void ValidSettings_WithMaxValues_ReturnsNoValidationErrors()
    {
        // Arrange
        var settings = new DatabaseSettings
        {
            ConnectionString = "Server=db.example.com;Port=65535;Database=leadprocessor;User=root;Password=password",
            Server = "db.example.com",
            Port = 65535,
            Database = "leadprocessor",
            User = "root",
            Password = "password",
            MaxRetryAttempts = 10,
            MaxRetryDelaySeconds = 300,
            CommandTimeoutSeconds = 600,
            EnableDetailedErrors = true,
            EnableQueryLogging = true
        };

        // Act
        var errors = settings.Validate().ToList();

        // Assert
        Assert.Empty(errors);
    }

    #endregion

    #region Required Field Validation Tests

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("   ")]
    public void MissingConnectionString_ReturnsValidationError(string invalidValue)
    {
        // Arrange
        var settings = new DatabaseSettings
        {
            ConnectionString = invalidValue,
            Server = "localhost",
            Port = 3306,
            Database = "leadprocessor",
            User = "root",
            Password = "password"
        };

        // Act
        var errors = settings.Validate().ToList();

        // Assert
        Assert.Contains("ConnectionString is required", errors);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void MissingServer_ReturnsValidationError(string invalidValue)
    {
        // Arrange
        var settings = new DatabaseSettings
        {
            ConnectionString = "dummy",
            Server = invalidValue,
            Database = "leadprocessor",
            User = "root",
            Password = "password"
        };

        // Act
        var errors = settings.Validate().ToList();

        // Assert
        Assert.Contains("Server is required", errors);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void MissingDatabase_ReturnsValidationError(string invalidValue)
    {
        // Arrange
        var settings = new DatabaseSettings
        {
            ConnectionString = "dummy",
            Server = "localhost",
            Database = invalidValue,
            User = "root",
            Password = "password"
        };

        // Act
        var errors = settings.Validate().ToList();

        // Assert
        Assert.Contains("Database is required", errors);
    }

    #endregion

    #region Port Validation Tests

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(65536)]
    [InlineData(100000)]
    public void InvalidPort_ReturnsValidationError(int invalidPort)
    {
        // Arrange
        var settings = new DatabaseSettings
        {
            ConnectionString = "dummy",
            Server = "localhost",
            Database = "leadprocessor",
            User = "root",
            Password = "password",
            Port = invalidPort
        };

        // Act
        var errors = settings.Validate().ToList();

        // Assert
        Assert.Contains(
            errors,
            e => e.Contains("Port must be between 1 and 65535"));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(3306)]
    [InlineData(5432)]
    [InlineData(65535)]
    public void ValidPort_ReturnsNoError(int validPort)
    {
        // Arrange
        var settings = new DatabaseSettings
        {
            ConnectionString = "dummy",
            Server = "localhost",
            Database = "leadprocessor",
            User = "root",
            Password = "password",
            Port = validPort
        };

        // Act
        var errors = settings.Validate().ToList();

        // Assert
        Assert.Empty(errors.Where(e => e.Contains("Port")));
    }

    #endregion

    #region Retry Settings Validation Tests

    [Theory]
    [InlineData(-1)]
    [InlineData(-100)]
    public void NegativeMaxRetryAttempts_ReturnsValidationError(int negativeRetries)
    {
        // Arrange
        var settings = new DatabaseSettings
        {
            ConnectionString = "dummy",
            Server = "localhost",
            Database = "leadprocessor",
            User = "root",
            Password = "password",
            MaxRetryAttempts = negativeRetries
        };

        // Act
        var errors = settings.Validate().ToList();

        // Assert
        Assert.Contains(
            errors,
            e => e.Contains("MaxRetryAttempts must be >= 0"));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void InvalidMaxRetryDelaySeconds_ReturnsValidationError(int invalidDelay)
    {
        // Arrange
        var settings = new DatabaseSettings
        {
            ConnectionString = "dummy",
            Server = "localhost",
            Database = "leadprocessor",
            User = "root",
            Password = "password",
            MaxRetryDelaySeconds = invalidDelay
        };

        // Act
        var errors = settings.Validate().ToList();

        // Assert
        Assert.Contains(
            errors,
            e => e.Contains("MaxRetryDelaySeconds must be > 0"));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public void InvalidCommandTimeoutSeconds_ReturnsValidationError(int invalidTimeout)
    {
        // Arrange
        var settings = new DatabaseSettings
        {
            ConnectionString = "dummy",
            Server = "localhost",
            Database = "leadprocessor",
            User = "root",
            Password = "password",
            CommandTimeoutSeconds = invalidTimeout
        };

        // Act
        var errors = settings.Validate().ToList();

        // Assert
        Assert.Contains(
            errors,
            e => e.Contains("CommandTimeoutSeconds must be > 0"));
    }

    #endregion

    #region Default Values Tests

    [Fact]
    public void NewInstance_HasCorrectDefaultValues()
    {
        // Arrange & Act
        var settings = new DatabaseSettings
        {
            ConnectionString = "dummy",
            Server = "dummy",
            Database = "dummy",
            User = "dummy",
            Password = "dummy"
        };

        // Assert
        Assert.Equal(3306, settings.Port);
        Assert.Equal(3, settings.MaxRetryAttempts);
        Assert.Equal(10, settings.MaxRetryDelaySeconds);
        Assert.Equal(30, settings.CommandTimeoutSeconds);
        Assert.False(settings.EnableDetailedErrors);
        Assert.False(settings.EnableQueryLogging);
    }

    #endregion

    #region Edge Cases Tests

    [Fact]
    public void ValidSettings_WithZeroRetries_ReturnsNoValidationError()
    {
        // Arrange
        var settings = new DatabaseSettings
        {
            ConnectionString = "dummy",
            Server = "localhost",
            Database = "leadprocessor",
            User = "root",
            Password = "password",
            MaxRetryAttempts = 0
        };

        // Act
        var errors = settings.Validate().ToList();

        // Assert
        Assert.Empty(errors.Where(e => e.Contains("MaxRetryAttempts")));
    }

    [Fact]
    public void ValidSettings_WithMinimumPort_ReturnsNoValidationError()
    {
        // Arrange
        var settings = new DatabaseSettings
        {
            ConnectionString = "dummy",
            Server = "localhost",
            Database = "leadprocessor",
            User = "root",
            Password = "password",
            Port = 1
        };

        // Act
        var errors = settings.Validate().ToList();

        // Assert
        Assert.Empty(errors.Where(e => e.Contains("Port")));
    }

    #endregion
}
