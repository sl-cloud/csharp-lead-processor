namespace LeadProcessor.Domain.Exceptions;

/// <summary>
/// Exception thrown when a duplicate lead is detected based on correlation ID.
/// This indicates an idempotency violation where the same message is being processed multiple times.
/// </summary>
public class DuplicateLeadException : Exception
{
    /// <summary>
    /// Gets the correlation ID that caused the duplicate detection.
    /// </summary>
    public string CorrelationId { get; }

    /// <summary>
    /// Initializes a new instance of the DuplicateLeadException class.
    /// </summary>
    /// <param name="correlationId">The correlation ID that already exists.</param>
    public DuplicateLeadException(string correlationId)
        : base($"A lead with correlation ID '{correlationId}' already exists.")
    {
        CorrelationId = correlationId;
    }

    /// <summary>
    /// Initializes a new instance of the DuplicateLeadException class with a custom message.
    /// </summary>
    /// <param name="correlationId">The correlation ID that already exists.</param>
    /// <param name="message">The custom error message.</param>
    public DuplicateLeadException(string correlationId, string message)
        : base(message)
    {
        CorrelationId = correlationId;
    }

    /// <summary>
    /// Initializes a new instance of the DuplicateLeadException class with a custom message and inner exception.
    /// </summary>
    /// <param name="correlationId">The correlation ID that already exists.</param>
    /// <param name="message">The custom error message.</param>
    /// <param name="innerException">The inner exception.</param>
    public DuplicateLeadException(string correlationId, string message, Exception innerException)
        : base(message, innerException)
    {
        CorrelationId = correlationId;
    }
}

