namespace LeadProcessor.Infrastructure.Services;

/// <summary>
/// Provides access to the database connection string.
/// </summary>
/// <remarks>
/// This interface enables thread-safe access to the database connection string
/// which is initialized during Lambda cold start. The connection string is typically
/// retrieved from AWS Secrets Manager and cached for the lifetime of the Lambda instance.
/// Implementations must be thread-safe.
/// </remarks>
public interface IDbConnectionStringProvider
{
    /// <summary>
    /// Initializes the connection string provider with the specified connection string.
    /// </summary>
    /// <param name="connectionString">The database connection string to store.</param>
    /// <exception cref="ArgumentNullException">Thrown when connectionString is null.</exception>
    /// <exception cref="ArgumentException">Thrown when connectionString is empty or whitespace.</exception>
    /// <remarks>
    /// This method should be called once during Lambda initialization (cold start)
    /// before any database operations are performed. Subsequent calls will overwrite
    /// the existing connection string.
    /// Thread-safe: Can be called from multiple threads simultaneously.
    /// </remarks>
    void Initialize(string connectionString);

    /// <summary>
    /// Retrieves the database connection string.
    /// </summary>
    /// <returns>The database connection string.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the connection string has not been initialized.</exception>
    /// <remarks>
    /// This method is called by the DbContext factory to obtain the connection string
    /// for creating database connections. It must be called after Initialize().
    /// Thread-safe: Can be called from multiple threads simultaneously.
    /// </remarks>
    string GetConnectionString();
}

