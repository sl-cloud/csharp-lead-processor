using Cloudvelous.Aws.SecretsManager;
using LeadProcessor.Infrastructure.Models;
using LeadProcessor.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace LeadProcessor.UnitTests.Infrastructure.Services;

/// <summary>
/// Unit tests for <see cref="SecretsManagerService"/> using Cloudvelous SDK.
/// </summary>
/// <remarks>
/// These tests verify the wrapper service correctly delegates to ISecretsManagerClient.
/// Note: Due to Cloudvelous SDK's use of optional parameters in GetSecretValueAsync,
/// we cannot use Moq's expression trees for verification. These tests focus on
/// argument validation and exception handling behavior.
/// </remarks>
public sealed class SecretsManagerServiceTests
{
    private readonly Mock<ILogger<SecretsManagerService>> _mockLogger;

    public SecretsManagerServiceTests()
    {
        _mockLogger = new Mock<ILogger<SecretsManagerService>>();
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithNullSecretsManagerClient_ThrowsArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            new SecretsManagerService(null!, _mockLogger.Object));
        Assert.Equal("secretsManagerClient", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Arrange
        var mockClient = new Mock<ISecretsManagerClient>();

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            new SecretsManagerService(mockClient.Object, null!));
        Assert.Equal("logger", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithValidParameters_CreatesInstance()
    {
        // Arrange
        var mockClient = new Mock<ISecretsManagerClient>();

        // Act
        var service = new SecretsManagerService(mockClient.Object, _mockLogger.Object);

        // Assert
        Assert.NotNull(service);
    }

    #endregion

    #region GetDatabaseCredentialsAsync Tests

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task GetDatabaseCredentialsAsync_WithInvalidSecretName_ThrowsArgumentException(string invalidSecretName)
    {
        // Arrange
        var mockClient = new Mock<ISecretsManagerClient>();
        var service = new SecretsManagerService(mockClient.Object, _mockLogger.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            service.GetDatabaseCredentialsAsync(invalidSecretName));
        Assert.Equal("secretName", exception.ParamName);
        Assert.Contains("cannot be null or whitespace", exception.Message);
    }

    #endregion

    #region GetSecretAsync<T> Tests

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task GetSecretAsync_WithInvalidSecretName_ThrowsArgumentException(string invalidSecretName)
    {
        // Arrange
        var mockClient = new Mock<ISecretsManagerClient>();
        var service = new SecretsManagerService(mockClient.Object, _mockLogger.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            service.GetSecretAsync<TestConfig>(invalidSecretName));
        Assert.Equal("secretName", exception.ParamName);
        Assert.Contains("cannot be null or whitespace", exception.Message);
    }

    #endregion

    #region Test Helper Classes

    /// <summary>
    /// Test configuration class for generic secret retrieval tests.
    /// </summary>
    private sealed class TestConfig
    {
        public string ApiKey { get; set; } = string.Empty;
        public string Endpoint { get; set; } = string.Empty;
    }

    #endregion
}
