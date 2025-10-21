using FluentValidation.TestHelper;
using LeadProcessor.Application.Commands;
using LeadProcessor.Application.Validators;

namespace LeadProcessor.UnitTests.Application.Validators;

/// <summary>
/// Unit tests for ProcessLeadCommandValidator.
/// </summary>
public class ProcessLeadCommandValidatorTests
{
    private readonly ProcessLeadCommandValidator _validator;

    public ProcessLeadCommandValidatorTests()
    {
        _validator = new ProcessLeadCommandValidator();
    }

    #region TenantId Validation Tests

    [Fact]
    public void Validate_WithEmptyTenantId_ShouldHaveValidationError()
    {
        // Arrange
        var command = CreateValidCommand() with { TenantId = string.Empty };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.TenantId)
            .WithErrorMessage("TenantId is required");
    }

    [Fact]
    public void Validate_WithTooLongTenantId_ShouldHaveValidationError()
    {
        // Arrange
        var command = CreateValidCommand() with { TenantId = new string('a', 101) };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.TenantId)
            .WithErrorMessage("TenantId must not exceed 100 characters");
    }

    [Fact]
    public void Validate_WithValidTenantId_ShouldNotHaveValidationError()
    {
        // Arrange
        var command = CreateValidCommand() with { TenantId = "tenant-123" };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.TenantId);
    }

    #endregion

    #region CorrelationId Validation Tests

    [Fact]
    public void Validate_WithEmptyCorrelationId_ShouldHaveValidationError()
    {
        // Arrange
        var command = CreateValidCommand() with { CorrelationId = string.Empty };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.CorrelationId)
            .WithErrorMessage("CorrelationId is required");
    }

    [Theory]
    [InlineData("not-a-guid")]
    [InlineData("123456")]
    [InlineData("invalid")]
    [InlineData("00000000-0000-0000-0000-00000000000G")]
    public void Validate_WithInvalidGuidCorrelationId_ShouldHaveValidationError(string invalidGuid)
    {
        // Arrange
        var command = CreateValidCommand() with { CorrelationId = invalidGuid };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.CorrelationId)
            .WithErrorMessage("CorrelationId must be a valid GUID");
    }

    [Fact]
    public void Validate_WithValidGuidCorrelationId_ShouldNotHaveValidationError()
    {
        // Arrange
        var command = CreateValidCommand() with { CorrelationId = Guid.NewGuid().ToString() };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.CorrelationId);
    }

    #endregion

    #region Email Validation Tests

    [Fact]
    public void Validate_WithEmptyEmail_ShouldHaveValidationError()
    {
        // Arrange
        var command = CreateValidCommand() with { Email = string.Empty };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage("Email is required");
    }

    [Theory]
    [InlineData("invalid-email")]
    [InlineData("@example.com")]
    [InlineData("user@")]
    public void Validate_WithInvalidEmail_ShouldHaveValidationError(string invalidEmail)
    {
        // Arrange
        var command = CreateValidCommand() with { Email = invalidEmail };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage("Email must be a valid email address");
    }

    [Theory]
    [InlineData("user@example.com")]
    [InlineData("test.user+tag@example.co.uk")]
    [InlineData("user_name@example-domain.com")]
    [InlineData("123@test.com")]
    public void Validate_WithValidEmail_ShouldNotHaveValidationError(string validEmail)
    {
        // Arrange
        var command = CreateValidCommand() with { Email = validEmail };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Validate_WithTooLongEmail_ShouldHaveValidationError()
    {
        // Arrange
        var longEmail = new string('a', 250) + "@test.com"; // Over 254 chars
        var command = CreateValidCommand() with { Email = longEmail };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage("Email must not exceed 254 characters");
    }

    #endregion

    #region FirstName and LastName Validation Tests

    [Fact]
    public void Validate_WithTooLongFirstName_ShouldHaveValidationError()
    {
        // Arrange
        var command = CreateValidCommand() with { FirstName = new string('a', 101) };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.FirstName)
            .WithErrorMessage("FirstName must not exceed 100 characters");
    }

    [Fact]
    public void Validate_WithTooLongLastName_ShouldHaveValidationError()
    {
        // Arrange
        var command = CreateValidCommand() with { LastName = new string('a', 101) };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.LastName)
            .WithErrorMessage("LastName must not exceed 100 characters");
    }

    [Fact]
    public void Validate_WithValidNames_ShouldNotHaveValidationError()
    {
        // Arrange
        var command = CreateValidCommand() with { FirstName = "John", LastName = "Doe" };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.FirstName);
        result.ShouldNotHaveValidationErrorFor(x => x.LastName);
    }

    [Fact]
    public void Validate_WithNullNames_ShouldNotHaveValidationError()
    {
        // Arrange
        var command = CreateValidCommand() with { FirstName = null, LastName = null };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.FirstName);
        result.ShouldNotHaveValidationErrorFor(x => x.LastName);
    }

    #endregion

    #region Phone Validation Tests

    [Theory]
    [InlineData("+1-555-1234")]
    [InlineData("555-1234")]
    [InlineData("+44 20 7123 4567")]
    [InlineData("(555) 123-4567")]
    [InlineData("+1 (555) 123-4567")]
    [InlineData("1234567")]
    [InlineData("+61 2 1234 5678")]
    public void Validate_WithValidPhoneNumber_ShouldNotHaveValidationError(string validPhone)
    {
        // Arrange
        var command = CreateValidCommand() with { Phone = validPhone };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Phone);
    }

    [Theory]
    [InlineData("abc-defg")]
    [InlineData("phone#123")]
    [InlineData("123")]
    [InlineData("12345")]
    public void Validate_WithInvalidPhoneNumber_ShouldHaveValidationError(string invalidPhone)
    {
        // Arrange
        var command = CreateValidCommand() with { Phone = invalidPhone };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Phone)
            .WithErrorMessage("Phone must be a valid phone number format");
    }

    [Fact]
    public void Validate_WithTooLongPhone_ShouldHaveValidationError()
    {
        // Arrange
        var command = CreateValidCommand() with { Phone = new string('1', 21) };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Phone)
            .WithErrorMessage("Phone must not exceed 20 characters");
    }

    [Fact]
    public void Validate_WithNullPhone_ShouldNotHaveValidationError()
    {
        // Arrange
        var command = CreateValidCommand() with { Phone = null };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Phone);
    }

    #endregion

    #region Company Validation Tests

    [Fact]
    public void Validate_WithTooLongCompany_ShouldHaveValidationError()
    {
        // Arrange
        var command = CreateValidCommand() with { Company = new string('a', 201) };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Company)
            .WithErrorMessage("Company must not exceed 200 characters");
    }

    [Fact]
    public void Validate_WithValidCompany_ShouldNotHaveValidationError()
    {
        // Arrange
        var command = CreateValidCommand() with { Company = "Acme Corp" };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Company);
    }

    #endregion

    #region Source Validation Tests

    [Fact]
    public void Validate_WithEmptySource_ShouldHaveValidationError()
    {
        // Arrange
        var command = CreateValidCommand() with { Source = string.Empty };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Source)
            .WithErrorMessage("Source is required");
    }

    [Fact]
    public void Validate_WithTooLongSource_ShouldHaveValidationError()
    {
        // Arrange
        var command = CreateValidCommand() with { Source = new string('a', 51) };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Source)
            .WithErrorMessage("Source must not exceed 50 characters");
    }

    [Fact]
    public void Validate_WithValidSource_ShouldNotHaveValidationError()
    {
        // Arrange
        var command = CreateValidCommand() with { Source = "website" };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Source);
    }

    #endregion

    #region Metadata Validation Tests

    [Fact]
    public void Validate_WithValidJsonMetadata_ShouldNotHaveValidationError()
    {
        // Arrange
        var command = CreateValidCommand() with { Metadata = "{\"key\":\"value\",\"number\":123}" };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Metadata);
    }

    [Theory]
    [InlineData("{\"key\":\"value\"}")]
    [InlineData("[]")]
    [InlineData("[1,2,3]")]
    [InlineData("{\"nested\":{\"key\":\"value\"}}")]
    public void Validate_WithVariousValidJsonFormats_ShouldNotHaveValidationError(string validJson)
    {
        // Arrange
        var command = CreateValidCommand() with { Metadata = validJson };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Metadata);
    }

    [Theory]
    [InlineData("invalid json")]
    [InlineData("{key:value}")]
    [InlineData("{'key':'value'}")]
    [InlineData("{\"key\":}")]
    public void Validate_WithInvalidJsonMetadata_ShouldHaveValidationError(string invalidJson)
    {
        // Arrange
        var command = CreateValidCommand() with { Metadata = invalidJson };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Metadata)
            .WithErrorMessage("Metadata must be valid JSON when provided");
    }

    [Fact]
    public void Validate_WithTooLongMetadata_ShouldHaveValidationError()
    {
        // Arrange
        var longMetadata = "{\"data\":\"" + new string('a', 4000) + "\"}";
        var command = CreateValidCommand() with { Metadata = longMetadata };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Metadata)
            .WithErrorMessage("Metadata must not exceed 4000 characters");
    }

    [Fact]
    public void Validate_WithNullMetadata_ShouldNotHaveValidationError()
    {
        // Arrange
        var command = CreateValidCommand() with { Metadata = null };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Metadata);
    }

    #endregion

    #region MessageTimestamp Validation Tests

    [Theory]
    [InlineData("2025-10-21T12:30:45Z")]
    [InlineData("2025-10-21T12:30:45+00:00")]
    [InlineData("2025-10-21T12:30:45.123Z")]
    [InlineData("2025-10-21T12:30:45-05:00")]
    public void Validate_WithValidIso8601Timestamp_ShouldNotHaveValidationError(string validTimestamp)
    {
        // Arrange
        var command = CreateValidCommand() with { MessageTimestamp = validTimestamp };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.MessageTimestamp);
    }

    [Theory]
    [InlineData("invalid-date")]
    [InlineData("not a date")]
    [InlineData("20251021")]
    [InlineData("2025-13-45")]
    public void Validate_WithInvalidTimestamp_ShouldHaveValidationError(string invalidTimestamp)
    {
        // Arrange
        var command = CreateValidCommand() with { MessageTimestamp = invalidTimestamp };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.MessageTimestamp)
            .WithErrorMessage("MessageTimestamp must be a valid ISO 8601 date-time string");
    }

    [Fact]
    public void Validate_WithNullMessageTimestamp_ShouldNotHaveValidationError()
    {
        // Arrange
        var command = CreateValidCommand() with { MessageTimestamp = null };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.MessageTimestamp);
    }

    #endregion

    #region Complete Command Validation Tests

    [Fact]
    public void Validate_WithCompletelyValidCommand_ShouldNotHaveAnyValidationErrors()
    {
        // Arrange
        var command = new ProcessLeadCommand
        {
            TenantId = "tenant-123",
            CorrelationId = Guid.NewGuid().ToString(),
            Email = "test@example.com",
            FirstName = "John",
            LastName = "Doe",
            Phone = "+1-555-1234",
            Company = "Acme Corp",
            Source = "website",
            Metadata = "{\"utm_source\":\"google\"}",
            MessageTimestamp = DateTimeOffset.UtcNow.ToString("o")
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithMinimumValidCommand_ShouldNotHaveAnyValidationErrors()
    {
        // Arrange
        var command = new ProcessLeadCommand
        {
            TenantId = "tenant-123",
            CorrelationId = Guid.NewGuid().ToString(),
            Email = "test@example.com",
            Source = "website"
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithMultipleErrors_ShouldHaveAllValidationErrors()
    {
        // Arrange
        var command = new ProcessLeadCommand
        {
            TenantId = string.Empty,
            CorrelationId = "invalid-guid",
            Email = "invalid-email",
            Source = string.Empty,
            Phone = "abc",
            Metadata = "invalid json"
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.TenantId);
        result.ShouldHaveValidationErrorFor(x => x.CorrelationId);
        result.ShouldHaveValidationErrorFor(x => x.Email);
        result.ShouldHaveValidationErrorFor(x => x.Source);
        result.ShouldHaveValidationErrorFor(x => x.Phone);
        result.ShouldHaveValidationErrorFor(x => x.Metadata);
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Creates a valid ProcessLeadCommand for testing.
    /// </summary>
    private static ProcessLeadCommand CreateValidCommand()
    {
        return new ProcessLeadCommand
        {
            TenantId = "tenant-123",
            CorrelationId = Guid.NewGuid().ToString(),
            Email = "test@example.com",
            Source = "website"
        };
    }

    #endregion
}

