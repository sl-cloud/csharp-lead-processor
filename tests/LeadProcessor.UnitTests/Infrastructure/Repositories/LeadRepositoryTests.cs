using LeadProcessor.Domain.Entities;
using LeadProcessor.Infrastructure.Persistence;
using LeadProcessor.Infrastructure.Repositories;
using LeadProcessor.TestHelpers.Services;
using Microsoft.EntityFrameworkCore;

namespace LeadProcessor.UnitTests.Infrastructure.Repositories;

/// <summary>
/// Unit tests for <see cref="LeadRepository"/>.
/// </summary>
/// <remarks>
/// These tests verify the repository's data access operations using EF Core InMemory database.
/// Each test uses an isolated in-memory database to ensure test independence.
/// </remarks>
public sealed class LeadRepositoryTests
{
    private static readonly DateTimeOffset FixedDateTime = new(2025, 10, 22, 12, 0, 0, TimeSpan.Zero);

    /// <summary>
    /// Creates a new in-memory database context for testing.
    /// Each call creates a fresh database with a unique name to ensure test isolation.
    /// </summary>
    private static LeadProcessorDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<LeadProcessorDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var dateTimeProvider = new FixedDateTimeProvider(FixedDateTime);
        return new LeadProcessorDbContext(options, dateTimeProvider);
    }

    /// <summary>
    /// Creates a sample lead entity for testing.
    /// </summary>
    private static Lead CreateSampleLead(string correlationId = "test-correlation-id") => new()
    {
        Email = "john.doe@example.com",
        FirstName = "John",
        LastName = "Doe",
        Phone = "+44 20 1234 5678",
        Company = "Acme Corp",
        Source = "WebForm",
        CorrelationId = correlationId,
        TenantId = "tenant-123",
        Metadata = """{"campaign":"spring-2025","utm_source":"google"}""",
        CreatedAt = FixedDateTime,
        UpdatedAt = FixedDateTime
    };

    #region Constructor Tests

    [Fact]
    public void Constructor_WithValidContext_CreatesInstance()
    {
        // Arrange
        using var context = CreateInMemoryContext();

        // Act
        var repository = new LeadRepository(context);

        // Assert
        Assert.NotNull(repository);
    }

    [Fact]
    public void Constructor_WithNullContext_ThrowsArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => new LeadRepository(null!));
        Assert.Equal("context", exception.ParamName);
    }

    #endregion

    #region SaveLeadAsync Tests

    [Fact]
    public async Task SaveLeadAsync_WithNewLead_InsertsLeadAndReturnsWithId()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var repository = new LeadRepository(context);
        var lead = CreateSampleLead();

        // Act
        var savedLead = await repository.SaveLeadAsync(lead);

        // Assert
        Assert.NotNull(savedLead);
        Assert.True(savedLead.Id > 0, "Saved lead should have a positive ID assigned by the database");
        Assert.Equal(lead.FirstName, savedLead.FirstName);
        Assert.Equal(lead.Email, savedLead.Email);
        Assert.Equal(lead.CorrelationId, savedLead.CorrelationId);

        // Verify in database
        var retrievedLead = await context.Leads.FindAsync(savedLead.Id);
        Assert.NotNull(retrievedLead);
        Assert.Equal(savedLead.Id, retrievedLead.Id);
        Assert.Equal(savedLead.CorrelationId, retrievedLead.CorrelationId);
    }

    [Fact]
    public async Task SaveLeadAsync_WithExistingLead_UpdatesLead()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var repository = new LeadRepository(context);
        var originalLead = CreateSampleLead();
        var savedLead = await repository.SaveLeadAsync(originalLead);

        // Create an updated version with the same ID
        // Repository should handle detaching the tracked entity automatically
        var updatedLead = savedLead with
        {
            FirstName = "Jane",
            LastName = "Smith",
            Company = "New Corp"
        };

        // Act
        var result = await repository.SaveLeadAsync(updatedLead);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(updatedLead.Id, result.Id);
        Assert.Equal("Jane", result.FirstName);
        Assert.Equal("Smith", result.LastName);
        Assert.Equal("New Corp", result.Company);
        Assert.Equal(savedLead.CorrelationId, result.CorrelationId);

        // Verify only one record exists
        var leadCount = await context.Leads.CountAsync();
        Assert.Equal(1, leadCount);
    }

    [Fact]
    public async Task SaveLeadAsync_WithNullLead_ThrowsArgumentNullException()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var repository = new LeadRepository(context);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(
            async () => await repository.SaveLeadAsync(null!));
    }

    [Fact]
    public async Task SaveLeadAsync_WithCancellationToken_SupportsCancellation()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var repository = new LeadRepository(context);
        var lead = CreateSampleLead();
        using var cts = new CancellationTokenSource();

        // Act
        cts.Cancel();

        // Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            async () => await repository.SaveLeadAsync(lead, cts.Token));
    }

    [Fact]
    public async Task SaveLeadAsync_WithMultipleLeads_SavesAllIndependently()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var repository = new LeadRepository(context);
        var lead1 = CreateSampleLead("correlation-1");
        var lead2 = CreateSampleLead("correlation-2");

        // Act
        var savedLead1 = await repository.SaveLeadAsync(lead1);
        var savedLead2 = await repository.SaveLeadAsync(lead2);

        // Assert
        Assert.NotEqual(savedLead1.Id, savedLead2.Id);
        Assert.Equal("correlation-1", savedLead1.CorrelationId);
        Assert.Equal("correlation-2", savedLead2.CorrelationId);

        var totalLeads = await context.Leads.CountAsync();
        Assert.Equal(2, totalLeads);
    }

    #endregion

    #region ExistsByCorrelationIdAsync Tests

    [Fact]
    public async Task ExistsByCorrelationIdAsync_WithExistingCorrelationId_ReturnsTrue()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var repository = new LeadRepository(context);
        var lead = CreateSampleLead("existing-correlation-id");
        await repository.SaveLeadAsync(lead);

        // Act
        var exists = await repository.ExistsByCorrelationIdAsync("existing-correlation-id");

        // Assert
        Assert.True(exists);
    }

    [Fact]
    public async Task ExistsByCorrelationIdAsync_WithNonExistentCorrelationId_ReturnsFalse()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var repository = new LeadRepository(context);

        // Act
        var exists = await repository.ExistsByCorrelationIdAsync("non-existent-correlation-id");

        // Assert
        Assert.False(exists);
    }

    [Fact]
    public async Task ExistsByCorrelationIdAsync_WithNullCorrelationId_ThrowsArgumentException()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var repository = new LeadRepository(context);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            async () => await repository.ExistsByCorrelationIdAsync(null!));
        Assert.Equal("correlationId", exception.ParamName);
    }

    [Fact]
    public async Task ExistsByCorrelationIdAsync_WithWhitespaceCorrelationId_ThrowsArgumentException()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var repository = new LeadRepository(context);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            async () => await repository.ExistsByCorrelationIdAsync("   "));
        Assert.Equal("correlationId", exception.ParamName);
    }

    [Fact]
    public async Task ExistsByCorrelationIdAsync_WithEmptyCorrelationId_ThrowsArgumentException()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var repository = new LeadRepository(context);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            async () => await repository.ExistsByCorrelationIdAsync(string.Empty));
        Assert.Equal("correlationId", exception.ParamName);
    }

    [Fact]
    public async Task ExistsByCorrelationIdAsync_WithCancellationToken_SupportsCancellation()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var repository = new LeadRepository(context);
        using var cts = new CancellationTokenSource();

        // Act
        cts.Cancel();

        // Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            async () => await repository.ExistsByCorrelationIdAsync("test-id", cts.Token));
    }

    [Fact]
    public async Task ExistsByCorrelationIdAsync_WithMultipleLeads_FindsCorrectOne()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var repository = new LeadRepository(context);
        await repository.SaveLeadAsync(CreateSampleLead("correlation-1"));
        await repository.SaveLeadAsync(CreateSampleLead("correlation-2"));
        await repository.SaveLeadAsync(CreateSampleLead("correlation-3"));

        // Act
        var exists2 = await repository.ExistsByCorrelationIdAsync("correlation-2");
        var exists4 = await repository.ExistsByCorrelationIdAsync("correlation-4");

        // Assert
        Assert.True(exists2);
        Assert.False(exists4);
    }

    #endregion

    #region GetByCorrelationIdAsync Tests

    [Fact]
    public async Task GetByCorrelationIdAsync_WithExistingCorrelationId_ReturnsLead()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var repository = new LeadRepository(context);
        var originalLead = CreateSampleLead("existing-correlation-id");
        await repository.SaveLeadAsync(originalLead);

        // Act
        var retrievedLead = await repository.GetByCorrelationIdAsync("existing-correlation-id");

        // Assert
        Assert.NotNull(retrievedLead);
        Assert.Equal(originalLead.CorrelationId, retrievedLead.CorrelationId);
        Assert.Equal(originalLead.FirstName, retrievedLead.FirstName);
        Assert.Equal(originalLead.Email, retrievedLead.Email);
        Assert.Equal(originalLead.TenantId, retrievedLead.TenantId);
    }

    [Fact]
    public async Task GetByCorrelationIdAsync_WithNonExistentCorrelationId_ReturnsNull()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var repository = new LeadRepository(context);

        // Act
        var retrievedLead = await repository.GetByCorrelationIdAsync("non-existent-correlation-id");

        // Assert
        Assert.Null(retrievedLead);
    }

    [Fact]
    public async Task GetByCorrelationIdAsync_WithNullCorrelationId_ThrowsArgumentException()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var repository = new LeadRepository(context);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            async () => await repository.GetByCorrelationIdAsync(null!));
        Assert.Equal("correlationId", exception.ParamName);
    }

    [Fact]
    public async Task GetByCorrelationIdAsync_WithWhitespaceCorrelationId_ThrowsArgumentException()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var repository = new LeadRepository(context);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            async () => await repository.GetByCorrelationIdAsync("   "));
        Assert.Equal("correlationId", exception.ParamName);
    }

    [Fact]
    public async Task GetByCorrelationIdAsync_WithEmptyCorrelationId_ThrowsArgumentException()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var repository = new LeadRepository(context);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            async () => await repository.GetByCorrelationIdAsync(string.Empty));
        Assert.Equal("correlationId", exception.ParamName);
    }

    [Fact]
    public async Task GetByCorrelationIdAsync_WithCancellationToken_SupportsCancellation()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var repository = new LeadRepository(context);
        using var cts = new CancellationTokenSource();

        // Act
        cts.Cancel();

        // Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            async () => await repository.GetByCorrelationIdAsync("test-id", cts.Token));
    }

    [Fact]
    public async Task GetByCorrelationIdAsync_WithMultipleLeads_ReturnsCorrectOne()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var repository = new LeadRepository(context);
        var lead1 = CreateSampleLead("correlation-1") with { FirstName = "Alice" };
        var lead2 = CreateSampleLead("correlation-2") with { FirstName = "Bob" };
        var lead3 = CreateSampleLead("correlation-3") with { FirstName = "Charlie" };
        await repository.SaveLeadAsync(lead1);
        await repository.SaveLeadAsync(lead2);
        await repository.SaveLeadAsync(lead3);

        // Act
        var retrievedLead = await repository.GetByCorrelationIdAsync("correlation-2");

        // Assert
        Assert.NotNull(retrievedLead);
        Assert.Equal("Bob", retrievedLead.FirstName);
        Assert.Equal("correlation-2", retrievedLead.CorrelationId);
    }

    [Fact]
    public async Task GetByCorrelationIdAsync_RetrievesAllLeadProperties()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var repository = new LeadRepository(context);
        var originalLead = CreateSampleLead("full-test-correlation-id");
        await repository.SaveLeadAsync(originalLead);

        // Act
        var retrievedLead = await repository.GetByCorrelationIdAsync("full-test-correlation-id");

        // Assert
        Assert.NotNull(retrievedLead);
        Assert.Equal(originalLead.Email, retrievedLead.Email);
        Assert.Equal(originalLead.FirstName, retrievedLead.FirstName);
        Assert.Equal(originalLead.LastName, retrievedLead.LastName);
        Assert.Equal(originalLead.Phone, retrievedLead.Phone);
        Assert.Equal(originalLead.Company, retrievedLead.Company);
        Assert.Equal(originalLead.Source, retrievedLead.Source);
        Assert.Equal(originalLead.Metadata, retrievedLead.Metadata);
        Assert.Equal(originalLead.CorrelationId, retrievedLead.CorrelationId);
        Assert.Equal(originalLead.TenantId, retrievedLead.TenantId);
    }

    #endregion

    #region Integration Tests

    [Fact]
    public async Task Repository_IdempotencyScenario_SaveAndCheckExistence()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var repository = new LeadRepository(context);
        var correlationId = "idempotency-test-id";

        // Act & Assert - Lead doesn't exist initially
        var existsBefore = await repository.ExistsByCorrelationIdAsync(correlationId);
        Assert.False(existsBefore);

        // Save the lead
        var lead = CreateSampleLead(correlationId);
        await repository.SaveLeadAsync(lead);

        // Lead exists after saving
        var existsAfter = await repository.ExistsByCorrelationIdAsync(correlationId);
        Assert.True(existsAfter);

        // Can retrieve the lead
        var retrievedLead = await repository.GetByCorrelationIdAsync(correlationId);
        Assert.NotNull(retrievedLead);
        Assert.Equal(correlationId, retrievedLead.CorrelationId);
    }

    [Fact]
    public async Task Repository_UpdateScenario_SaveRetrieveUpdateRetrieve()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var repository = new LeadRepository(context);
        var correlationId = "update-test-id";

        // Act - Save initial lead
        var originalLead = CreateSampleLead(correlationId) with { Source = "WebForm" };
        var savedLead = await repository.SaveLeadAsync(originalLead);
        var originalId = savedLead.Id;

        // Retrieve the lead
        var retrievedLead = await repository.GetByCorrelationIdAsync(correlationId);
        Assert.NotNull(retrievedLead);
        Assert.Equal("WebForm", retrievedLead.Source);

        // Update the lead - repository should handle any tracking conflicts
        var updatedLead = retrievedLead with { Source = "EmailCampaign" };
        await repository.SaveLeadAsync(updatedLead);

        // Retrieve again
        var finalLead = await repository.GetByCorrelationIdAsync(correlationId);

        // Assert
        Assert.NotNull(finalLead);
        Assert.Equal(originalId, finalLead.Id);
        Assert.Equal("EmailCampaign", finalLead.Source);
        Assert.Equal(correlationId, finalLead.CorrelationId);
    }

    #endregion
}

