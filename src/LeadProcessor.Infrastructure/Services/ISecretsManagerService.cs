namespace LeadProcessor.Infrastructure.Services;

using LeadProcessor.Infrastructure.Models;

/// <summary>
/// Provides access to secrets stored in AWS Secrets Manager.
/// </summary>
/// <remarks>
/// This interface wraps the Cloudvelous.Aws.SecretsManager SDK, providing:
/// - Automatic caching (60 minutes default, configurable)
/// - Built-in retry policies with exponential backoff
/// - Type-safe secret deserialization
/// - Structured logging integration
/// All implementations are thread-safe and designed for Lambda cold-start optimization.
/// </remarks>
public interface ISecretsManagerService
{
    /// <summary>
    /// Retrieves database credentials from AWS Secrets Manager.
    /// </summary>
    /// <param name="secretName">The name or ARN of the secret containing database credentials.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the database credentials.</returns>
    /// <exception cref="ArgumentException">Thrown when secretName is null or whitespace.</exception>
    /// <exception cref="Cloudvelous.Aws.SecretsManager.Exceptions.SecretNotFoundException">Thrown when the secret is not found.</exception>
    /// <exception cref="System.Text.Json.JsonException">Thrown when the secret value cannot be deserialized.</exception>
    /// <remarks>
    /// The secret value must be a JSON string with the following structure:
    /// <code>
    /// {
    ///   "host": "database-endpoint.region.rds.amazonaws.com",
    ///   "port": 3306,
    ///   "database": "dbname",
    ///   "username": "admin",
    ///   "password": "secret",
    ///   "engine": "mysql"
    /// }
    /// </code>
    /// Cloudvelous SDK handles caching automatically (default 60 minutes).
    /// Thread-safe: Can be called from multiple threads simultaneously.
    /// </remarks>
    Task<DatabaseCredentials> GetDatabaseCredentialsAsync(string secretName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a typed secret value from AWS Secrets Manager.
    /// </summary>
    /// <typeparam name="T">The type to deserialize the secret value into.</typeparam>
    /// <param name="secretName">The name or ARN of the secret to retrieve.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the deserialized secret value.</returns>
    /// <exception cref="ArgumentException">Thrown when secretName is null or whitespace.</exception>
    /// <exception cref="Cloudvelous.Aws.SecretsManager.Exceptions.SecretNotFoundException">Thrown when the secret is not found.</exception>
    /// <exception cref="System.Text.Json.JsonException">Thrown when the secret value cannot be deserialized.</exception>
    /// <remarks>
    /// This method uses Cloudvelous SDK's built-in type-safe deserialization.
    /// Cloudvelous SDK handles caching automatically (default 60 minutes).
    /// Thread-safe: Can be called from multiple threads simultaneously.
    /// </remarks>
    Task<T> GetSecretAsync<T>(string secretName, CancellationToken cancellationToken = default) where T : class;
}

