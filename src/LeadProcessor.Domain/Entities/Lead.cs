namespace LeadProcessor.Domain.Entities;

/// <summary>
/// Represents a lead entity captured from various sources.
/// </summary>
public record Lead
{
    /// <summary>
    /// Gets the unique identifier for the lead.
    /// </summary>
    public int Id { get; init; }

    /// <summary>
    /// Gets the tenant identifier for multi-tenancy support.
    /// </summary>
    public required string TenantId { get; init; }

    /// <summary>
    /// Gets the correlation identifier for idempotency and message tracking.
    /// </summary>
    public required string CorrelationId { get; init; }

    /// <summary>
    /// Gets the email address of the lead.
    /// </summary>
    public required string Email { get; init; }

    /// <summary>
    /// Gets the first name of the lead.
    /// </summary>
    public string? FirstName { get; init; }

    /// <summary>
    /// Gets the last name of the lead.
    /// </summary>
    public string? LastName { get; init; }

    /// <summary>
    /// Gets the phone number of the lead.
    /// </summary>
    public string? Phone { get; init; }

    /// <summary>
    /// Gets the company name of the lead.
    /// </summary>
    public string? Company { get; init; }

    /// <summary>
    /// Gets the source from which the lead originated (e.g., website, mobile app, referral).
    /// </summary>
    public required string Source { get; init; }

    /// <summary>
    /// Gets the metadata as a JSON string containing additional information about the lead.
    /// </summary>
    public string? Metadata { get; init; }

    /// <summary>
    /// Gets the UTC date and time when the lead was created.
    /// </summary>
    public DateTimeOffset CreatedAt { get; init; }

    /// <summary>
    /// Gets the UTC date and time when the lead was last updated.
    /// </summary>
    public DateTimeOffset UpdatedAt { get; init; }

    /// <summary>
    /// Validates that the email format is correct.
    /// </summary>
    /// <returns>
    /// True if the email is in a valid format according to RFC 5322, otherwise false.
    /// Returns false if email is null, empty, or whitespace.
    /// </returns>
    public bool HasValidEmail()
    {
        if (string.IsNullOrWhiteSpace(Email))
            return false;

        try
        {
            var addr = new System.Net.Mail.MailAddress(Email);
            return addr.Address == Email;
        }
        catch (FormatException)
        {
            return false;
        }
    }

    /// <summary>
    /// Validates that the correlation ID is in GUID format.
    /// </summary>
    /// <returns>True if the correlation ID can be parsed as a GUID, otherwise false.</returns>
    public bool IsCorrelationIdGuid()
    {
        return Guid.TryParse(CorrelationId, out _);
    }

    /// <summary>
    /// Gets the full name of the lead by combining first and last names.
    /// </summary>
    /// <returns>The full name, or null if both names are empty.</returns>
    public string? GetFullName()
    {
        var hasFirst = !string.IsNullOrWhiteSpace(FirstName);
        var hasLast = !string.IsNullOrWhiteSpace(LastName);

        return (hasFirst, hasLast) switch
        {
            (true, true) => $"{FirstName} {LastName}",
            (true, false) => FirstName,
            (false, true) => LastName,
            _ => null
        };
    }
}

