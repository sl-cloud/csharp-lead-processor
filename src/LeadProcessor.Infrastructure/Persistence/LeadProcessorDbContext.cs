namespace LeadProcessor.Infrastructure.Persistence;

using LeadProcessor.Domain.Entities;
using LeadProcessor.Domain.Services;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Entity Framework Core database context for the Lead Processor application.
/// </summary>
/// <remarks>
/// This context manages the Lead entity and ensures UTC timestamp handling for all DateTimeOffset properties.
/// Automatically updates the UpdatedAt timestamp for modified entities during save operations.
/// </remarks>
/// <param name="options">The options to configure the context.</param>
/// <param name="dateTimeProvider">The provider for consistent UTC time handling.</param>
public class LeadProcessorDbContext(
    DbContextOptions<LeadProcessorDbContext> options,
    IDateTimeProvider dateTimeProvider) : DbContext(options)
{
    /// <summary>
    /// Gets or sets the DbSet for Lead entities.
    /// </summary>
    public DbSet<Lead> Leads => Set<Lead>();

    /// <summary>
    /// Configures the model using Fluent API.
    /// </summary>
    /// <param name="modelBuilder">The model builder used to configure the entities.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Lead>(entity =>
        {
            // Table configuration
            entity.ToTable("Leads");
            entity.HasKey(e => e.Id);

            // Primary key configuration
            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd();

            // Required string properties with max lengths
            entity.Property(e => e.TenantId)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.CorrelationId)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.Email)
                .IsRequired()
                .HasMaxLength(320); // RFC 5321 max email length

            entity.Property(e => e.Source)
                .IsRequired()
                .HasMaxLength(100);

            // Optional string properties with max lengths
            entity.Property(e => e.FirstName)
                .HasMaxLength(100);

            entity.Property(e => e.LastName)
                .HasMaxLength(100);

            entity.Property(e => e.Phone)
                .HasMaxLength(50);

            entity.Property(e => e.Company)
                .HasMaxLength(200);

            // JSON column for metadata
            entity.Property(e => e.Metadata)
                .HasColumnType("json");

            // UTC DateTimeOffset configuration
            // Store as UTC datetime and reconstruct with zero offset on retrieval
            entity.Property(e => e.CreatedAt)
                .IsRequired()
                .HasConversion(
                    v => v.UtcDateTime,
                    v => new DateTimeOffset(v, TimeSpan.Zero));

            entity.Property(e => e.UpdatedAt)
                .IsRequired()
                .HasConversion(
                    v => v.UtcDateTime,
                    v => new DateTimeOffset(v, TimeSpan.Zero));

            // Indexes for query performance
            entity.HasIndex(e => e.TenantId)
                .HasDatabaseName("IX_Leads_TenantId");

            entity.HasIndex(e => e.CorrelationId)
                .IsUnique()
                .HasDatabaseName("IX_Leads_CorrelationId");

            entity.HasIndex(e => e.Email)
                .HasDatabaseName("IX_Leads_Email");

            entity.HasIndex(e => e.CreatedAt)
                .HasDatabaseName("IX_Leads_CreatedAt");

            // Composite index for tenant-based queries
            entity.HasIndex(e => new { e.TenantId, e.CreatedAt })
                .HasDatabaseName("IX_Leads_TenantId_CreatedAt");
        });
    }

    /// <summary>
    /// Saves all changes made in this context to the database.
    /// Automatically updates the UpdatedAt timestamp for modified Lead entities.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous save operation. The task result contains the number of state entries written to the database.</returns>
    /// <remarks>
    /// This method intercepts modified Lead entities and creates new instances with updated timestamps
    /// to maintain immutability of record types while ensuring audit trail accuracy.
    /// </remarks>
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateModifiedLeadTimestamps();
        return await base.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Saves all changes made in this context to the database synchronously.
    /// </summary>
    /// <param name="acceptAllChangesOnSuccess">Indicates whether all changes should be accepted if the save operation succeeds.</param>
    /// <returns>The number of state entries written to the database.</returns>
    /// <remarks>
    /// This synchronous method is provided for compatibility but async methods should be preferred.
    /// </remarks>
    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        UpdateModifiedLeadTimestamps();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    /// <summary>
    /// Updates the UpdatedAt timestamp for all modified Lead entities in the change tracker.
    /// </summary>
    /// <remarks>
    /// This method caches the current UTC time once to ensure all modified entities
    /// in the same transaction receive the exact same timestamp, maintaining transactional consistency.
    /// The timestamp update is required to maintain immutability of record types while ensuring audit trail accuracy.
    /// </remarks>
    private void UpdateModifiedLeadTimestamps()
    {
        var modifiedLeadEntries = ChangeTracker.Entries<Lead>()
            .Where(e => e.State == EntityState.Modified)
            .ToList();

        if (modifiedLeadEntries.Count == 0)
        {
            return;
        }

        // Cache the timestamp once to ensure all entities get the same value
        // This maintains transactional consistency
        var updateTimestamp = dateTimeProvider.UtcNow;

        foreach (var entry in modifiedLeadEntries)
        {
            // Create a new Lead instance with the updated timestamp
            // This is required because Lead is a record type (immutable)
            var updatedLead = entry.Entity with { UpdatedAt = updateTimestamp };
            entry.CurrentValues.SetValues(updatedLead);
        }
    }
}

