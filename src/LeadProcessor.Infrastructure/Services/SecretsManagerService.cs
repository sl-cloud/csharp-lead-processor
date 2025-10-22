namespace LeadProcessor.Infrastructure.Services;

using Cloudvelous.Aws.SecretsManager;
using LeadProcessor.Infrastructure.Models;
using Microsoft.Extensions.Logging;

/// <summary>
/// AWS Secrets Manager service using Cloudvelous SDK with built-in caching and retry policies.
/// </summary>
/// <remarks>
/// This service wraps the Cloudvelous.Aws.SecretsManager SDK, which provides:
/// - Automatic caching with configurable TTL (default 60 minutes)
/// - Built-in retry policies using Polly (3 retries with exponential backoff)
/// - Type-safe deserialization with System.Text.Json
/// - Thread-safe operations using concurrent collections
/// All caching, retry, and resilience logic is handled by the Cloudvelous SDK.
/// </remarks>
/// <param name="secretsManagerClient">The Cloudvelous Secrets Manager client.</param>
/// <param name="logger">The logger for diagnostic information.</param>
public sealed class SecretsManagerService(
    ISecretsManagerClient secretsManagerClient,
    ILogger<SecretsManagerService> logger) : ISecretsManagerService
{
    private readonly ISecretsManagerClient _secretsManagerClient = secretsManagerClient ?? throw new ArgumentNullException(nameof(secretsManagerClient));
    private readonly ILogger<SecretsManagerService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    /// <summary>
    /// Retrieves database credentials from AWS Secrets Manager.
    /// </summary>
    /// <param name="secretName">The name or ARN of the secret containing database credentials.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the database credentials.</returns>
    /// <exception cref="ArgumentException">Thrown when secretName is null or whitespace.</exception>
    /// <remarks>
    /// Cloudvelous SDK automatically:
    /// - Caches the secret for 60 minutes (configurable)
    /// - Retries failed requests with exponential backoff
    /// - Deserializes JSON to strongly-typed objects
    /// - Handles thread-safety with concurrent collections
    /// </remarks>
    public async Task<DatabaseCredentials> GetDatabaseCredentialsAsync(string secretName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(secretName))
        {
            throw new ArgumentException("Secret name cannot be null or whitespace.", nameof(secretName));
        }

        _logger.LogDebug("Retrieving database credentials for secret: {SecretName}", secretName);

        try
        {
            // Cloudvelous SDK handles caching, retries, and deserialization automatically
            var credentials = await _secretsManagerClient.GetSecretValueAsync<DatabaseCredentials>(secretName);

            _logger.LogInformation("Successfully retrieved database credentials for secret: {SecretName}", secretName);
            return credentials!;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve database credentials for secret: {SecretName}", secretName);
            throw;
        }
    }

    /// <summary>
    /// Retrieves a typed secret value from AWS Secrets Manager.
    /// </summary>
    /// <typeparam name="T">The type to deserialize the secret value into.</typeparam>
    /// <param name="secretName">The name or ARN of the secret to retrieve.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the deserialized secret value.</returns>
    /// <exception cref="ArgumentException">Thrown when secretName is null or whitespace.</exception>
    /// <remarks>
    /// Cloudvelous SDK automatically:
    /// - Caches the secret for 60 minutes (configurable)
    /// - Retries failed requests with exponential backoff
    /// - Deserializes JSON to strongly-typed objects
    /// - Handles thread-safety with concurrent collections
    /// </remarks>
    public async Task<T> GetSecretAsync<T>(string secretName, CancellationToken cancellationToken = default) where T : class
    {
        if (string.IsNullOrWhiteSpace(secretName))
        {
            throw new ArgumentException("Secret name cannot be null or whitespace.", nameof(secretName));
        }

        _logger.LogDebug("Retrieving secret: {SecretName}", secretName);

        try
        {
            // Cloudvelous SDK handles caching, retries, and deserialization automatically
            var secret = await _secretsManagerClient.GetSecretValueAsync<T>(secretName);

            _logger.LogInformation("Successfully retrieved secret: {SecretName}", secretName);
            return secret!;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve secret: {SecretName}", secretName);
            throw;
        }
    }
}

