namespace LeadProcessor.Infrastructure.Configuration;

/// <summary>
/// Configuration settings for AWS services (SQS, SecretsManager, RDS).
/// </summary>
/// <remarks>
/// This class is designed to be bound from configuration via the IOptions pattern.
/// Use in Startup/Program.cs:
/// <code>
/// services.Configure&lt;AwsSettings&gt;(configuration.GetSection("AWS"));
/// </code>
/// All string properties use the 'required' modifier to enforce configuration at startup.
/// Credentials should be managed via IAM roles (for Lambda) or environment variables (for local development).
/// </remarks>
public sealed class AwsSettings
{
    /// <summary>
    /// Gets or sets the AWS region for all services.
    /// </summary>
    /// <remarks>
    /// Examples: "us-east-1", "eu-west-1", "ap-southeast-1"
    /// Should match the region where RDS, SQS, and Secrets Manager resources are provisioned.
    /// </remarks>
    public required string Region { get; set; }

    /// <summary>
    /// Gets or sets the SQS queue URL for receiving messages.
    /// </summary>
    /// <remarks>
    /// Format: "https://sqs.{region}.amazonaws.com/{account-id}/{queue-name}"
    /// Used by the Lambda function handler to process incoming lead events.
    /// </remarks>
    public required string SqsQueueUrl { get; set; }

    /// <summary>
    /// Gets or sets the SQS Dead Letter Queue (DLQ) URL for failed messages.
    /// </summary>
    /// <remarks>
    /// Format: "https://sqs.{region}.amazonaws.com/{account-id}/{dlq-name}"
    /// Messages that fail after MaxRetryAttempts are moved to this queue.
    /// </remarks>
    public required string SqsDlqUrl { get; set; }

    /// <summary>
    /// Gets or sets the AWS Secrets Manager secret name for database credentials.
    /// </summary>
    /// <remarks>
    /// Should contain JSON with keys: Server, Port, Database, User, Password
    /// Example: "leadprocessor/rds/credentials"
    /// </remarks>
    public required string SecretsManagerSecretName { get; set; }

    /// <summary>
    /// Gets or sets the RDS cluster/instance endpoint.
    /// </summary>
    /// <remarks>
    /// Format: "leadprocessor-db.c9akciq32.{region}.rds.amazonaws.com"
    /// Used to construct the database connection string.
    /// </remarks>
    public required string RdsEndpoint { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of retry attempts for AWS SDK operations.
    /// </summary>
    /// <remarks>
    /// Default is 3 retries for transient AWS errors.
    /// Set to 0 to disable retries.
    /// </remarks>
    public int MaxRetryAttempts { get; set; } = 3;

    /// <summary>
    /// Gets or sets a value indicating whether to use IAM authentication for RDS.
    /// </summary>
    /// <remarks>
    /// When enabled (true), uses temporary security credentials from IAM role instead of stored password.
    /// Recommended for production Lambda deployments.
    /// Default is false for simplicity in development.
    /// </remarks>
    public bool UseIamDatabaseAuthentication { get; set; } = false;

    /// <summary>
    /// Gets or sets the IAM database authentication token lifetime in seconds.
    /// </summary>
    /// <remarks>
    /// Default is 900 seconds (15 minutes).
    /// Tokens are cached and reused within their lifetime to reduce API calls.
    /// </remarks>
    public int IamTokenLifetimeSeconds { get; set; } = 900;

    /// <summary>
    /// Gets or sets a value indicating whether to validate AWS credentials at startup.
    /// </summary>
    /// <remarks>
    /// When enabled, performs a test call to verify AWS credentials are valid.
    /// Useful for catching configuration issues early in the Lambda initialization phase.
    /// </remarks>
    public bool ValidateCredentialsAtStartup { get; set; } = true;

    /// <summary>
    /// Validates the configuration settings.
    /// </summary>
    /// <returns>A list of validation errors, empty if valid.</returns>
    /// <remarks>
    /// Called during startup to ensure all required settings are present and valid.
    /// </remarks>
    public IEnumerable<string> Validate()
    {
        if (string.IsNullOrWhiteSpace(Region))
            yield return "Region is required";

        if (string.IsNullOrWhiteSpace(SqsQueueUrl))
            yield return "SqsQueueUrl is required";

        if (string.IsNullOrWhiteSpace(SqsDlqUrl))
            yield return "SqsDlqUrl is required";

        if (string.IsNullOrWhiteSpace(SecretsManagerSecretName))
            yield return "SecretsManagerSecretName is required";

        if (string.IsNullOrWhiteSpace(RdsEndpoint))
            yield return "RdsEndpoint is required";

        if (MaxRetryAttempts < 0)
            yield return $"MaxRetryAttempts must be >= 0, got {MaxRetryAttempts}";

        if (IamTokenLifetimeSeconds <= 0)
            yield return $"IamTokenLifetimeSeconds must be > 0, got {IamTokenLifetimeSeconds}";

        // Validate SQS URLs format (only if not null/empty)
        if (!string.IsNullOrWhiteSpace(SqsQueueUrl) && !SqsQueueUrl.StartsWith("https://sqs."))
            yield return "SqsQueueUrl should be in format: https://sqs.{region}.amazonaws.com/{account-id}/{queue-name}";

        if (!string.IsNullOrWhiteSpace(SqsDlqUrl) && !SqsDlqUrl.StartsWith("https://sqs."))
            yield return "SqsDlqUrl should be in format: https://sqs.{region}.amazonaws.com/{account-id}/{dlq-name}";

        // Validate RDS endpoint format (only if not null/empty)
        if (!string.IsNullOrWhiteSpace(RdsEndpoint) && !RdsEndpoint.Contains(".rds.amazonaws.com"))
            yield return "RdsEndpoint should be in format: {identifier}.{region}.rds.amazonaws.com";
    }
}
