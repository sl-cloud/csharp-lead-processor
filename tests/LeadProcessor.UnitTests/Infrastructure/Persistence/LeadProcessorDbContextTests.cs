using LeadProcessor.Domain.Entities;
using LeadProcessor.Infrastructure.Persistence;
using LeadProcessor.TestHelpers.Services;
using Microsoft.EntityFrameworkCore;

namespace LeadProcessor.UnitTests.Infrastructure.Persistence;

/// <summary>
/// Unit tests for <see cref="LeadProcessorDbContext"/>.
/// </summary>
public class LeadProcessorDbContextTests
{
    private readonly FixedDateTimeProvider _dateTimeProvider;

    public LeadProcessorDbContextTests()
    {
        _dateTimeProvider = new FixedDateTimeProvider(
            new DateTimeOffset(2025, 1, 15, 10, 30, 0, TimeSpan.Zero));
    }

    /// <summary>
    /// Creates a new in-memory database context for testing.
    /// </summary>
    private LeadProcessorDbContext CreateContext(string databaseName = "TestDb")
    {
        var options = new DbContextOptionsBuilder<LeadProcessorDbContext>()
            .UseInMemoryDatabase(databaseName)
            .Options;

        return new LeadProcessorDbContext(options, _dateTimeProvider);
    }

    [Fact]
    public async Task SaveChangesAsync_WithNewLead_SavesSuccessfully()
    {
        // Arrange
        await using var context = CreateContext();
        var lead = new Lead
        {
            TenantId = "tenant-123",
            CorrelationId = Guid.NewGuid().ToString(),
            Email = "test@example.com",
            FirstName = "John",
            LastName = "Doe",
            Source = "website",
            CreatedAt = _dateTimeProvider.UtcNow,
            UpdatedAt = _dateTimeProvider.UtcNow
        };

        // Act
        context.Leads.Add(lead);
        var result = await context.SaveChangesAsync();

        // Assert
        Assert.Equal(1, result);
        Assert.Equal(1, await context.Leads.CountAsync());
    }

    [Fact]
    public async Task SaveChangesAsync_WithModifiedLead_UpdatesUpdatedAtTimestamp()
    {
        // Arrange
        var databaseName = Guid.NewGuid().ToString();
        var originalTime = new DateTimeOffset(2025, 1, 15, 10, 0, 0, TimeSpan.Zero);
        var updatedTime = new DateTimeOffset(2025, 1, 15, 11, 0, 0, TimeSpan.Zero);

        // Create and save initial lead
        await using (var context = CreateContext(databaseName))
        {
            var lead = new Lead
            {
                TenantId = "tenant-123",
                CorrelationId = Guid.NewGuid().ToString(),
                Email = "test@example.com",
                Source = "website",
                CreatedAt = originalTime,
                UpdatedAt = originalTime
            };
            context.Leads.Add(lead);
            await context.SaveChangesAsync();
        }

        // Act - Update the lead with a new time provider
        var newTimeProvider = new FixedDateTimeProvider(updatedTime);
        await using (var context = new LeadProcessorDbContext(
            new DbContextOptionsBuilder<LeadProcessorDbContext>()
                .UseInMemoryDatabase(databaseName)
                .Options,
            newTimeProvider))
        {
            var lead = await context.Leads.FirstAsync();
            var modifiedLead = lead with { Email = "updated@example.com" };
            context.Entry(lead).CurrentValues.SetValues(modifiedLead);
            await context.SaveChangesAsync();
        }

        // Assert
        await using (var context = CreateContext(databaseName))
        {
            var savedLead = await context.Leads.FirstAsync();
            Assert.Equal("updated@example.com", savedLead.Email);
            Assert.Equal(originalTime, savedLead.CreatedAt);
            Assert.Equal(updatedTime, savedLead.UpdatedAt);
        }
    }

    [Fact]
    public void SaveChanges_SynchronousVersion_UpdatesUpdatedAtTimestamp()
    {
        // Arrange
        var databaseName = Guid.NewGuid().ToString();
        var originalTime = new DateTimeOffset(2025, 1, 15, 10, 0, 0, TimeSpan.Zero);
        var updatedTime = new DateTimeOffset(2025, 1, 15, 11, 0, 0, TimeSpan.Zero);

        // Create and save initial lead
        using (var context = CreateContext(databaseName))
        {
            var lead = new Lead
            {
                TenantId = "tenant-123",
                CorrelationId = Guid.NewGuid().ToString(),
                Email = "test@example.com",
                Source = "website",
                CreatedAt = originalTime,
                UpdatedAt = originalTime
            };
            context.Leads.Add(lead);
            context.SaveChanges();
        }

        // Act - Update the lead with a new time provider
        var newTimeProvider = new FixedDateTimeProvider(updatedTime);
        using (var context = new LeadProcessorDbContext(
            new DbContextOptionsBuilder<LeadProcessorDbContext>()
                .UseInMemoryDatabase(databaseName)
                .Options,
            newTimeProvider))
        {
            var lead = context.Leads.First();
            var modifiedLead = lead with { Email = "updated@example.com" };
            context.Entry(lead).CurrentValues.SetValues(modifiedLead);
            context.SaveChanges();
        }

        // Assert
        using (var context = CreateContext(databaseName))
        {
            var savedLead = context.Leads.First();
            Assert.Equal("updated@example.com", savedLead.Email);
            Assert.Equal(originalTime, savedLead.CreatedAt);
            Assert.Equal(updatedTime, savedLead.UpdatedAt);
        }
    }

    [Fact]
    public void Leads_DbSet_IsConfiguredCorrectly()
    {
        // Arrange
        using var context = CreateContext();

        // Act
        var leadsSet = context.Leads;

        // Assert
        Assert.NotNull(leadsSet);
        Assert.IsAssignableFrom<DbSet<Lead>>(leadsSet);
    }

    [Fact]
    public async Task SaveChangesAsync_WithMultipleModifiedLeads_UpdatesAllTimestamps()
    {
        // Arrange
        var databaseName = Guid.NewGuid().ToString();
        var originalTime = new DateTimeOffset(2025, 1, 15, 10, 0, 0, TimeSpan.Zero);
        var updatedTime = new DateTimeOffset(2025, 1, 15, 11, 0, 0, TimeSpan.Zero);

        // Create and save multiple leads
        await using (var context = CreateContext(databaseName))
        {
            var leads = new[]
            {
                new Lead
                {
                    TenantId = "tenant-123",
                    CorrelationId = Guid.NewGuid().ToString(),
                    Email = "test1@example.com",
                    Source = "website",
                    CreatedAt = originalTime,
                    UpdatedAt = originalTime
                },
                new Lead
                {
                    TenantId = "tenant-123",
                    CorrelationId = Guid.NewGuid().ToString(),
                    Email = "test2@example.com",
                    Source = "mobile",
                    CreatedAt = originalTime,
                    UpdatedAt = originalTime
                }
            };
            context.Leads.AddRange(leads);
            await context.SaveChangesAsync();
        }

        // Act - Update both leads
        var newTimeProvider = new FixedDateTimeProvider(updatedTime);
        await using (var context = new LeadProcessorDbContext(
            new DbContextOptionsBuilder<LeadProcessorDbContext>()
                .UseInMemoryDatabase(databaseName)
                .Options,
            newTimeProvider))
        {
            var leads = await context.Leads.ToListAsync();
            foreach (var lead in leads)
            {
                var modifiedLead = lead with { Phone = "+1234567890" };
                context.Entry(lead).CurrentValues.SetValues(modifiedLead);
            }
            await context.SaveChangesAsync();
        }

        // Assert
        await using (var context = CreateContext(databaseName))
        {
            var savedLeads = await context.Leads.ToListAsync();
            Assert.All(savedLeads, lead =>
            {
                Assert.Equal("+1234567890", lead.Phone);
                Assert.Equal(originalTime, lead.CreatedAt);
                Assert.Equal(updatedTime, lead.UpdatedAt);
            });
        }
    }

    [Fact]
    public async Task SaveChangesAsync_WithUnmodifiedLead_DoesNotUpdateTimestamp()
    {
        // Arrange
        var databaseName = Guid.NewGuid().ToString();
        var originalTime = new DateTimeOffset(2025, 1, 15, 10, 0, 0, TimeSpan.Zero);

        // Create and save initial lead
        await using (var context = CreateContext(databaseName))
        {
            var lead = new Lead
            {
                TenantId = "tenant-123",
                CorrelationId = Guid.NewGuid().ToString(),
                Email = "test@example.com",
                Source = "website",
                CreatedAt = originalTime,
                UpdatedAt = originalTime
            };
            context.Leads.Add(lead);
            await context.SaveChangesAsync();
        }

        // Act - Just query without modifying
        var newTimeProvider = new FixedDateTimeProvider(
            new DateTimeOffset(2025, 1, 15, 11, 0, 0, TimeSpan.Zero));
        await using (var context = new LeadProcessorDbContext(
            new DbContextOptionsBuilder<LeadProcessorDbContext>()
                .UseInMemoryDatabase(databaseName)
                .Options,
            newTimeProvider))
        {
            _ = await context.Leads.FirstAsync();
            await context.SaveChangesAsync();
        }

        // Assert - Timestamp should remain unchanged
        await using (var context = CreateContext(databaseName))
        {
            var savedLead = await context.Leads.FirstAsync();
            Assert.Equal(originalTime, savedLead.UpdatedAt);
        }
    }

    [Fact]
    public async Task SaveChangesAsync_WithCancellationToken_SupportsCancellation()
    {
        // Arrange
        await using var context = CreateContext();
        var lead = new Lead
        {
            TenantId = "tenant-123",
            CorrelationId = Guid.NewGuid().ToString(),
            Email = "test@example.com",
            Source = "website",
            CreatedAt = _dateTimeProvider.UtcNow,
            UpdatedAt = _dateTimeProvider.UtcNow
        };
        context.Leads.Add(lead);

        using var cts = new CancellationTokenSource();

        // Act & Assert - Should complete normally
        var result = await context.SaveChangesAsync(cts.Token);
        Assert.Equal(1, result);
    }

    [Fact]
    public void Constructor_WithValidParameters_CreatesInstance()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<LeadProcessorDbContext>()
            .UseInMemoryDatabase("TestDb")
            .Options;

        // Act
        var context = new LeadProcessorDbContext(options, _dateTimeProvider);

        // Assert
        Assert.NotNull(context);
        Assert.NotNull(context.Leads);
    }

    [Fact]
    public async Task DateTimeOffset_Storage_PreservesUtcOffset()
    {
        // Arrange
        var databaseName = Guid.NewGuid().ToString();
        var utcTime = new DateTimeOffset(2025, 1, 15, 10, 30, 0, TimeSpan.Zero);

        await using (var context = CreateContext(databaseName))
        {
            var lead = new Lead
            {
                TenantId = "tenant-123",
                CorrelationId = Guid.NewGuid().ToString(),
                Email = "test@example.com",
                Source = "website",
                CreatedAt = utcTime,
                UpdatedAt = utcTime
            };
            context.Leads.Add(lead);
            await context.SaveChangesAsync();
        }

        // Act
        await using (var context = CreateContext(databaseName))
        {
            var savedLead = await context.Leads.FirstAsync();

            // Assert - Verify UTC offset is preserved
            Assert.Equal(TimeSpan.Zero, savedLead.CreatedAt.Offset);
            Assert.Equal(TimeSpan.Zero, savedLead.UpdatedAt.Offset);
            Assert.Equal(utcTime, savedLead.CreatedAt);
            Assert.Equal(utcTime, savedLead.UpdatedAt);
        }
    }
}

