namespace LeadProcessor.Infrastructure.Configuration;

/// <summary>
/// Configuration settings for database connections and EF Core behavior.
/// </summary>
/// <remarks>
/// This class is designed to be bound from configuration via the IOptions pattern.
/// Use in Startup/Program.cs:
/// <code>
/// services.Configure&lt;DatabaseSettings&gt;(configuration.GetSection("Database"));
/// </code>
/// All string properties use the 'required' modifier to enforce configuration at startup.
/// </remarks>
public sealed class DatabaseSettings
{
    /// <summary>
    /// Gets or sets the database connection string for RDS MySQL database.
    /// </summary>
    /// <remarks>
    /// Should be in format: "Server=hostname;Port=3306;Database=dbname;User=user;Password=password"
    /// Can be loaded from AWS Secrets Manager or environment-specific appsettings.json
    /// </remarks>
    public required string ConnectionString { get; set; }

    /// <summary>
    /// Gets or sets the database server hostname or endpoint.
    /// </summary>
    /// <remarks>
    /// For RDS: format is typically "leadprocessor-db.c9akciq32.us-east-1.rds.amazonaws.com"
    /// For local development: "localhost"
    /// </remarks>
    public required string Server { get; set; }

    /// <summary>
    /// Gets or sets the database server port.
    /// </summary>
    /// <remarks>
    /// Default MySQL port is 3306.
    /// </remarks>
    public int Port { get; set; } = 3306;

    /// <summary>
    /// Gets or sets the database name.
    /// </summary>
    /// <remarks>
    /// For this application: "leadprocessor" or similar.
    /// </remarks>
    public required string Database { get; set; }

    /// <summary>
    /// Gets or sets the database user for authentication.
    /// </summary>
    public required string User { get; set; }

    /// <summary>
    /// Gets or sets the database password for authentication.
    /// </summary>
    /// <remarks>
    /// In production, should be loaded from AWS Secrets Manager, not hardcoded or in appsettings.
    /// </remarks>
    public required string Password { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of retry attempts for transient database failures.
    /// </summary>
    /// <remarks>
    /// Default is 3 retries for transient failures (connection timeouts, deadlocks, etc.).
    /// Set to 0 to disable retries.
    /// </remarks>
    public int MaxRetryAttempts { get; set; } = 3;

    /// <summary>
    /// Gets or sets the maximum delay between retry attempts in seconds.
    /// </summary>
    /// <remarks>
    /// Default is 10 seconds. Actual delay is randomized between 0 and this value.
    /// </remarks>
    public int MaxRetryDelaySeconds { get; set; } = 10;

    /// <summary>
    /// Gets or sets the command timeout in seconds for database operations.
    /// </summary>
    /// <remarks>
    /// Default is 30 seconds. Increase for long-running queries or bulk operations.
    /// </remarks>
    public int CommandTimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Gets or sets a value indicating whether to enable detailed error logging.
    /// </summary>
    /// <remarks>
    /// When enabled, provides more detailed diagnostics about database operations.
    /// Should be disabled in production for security and performance.
    /// </remarks>
    public bool EnableDetailedErrors { get; set; } = false;

    /// <summary>
    /// Gets or sets a value indicating whether to enable EF Core query logging.
    /// </summary>
    /// <remarks>
    /// When enabled, logs all generated SQL queries. Should only be used for debugging.
    /// Can impact performance and expose sensitive data in logs.
    /// </remarks>
    public bool EnableQueryLogging { get; set; } = false;

    /// <summary>
    /// Validates the configuration settings.
    /// </summary>
    /// <returns>A list of validation errors, empty if valid.</returns>
    /// <remarks>
    /// Called during startup to ensure all required settings are present and valid.
    /// </remarks>
    public IEnumerable<string> Validate()
    {
        if (string.IsNullOrWhiteSpace(ConnectionString))
            yield return "ConnectionString is required";

        if (string.IsNullOrWhiteSpace(Server))
            yield return "Server is required";

        if (Port <= 0 || Port > 65535)
            yield return $"Port must be between 1 and 65535, got {Port}";

        if (string.IsNullOrWhiteSpace(Database))
            yield return "Database is required";

        if (string.IsNullOrWhiteSpace(User))
            yield return "User is required";

        if (string.IsNullOrWhiteSpace(Password))
            yield return "Password is required";

        if (MaxRetryAttempts < 0)
            yield return $"MaxRetryAttempts must be >= 0, got {MaxRetryAttempts}";

        if (MaxRetryDelaySeconds <= 0)
            yield return $"MaxRetryDelaySeconds must be > 0, got {MaxRetryDelaySeconds}";

        if (CommandTimeoutSeconds <= 0)
            yield return $"CommandTimeoutSeconds must be > 0, got {CommandTimeoutSeconds}";
    }
}
