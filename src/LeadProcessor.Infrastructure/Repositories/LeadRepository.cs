using LeadProcessor.Domain.Entities;
using LeadProcessor.Domain.Repositories;
using LeadProcessor.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LeadProcessor.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation of <see cref="ILeadRepository"/> for managing lead persistence operations.
/// </summary>
/// <remarks>
/// This repository provides data access operations for the Lead entity using Entity Framework Core.
/// All operations are async and support cancellation tokens for graceful shutdown.
/// Thread-safe when used with proper DbContext lifecycle management (scoped per request).
/// </remarks>
/// <param name="context">The database context for lead operations.</param>
public sealed class LeadRepository(LeadProcessorDbContext context) : ILeadRepository
{
    private readonly LeadProcessorDbContext _context = context ?? throw new ArgumentNullException(nameof(context));

    /// <summary>
    /// Saves a lead to the data store asynchronously.
    /// </summary>
    /// <param name="lead">The lead entity to save.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>The saved lead entity with updated fields (e.g., Id, timestamps).</returns>
    /// <exception cref="ArgumentNullException">Thrown when lead is null.</exception>
    /// <exception cref="DbUpdateException">Thrown when a database update error occurs.</exception>
    /// <remarks>
    /// This method handles both insert and update operations:
    /// - For new leads (Id == 0), performs an INSERT
    /// - For existing leads (Id > 0), performs an UPDATE
    /// The UpdatedAt timestamp is automatically updated by the DbContext on save.
    /// For updates, detaches any tracked entity to prevent tracking conflicts.
    /// </remarks>
    public async Task<Lead> SaveLeadAsync(Lead lead, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(lead);

        // Determine if this is a new entity or an existing one
        if (lead.Id == 0)
        {
            // New entity - add to context for insert
            await _context.Leads.AddAsync(lead, cancellationToken);
        }
        else
        {
            // Existing entity - check if it's already tracked
            var trackedEntity = _context.Leads.Local.FirstOrDefault(l => l.Id == lead.Id);
            if (trackedEntity != null)
            {
                // Detach the tracked entity to avoid conflicts
                _context.Entry(trackedEntity).State = EntityState.Detached;
            }

            // Attach and mark as modified for update
            _context.Leads.Update(lead);
        }

        await _context.SaveChangesAsync(cancellationToken);
        return lead;
    }

    /// <summary>
    /// Checks if a lead with the specified correlation ID already exists.
    /// This method supports idempotency by allowing duplicate message detection.
    /// </summary>
    /// <param name="correlationId">The correlation ID to check for.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>True if a lead with the correlation ID exists, otherwise false.</returns>
    /// <exception cref="ArgumentException">Thrown when correlationId is null or whitespace.</exception>
    /// <remarks>
    /// This method uses an optimized query that only checks for existence without loading the entity.
    /// It leverages the indexed CorrelationId column for fast lookups.
    /// </remarks>
    public async Task<bool> ExistsByCorrelationIdAsync(string correlationId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(correlationId))
        {
            throw new ArgumentException("Correlation ID cannot be null or whitespace.", nameof(correlationId));
        }

        return await _context.Leads
            .AnyAsync(l => l.CorrelationId == correlationId, cancellationToken);
    }

    /// <summary>
    /// Retrieves a lead by its correlation ID asynchronously.
    /// </summary>
    /// <param name="correlationId">The correlation ID to search for.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>The lead entity if found, otherwise null.</returns>
    /// <exception cref="ArgumentException">Thrown when correlationId is null or whitespace.</exception>
    /// <remarks>
    /// This method leverages the indexed CorrelationId column for efficient lookups.
    /// Returns null if no lead is found with the specified correlation ID.
    /// </remarks>
    public async Task<Lead?> GetByCorrelationIdAsync(string correlationId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(correlationId))
        {
            throw new ArgumentException("Correlation ID cannot be null or whitespace.", nameof(correlationId));
        }

        return await _context.Leads
            .FirstOrDefaultAsync(l => l.CorrelationId == correlationId, cancellationToken);
    }
}

