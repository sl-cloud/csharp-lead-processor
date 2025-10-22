using LeadProcessor.Infrastructure.Configuration;

namespace LeadProcessor.UnitTests.Infrastructure.Configuration;

/// <summary>
/// Unit tests for <see cref="AwsSettings"/>.
/// </summary>
public sealed class AwsSettingsTests
{
    #region Valid Settings Tests

    [Fact]
    public void ValidSettings_ReturnsNoValidationErrors()
    {
        // Arrange
        var settings = new AwsSettings
        {
            Region = "us-east-1",
            SqsQueueUrl = "https://sqs.us-east-1.amazonaws.com/123456789012/lead-events",
            SqsDlqUrl = "https://sqs.us-east-1.amazonaws.com/123456789012/lead-events-dlq",
            SecretsManagerSecretName = "leadprocessor/rds/credentials",
            RdsEndpoint = "leadprocessor-db.c9akciq32.us-east-1.rds.amazonaws.com"
        };

        // Act
        var errors = settings.Validate().ToList();

        // Assert
        Assert.Empty(errors);
    }

    [Fact]
    public void ValidSettings_WithIamAuthentication_ReturnsNoValidationErrors()
    {
        // Arrange
        var settings = new AwsSettings
        {
            Region = "eu-west-1",
            SqsQueueUrl = "https://sqs.eu-west-1.amazonaws.com/123456789012/lead-events",
            SqsDlqUrl = "https://sqs.eu-west-1.amazonaws.com/123456789012/lead-events-dlq",
            SecretsManagerSecretName = "leadprocessor/rds/credentials",
            RdsEndpoint = "leadprocessor-db.c9akciq32.eu-west-1.rds.amazonaws.com",
            UseIamDatabaseAuthentication = true,
            IamTokenLifetimeSeconds = 600
        };

        // Act
        var errors = settings.Validate().ToList();

        // Assert
        Assert.Empty(errors);
    }

    [Theory]
    [InlineData("us-east-1")]
    [InlineData("eu-west-1")]
    [InlineData("ap-southeast-1")]
    [InlineData("us-west-2")]
    public void ValidSettings_WithDifferentRegions_ReturnsNoValidationErrors(string region)
    {
        // Arrange
        var settings = new AwsSettings
        {
            Region = region,
            SqsQueueUrl = $"https://sqs.{region}.amazonaws.com/123456789012/lead-events",
            SqsDlqUrl = $"https://sqs.{region}.amazonaws.com/123456789012/lead-events-dlq",
            SecretsManagerSecretName = "leadprocessor/rds/credentials",
            RdsEndpoint = $"leadprocessor-db.c9akciq32.{region}.rds.amazonaws.com"
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
    public void MissingRegion_ReturnsValidationError(string invalidValue)
    {
        // Arrange
        var settings = new AwsSettings
        {
            Region = invalidValue,
            SqsQueueUrl = "https://sqs.us-east-1.amazonaws.com/123456789012/lead-events",
            SqsDlqUrl = "https://sqs.us-east-1.amazonaws.com/123456789012/lead-events-dlq",
            SecretsManagerSecretName = "leadprocessor/rds/credentials",
            RdsEndpoint = "leadprocessor-db.c9akciq32.us-east-1.rds.amazonaws.com"
        };

        // Act
        var errors = settings.Validate().ToList();

        // Assert
        Assert.Contains("Region is required", errors);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void MissingSqsQueueUrl_ReturnsValidationError(string invalidValue)
    {
        // Arrange
        var settings = new AwsSettings
        {
            Region = "us-east-1",
            SqsQueueUrl = invalidValue,
            SqsDlqUrl = "https://sqs.us-east-1.amazonaws.com/123456789012/lead-events-dlq",
            SecretsManagerSecretName = "leadprocessor/rds/credentials",
            RdsEndpoint = "leadprocessor-db.c9akciq32.us-east-1.rds.amazonaws.com"
        };

        // Act
        var errors = settings.Validate().ToList();

        // Assert
        Assert.Contains("SqsQueueUrl is required", errors);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void MissingSqsDlqUrl_ReturnsValidationError(string invalidValue)
    {
        // Arrange
        var settings = new AwsSettings
        {
            Region = "us-east-1",
            SqsQueueUrl = "https://sqs.us-east-1.amazonaws.com/123456789012/lead-events",
            SqsDlqUrl = invalidValue,
            SecretsManagerSecretName = "leadprocessor/rds/credentials",
            RdsEndpoint = "leadprocessor-db.c9akciq32.us-east-1.rds.amazonaws.com"
        };

        // Act
        var errors = settings.Validate().ToList();

        // Assert
        Assert.Contains("SqsDlqUrl is required", errors);
    }

    #endregion

    #region Numeric Field Validation Tests

    [Theory]
    [InlineData(-1)]
    [InlineData(-100)]
    public void NegativeMaxRetryAttempts_ReturnsValidationError(int negativeRetries)
    {
        // Arrange
        var settings = new AwsSettings
        {
            Region = "us-east-1",
            SqsQueueUrl = "https://sqs.us-east-1.amazonaws.com/123456789012/lead-events",
            SqsDlqUrl = "https://sqs.us-east-1.amazonaws.com/123456789012/lead-events-dlq",
            SecretsManagerSecretName = "leadprocessor/rds/credentials",
            RdsEndpoint = "leadprocessor-db.c9akciq32.us-east-1.rds.amazonaws.com",
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
    public void InvalidIamTokenLifetimeSeconds_ReturnsValidationError(int invalidLifetime)
    {
        // Arrange
        var settings = new AwsSettings
        {
            Region = "us-east-1",
            SqsQueueUrl = "https://sqs.us-east-1.amazonaws.com/123456789012/lead-events",
            SqsDlqUrl = "https://sqs.us-east-1.amazonaws.com/123456789012/lead-events-dlq",
            SecretsManagerSecretName = "leadprocessor/rds/credentials",
            RdsEndpoint = "leadprocessor-db.c9akciq32.us-east-1.rds.amazonaws.com",
            IamTokenLifetimeSeconds = invalidLifetime
        };

        // Act
        var errors = settings.Validate().ToList();

        // Assert
        Assert.Contains(
            errors,
            e => e.Contains("IamTokenLifetimeSeconds must be > 0"));
    }

    #endregion

    #region URL Format Validation Tests

    [Theory]
    [InlineData("http://sqs.us-east-1.amazonaws.com/123456789012/lead-events")]
    [InlineData("sqs.us-east-1.amazonaws.com/123456789012/lead-events")]
    [InlineData("https://dynamodb.us-east-1.amazonaws.com/123456789012/lead-events")]
    public void InvalidSqsQueueUrl_ReturnsValidationError(string invalidUrl)
    {
        // Arrange
        var settings = new AwsSettings
        {
            Region = "us-east-1",
            SqsQueueUrl = invalidUrl,
            SqsDlqUrl = "https://sqs.us-east-1.amazonaws.com/123456789012/lead-events-dlq",
            SecretsManagerSecretName = "leadprocessor/rds/credentials",
            RdsEndpoint = "leadprocessor-db.c9akciq32.us-east-1.rds.amazonaws.com"
        };

        // Act
        var errors = settings.Validate().ToList();

        // Assert
        Assert.Contains(
            errors,
            e => e.Contains("SqsQueueUrl should be in format"));
    }

    [Theory]
    [InlineData("leadprocessor-db.us-east-1.ec2.amazonaws.com")]
    [InlineData("leadprocessor-db.us-east-1.redshift.amazonaws.com")]
    [InlineData("localhost")]
    public void InvalidRdsEndpoint_ReturnsValidationError(string invalidEndpoint)
    {
        // Arrange
        var settings = new AwsSettings
        {
            Region = "us-east-1",
            SqsQueueUrl = "https://sqs.us-east-1.amazonaws.com/123456789012/lead-events",
            SqsDlqUrl = "https://sqs.us-east-1.amazonaws.com/123456789012/lead-events-dlq",
            SecretsManagerSecretName = "leadprocessor/rds/credentials",
            RdsEndpoint = invalidEndpoint
        };

        // Act
        var errors = settings.Validate().ToList();

        // Assert
        Assert.Contains(
            errors,
            e => e.Contains("RdsEndpoint should be in format"));
    }

    #endregion

    #region Default Values Tests

    [Fact]
    public void NewInstance_HasCorrectDefaultValues()
    {
        // Arrange & Act
        var settings = new AwsSettings
        {
            Region = "us-east-1",
            SqsQueueUrl = "https://sqs.us-east-1.amazonaws.com/123456789012/dummy",
            SqsDlqUrl = "https://sqs.us-east-1.amazonaws.com/123456789012/dummy",
            SecretsManagerSecretName = "dummy",
            RdsEndpoint = "dummy.us-east-1.rds.amazonaws.com"
        };

        // Assert
        Assert.Equal(3, settings.MaxRetryAttempts);
        Assert.False(settings.UseIamDatabaseAuthentication);
        Assert.Equal(900, settings.IamTokenLifetimeSeconds);
        Assert.True(settings.ValidateCredentialsAtStartup);
    }

    #endregion

    #region Edge Cases Tests

    [Fact]
    public void ValidSettings_WithZeroRetries_ReturnsNoValidationError()
    {
        // Arrange
        var settings = new AwsSettings
        {
            Region = "us-east-1",
            SqsQueueUrl = "https://sqs.us-east-1.amazonaws.com/123456789012/lead-events",
            SqsDlqUrl = "https://sqs.us-east-1.amazonaws.com/123456789012/lead-events-dlq",
            SecretsManagerSecretName = "leadprocessor/rds/credentials",
            RdsEndpoint = "leadprocessor-db.c9akciq32.us-east-1.rds.amazonaws.com",
            MaxRetryAttempts = 0
        };

        // Act
        var errors = settings.Validate().ToList();

        // Assert
        Assert.Empty(errors.Where(e => e.Contains("MaxRetryAttempts")));
    }

    [Fact]
    public void ValidSettings_WithMinimalIamTokenLifetime_ReturnsNoValidationError()
    {
        // Arrange
        var settings = new AwsSettings
        {
            Region = "us-east-1",
            SqsQueueUrl = "https://sqs.us-east-1.amazonaws.com/123456789012/lead-events",
            SqsDlqUrl = "https://sqs.us-east-1.amazonaws.com/123456789012/lead-events-dlq",
            SecretsManagerSecretName = "leadprocessor/rds/credentials",
            RdsEndpoint = "leadprocessor-db.c9akciq32.us-east-1.rds.amazonaws.com",
            IamTokenLifetimeSeconds = 1
        };

        // Act
        var errors = settings.Validate().ToList();

        // Assert
        Assert.Empty(errors.Where(e => e.Contains("IamTokenLifetimeSeconds")));
    }

    #endregion
}
