using MediatR;

namespace LeadProcessor.Application.Commands;

/// <summary>
/// Command to process a lead received from the SQS queue.
/// Implements MediatR's IRequest interface for CQRS pattern.
/// </summary>
public record ProcessLeadCommand : IRequest<Unit>
{
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
    /// Gets the source from which the lead originated.
    /// </summary>
    public required string Source { get; init; }

    /// <summary>
    /// Gets the metadata as a JSON string containing additional information.
    /// </summary>
    public string? Metadata { get; init; }

    /// <summary>
    /// Gets the ISO 8601 formatted timestamp when the message was sent.
    /// Used for message tracking and debugging.
    /// </summary>
    public string? MessageTimestamp { get; init; }
}

