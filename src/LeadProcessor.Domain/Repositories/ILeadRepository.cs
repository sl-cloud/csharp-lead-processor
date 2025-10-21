using LeadProcessor.Domain.Entities;

namespace LeadProcessor.Domain.Repositories;

/// <summary>
/// Repository interface for managing lead persistence operations.
/// </summary>
public interface ILeadRepository
{
    /// <summary>
    /// Saves a lead to the data store asynchronously.
    /// </summary>
    /// <param name="lead">The lead entity to save.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>The saved lead entity with updated fields (e.g., Id, timestamps).</returns>
    Task<Lead> SaveLeadAsync(Lead lead, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a lead with the specified correlation ID already exists.
    /// This method supports idempotency by allowing duplicate message detection.
    /// </summary>
    /// <param name="correlationId">The correlation ID to check for.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>True if a lead with the correlation ID exists, otherwise false.</returns>
    Task<bool> ExistsByCorrelationIdAsync(string correlationId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a lead by its correlation ID asynchronously.
    /// </summary>
    /// <param name="correlationId">The correlation ID to search for.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>The lead entity if found, otherwise null.</returns>
    Task<Lead?> GetByCorrelationIdAsync(string correlationId, CancellationToken cancellationToken = default);
}

