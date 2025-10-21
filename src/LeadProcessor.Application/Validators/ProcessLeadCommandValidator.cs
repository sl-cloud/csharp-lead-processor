using FluentValidation;
using LeadProcessor.Application.Commands;
using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace LeadProcessor.Application.Validators;

/// <summary>
/// Validator for ProcessLeadCommand using FluentValidation.
/// Ensures all lead data meets required business rules before processing.
/// </summary>
public partial class ProcessLeadCommandValidator : AbstractValidator<ProcessLeadCommand>
{
    private const int MaxEmailLength = 254; // RFC 5321
    private const int MaxPhoneLength = 20;
    private const int MaxNameLength = 100;
    private const int MaxCompanyLength = 200;
    private const int MaxSourceLength = 50;
    private const int MaxTenantIdLength = 100;
    private const int MaxMetadataLength = 4000; // Reasonable JSON limit

    /// <summary>
    /// Initializes a new instance of the ProcessLeadCommandValidator.
    /// </summary>
    public ProcessLeadCommandValidator()
    {
        // TenantId validation
        RuleFor(x => x.TenantId)
            .NotEmpty()
            .WithMessage("TenantId is required")
            .MaximumLength(MaxTenantIdLength)
            .WithMessage($"TenantId must not exceed {MaxTenantIdLength} characters");

        // CorrelationId validation
        RuleFor(x => x.CorrelationId)
            .NotEmpty()
            .WithMessage("CorrelationId is required")
            .Must(BeValidGuid)
            .WithMessage("CorrelationId must be a valid GUID");

        // Email validation
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email is required")
            .MaximumLength(MaxEmailLength)
            .WithMessage($"Email must not exceed {MaxEmailLength} characters")
            .EmailAddress()
            .WithMessage("Email must be a valid email address");

        // FirstName validation
        RuleFor(x => x.FirstName)
            .MaximumLength(MaxNameLength)
            .When(x => !string.IsNullOrEmpty(x.FirstName))
            .WithMessage($"FirstName must not exceed {MaxNameLength} characters");

        // LastName validation
        RuleFor(x => x.LastName)
            .MaximumLength(MaxNameLength)
            .When(x => !string.IsNullOrEmpty(x.LastName))
            .WithMessage($"LastName must not exceed {MaxNameLength} characters");

        // Phone validation
        RuleFor(x => x.Phone)
            .MaximumLength(MaxPhoneLength)
            .When(x => !string.IsNullOrEmpty(x.Phone))
            .WithMessage($"Phone must not exceed {MaxPhoneLength} characters")
            .Must(BeValidPhoneNumber)
            .When(x => !string.IsNullOrEmpty(x.Phone))
            .WithMessage("Phone must be a valid phone number format");

        // Company validation
        RuleFor(x => x.Company)
            .MaximumLength(MaxCompanyLength)
            .When(x => !string.IsNullOrEmpty(x.Company))
            .WithMessage($"Company must not exceed {MaxCompanyLength} characters");

        // Source validation
        RuleFor(x => x.Source)
            .NotEmpty()
            .WithMessage("Source is required")
            .MaximumLength(MaxSourceLength)
            .WithMessage($"Source must not exceed {MaxSourceLength} characters");

        // Metadata validation
        RuleFor(x => x.Metadata)
            .MaximumLength(MaxMetadataLength)
            .When(x => !string.IsNullOrEmpty(x.Metadata))
            .WithMessage($"Metadata must not exceed {MaxMetadataLength} characters")
            .Must(BeValidJson)
            .When(x => !string.IsNullOrEmpty(x.Metadata))
            .WithMessage("Metadata must be valid JSON when provided");

        // MessageTimestamp validation
        RuleFor(x => x.MessageTimestamp)
            .Must(BeValidIso8601DateTime)
            .When(x => !string.IsNullOrEmpty(x.MessageTimestamp))
            .WithMessage("MessageTimestamp must be a valid ISO 8601 date-time string");
    }

    /// <summary>
    /// Validates that the string is a valid GUID.
    /// </summary>
    private static bool BeValidGuid(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;

        return Guid.TryParse(value, out _);
    }

    /// <summary>
    /// Validates that the string is valid JSON.
    /// </summary>
    private static bool BeValidJson(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return true;

        try
        {
            using var document = JsonDocument.Parse(json);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    /// <summary>
    /// Validates that the string is a valid ISO 8601 date-time.
    /// </summary>
    private static bool BeValidIso8601DateTime(string? timestamp)
    {
        if (string.IsNullOrWhiteSpace(timestamp))
            return true;

        return DateTimeOffset.TryParse(
            timestamp,
            CultureInfo.InvariantCulture,
            DateTimeStyles.AssumeUniversal | DateTimeStyles.AllowWhiteSpaces,
            out _);
    }

    /// <summary>
    /// Validates phone number format.
    /// Accepts international formats with optional country code, spaces, dashes, and parentheses.
    /// </summary>
    private static bool BeValidPhoneNumber(string? phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
            return true;

        // Pattern allows:
        // - Optional + at start (international)
        // - Digits, spaces, dashes, parentheses
        // - At least 7 digits (minimum valid phone number)
        var pattern = PhoneNumberRegex();
        if (!pattern.IsMatch(phone))
            return false;

        // Ensure at least 7 digits exist
        var digitCount = phone.Count(char.IsDigit);
        return digitCount >= 7;
    }

    [GeneratedRegex(@"^[\+]?[\d\s\-\(\)]+$", RegexOptions.None, matchTimeoutMilliseconds: 1000)]
    private static partial Regex PhoneNumberRegex();
}

