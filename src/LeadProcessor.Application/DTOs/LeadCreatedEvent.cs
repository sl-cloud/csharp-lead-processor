namespace LeadProcessor.Application.DTOs;

/// <summary>
/// Represents a lead creation event received from the SQS message queue.
/// This DTO maps to the message structure sent by the PHP gateway.
/// </summary>
public record LeadCreatedEvent
{
    /// <summary>
    /// Gets the tenant identifier for multi-tenancy support.
    /// </summary>
    public required string TenantId { get; init; }

    /// <summary>
    /// Gets the correlation identifier for idempotency and message tracking.
    /// Must be unique per message to prevent duplicate processing.
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
    /// Gets the ISO 8601 formatted timestamp when the message was sent.
    /// This will be parsed to DateTimeOffset in the handler.
    /// </summary>
    public string? MessageTimestamp { get; init; }
}

