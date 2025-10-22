namespace LeadProcessor.Infrastructure.Models;

/// <summary>
/// Represents database credentials retrieved from AWS Secrets Manager.
/// </summary>
/// <remarks>
/// This immutable record type ensures credentials are not modified after retrieval.
/// Used to construct database connection strings from AWS Secrets Manager secrets.
/// </remarks>
public sealed record DatabaseCredentials
{
    /// <summary>
    /// Gets the database server hostname or endpoint.
    /// </summary>
    /// <remarks>
    /// For RDS: typically in format "leadprocessor-db.c9akciq32.us-east-1.rds.amazonaws.com"
    /// For local development: "localhost"
    /// </remarks>
    public required string Host { get; init; }

    /// <summary>
    /// Gets the database server port.
    /// </summary>
    /// <remarks>
    /// Default MySQL port is 3306, PostgreSQL is 5432.
    /// </remarks>
    public required int Port { get; init; }

    /// <summary>
    /// Gets the database name.
    /// </summary>
    public required string Database { get; init; }

    /// <summary>
    /// Gets the database username for authentication.
    /// </summary>
    public required string Username { get; init; }

    /// <summary>
    /// Gets the database password for authentication.
    /// </summary>
    /// <remarks>
    /// This value should be kept secure and not logged.
    /// In production, retrieved from AWS Secrets Manager with automatic rotation support.
    /// </remarks>
    public required string Password { get; init; }

    /// <summary>
    /// Gets the database engine type (e.g., "mysql", "postgres").
    /// </summary>
    /// <remarks>
    /// Optional field that can be used for engine-specific connection string formatting.
    /// </remarks>
    public string? Engine { get; init; }
}

