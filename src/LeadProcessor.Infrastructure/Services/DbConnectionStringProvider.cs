namespace LeadProcessor.Infrastructure.Services;

/// <summary>
/// Thread-safe implementation of <see cref="IDbConnectionStringProvider"/> that caches the database connection string.
/// </summary>
/// <remarks>
/// This class is designed to be registered as a singleton in the dependency injection container.
/// The connection string is initialized during Lambda cold start and cached for the lifetime
/// of the Lambda instance, avoiding repeated calls to AWS Secrets Manager.
/// All methods are thread-safe and can be called concurrently from multiple threads.
/// </remarks>
public sealed class DbConnectionStringProvider : IDbConnectionStringProvider
{
    private string? _connectionString;
    private readonly object _lock = new();

    /// <summary>
    /// Initializes the connection string provider with the specified connection string.
    /// </summary>
    /// <param name="connectionString">The database connection string to store.</param>
    /// <exception cref="ArgumentNullException">Thrown when connectionString is null.</exception>
    /// <exception cref="ArgumentException">Thrown when connectionString is empty or whitespace.</exception>
    /// <remarks>
    /// This method is thread-safe and can be called multiple times. Each call will overwrite
    /// the previously stored connection string. In a typical Lambda scenario, this is called
    /// once during cold start initialization.
    /// </remarks>
    public void Initialize(string connectionString)
    {
        if (connectionString == null)
        {
            throw new ArgumentNullException(nameof(connectionString));
        }

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new ArgumentException("Connection string cannot be empty or whitespace.", nameof(connectionString));
        }

        lock (_lock)
        {
            _connectionString = connectionString;
        }
    }

    /// <summary>
    /// Retrieves the database connection string.
    /// </summary>
    /// <returns>The database connection string that was previously initialized.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the connection string has not been initialized via <see cref="Initialize"/>.
    /// </exception>
    /// <remarks>
    /// This method is thread-safe and can be called concurrently from multiple threads.
    /// It will always return the most recently initialized connection string.
    /// </remarks>
    public string GetConnectionString()
    {
        lock (_lock)
        {
            return _connectionString ?? throw new InvalidOperationException(
                "Connection string not initialized. Call Initialize() before GetConnectionString().");
        }
    }
}

