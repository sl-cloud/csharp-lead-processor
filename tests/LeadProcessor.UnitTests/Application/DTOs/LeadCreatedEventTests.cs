using LeadProcessor.Application.DTOs;

namespace LeadProcessor.UnitTests.Application.DTOs;

/// <summary>
/// Unit tests for the LeadCreatedEvent DTO.
/// </summary>
public class LeadCreatedEventTests
{
    #region Initialization Tests

    [Fact]
    public void LeadCreatedEvent_CanBeInitializedWithAllProperties()
    {
        // Arrange & Act
        var correlationId = Guid.NewGuid().ToString();
        var timestamp = DateTimeOffset.UtcNow.ToString("o");

        var leadEvent = new LeadCreatedEvent
        {
            TenantId = "tenant-123",
            CorrelationId = correlationId,
            Email = "test@example.com",
            FirstName = "John",
            LastName = "Doe",
            Phone = "+1-555-1234",
            Company = "Acme Corp",
            Source = "website",
            Metadata = "{\"utm_source\":\"google\"}",
            MessageTimestamp = timestamp
        };

        // Assert
        Assert.Equal("tenant-123", leadEvent.TenantId);
        Assert.Equal(correlationId, leadEvent.CorrelationId);
        Assert.Equal("test@example.com", leadEvent.Email);
        Assert.Equal("John", leadEvent.FirstName);
        Assert.Equal("Doe", leadEvent.LastName);
        Assert.Equal("+1-555-1234", leadEvent.Phone);
        Assert.Equal("Acme Corp", leadEvent.Company);
        Assert.Equal("website", leadEvent.Source);
        Assert.Equal("{\"utm_source\":\"google\"}", leadEvent.Metadata);
        Assert.Equal(timestamp, leadEvent.MessageTimestamp);
    }

    [Fact]
    public void LeadCreatedEvent_CanBeInitializedWithRequiredFieldsOnly()
    {
        // Arrange & Act
        var leadEvent = new LeadCreatedEvent
        {
            TenantId = "tenant-123",
            CorrelationId = Guid.NewGuid().ToString(),
            Email = "test@example.com",
            Source = "website"
        };

        // Assert
        Assert.NotNull(leadEvent.TenantId);
        Assert.NotNull(leadEvent.CorrelationId);
        Assert.NotNull(leadEvent.Email);
        Assert.NotNull(leadEvent.Source);
        Assert.Null(leadEvent.FirstName);
        Assert.Null(leadEvent.LastName);
        Assert.Null(leadEvent.Phone);
        Assert.Null(leadEvent.Company);
        Assert.Null(leadEvent.Metadata);
        Assert.Null(leadEvent.MessageTimestamp);
    }

    #endregion

    #region Record Equality Tests

    [Fact]
    public void LeadCreatedEvent_WithSameValues_AreEqual()
    {
        // Arrange
        var correlationId = Guid.NewGuid().ToString();
        var timestamp = DateTimeOffset.UtcNow.ToString("o");

        var event1 = new LeadCreatedEvent
        {
            TenantId = "tenant-123",
            CorrelationId = correlationId,
            Email = "test@example.com",
            FirstName = "John",
            LastName = "Doe",
            Phone = "555-1234",
            Company = "Acme Corp",
            Source = "website",
            Metadata = "{\"key\":\"value\"}",
            MessageTimestamp = timestamp
        };

        var event2 = new LeadCreatedEvent
        {
            TenantId = "tenant-123",
            CorrelationId = correlationId,
            Email = "test@example.com",
            FirstName = "John",
            LastName = "Doe",
            Phone = "555-1234",
            Company = "Acme Corp",
            Source = "website",
            Metadata = "{\"key\":\"value\"}",
            MessageTimestamp = timestamp
        };

        // Act & Assert
        Assert.Equal(event1, event2);
        Assert.True(event1 == event2);
        Assert.False(event1 != event2);
        Assert.Equal(event1.GetHashCode(), event2.GetHashCode());
    }

    [Fact]
    public void LeadCreatedEvent_WithDifferentValues_AreNotEqual()
    {
        // Arrange
        var event1 = new LeadCreatedEvent
        {
            TenantId = "tenant-123",
            CorrelationId = Guid.NewGuid().ToString(),
            Email = "test1@example.com",
            Source = "website"
        };

        var event2 = new LeadCreatedEvent
        {
            TenantId = "tenant-456",
            CorrelationId = Guid.NewGuid().ToString(),
            Email = "test2@example.com",
            Source = "mobile"
        };

        // Act & Assert
        Assert.NotEqual(event1, event2);
        Assert.False(event1 == event2);
        Assert.True(event1 != event2);
    }

    [Fact]
    public void LeadCreatedEvent_WithDifferentEmail_AreNotEqual()
    {
        // Arrange
        var correlationId = Guid.NewGuid().ToString();

        var event1 = new LeadCreatedEvent
        {
            TenantId = "tenant-123",
            CorrelationId = correlationId,
            Email = "test1@example.com",
            Source = "website"
        };

        var event2 = new LeadCreatedEvent
        {
            TenantId = "tenant-123",
            CorrelationId = correlationId,
            Email = "test2@example.com",
            Source = "website"
        };

        // Act & Assert
        Assert.NotEqual(event1, event2);
    }

    #endregion

    #region Immutability Tests

    [Fact]
    public void LeadCreatedEvent_IsImmutable_CannotModifyAfterInitialization()
    {
        // Arrange
        var leadEvent = new LeadCreatedEvent
        {
            TenantId = "tenant-123",
            CorrelationId = Guid.NewGuid().ToString(),
            Email = "test@example.com",
            Source = "website"
        };

        // Act - Create a modified copy using 'with' expression
        var modifiedEvent = leadEvent with { Email = "modified@example.com" };

        // Assert - Original is unchanged
        Assert.Equal("test@example.com", leadEvent.Email);
        Assert.Equal("modified@example.com", modifiedEvent.Email);
        Assert.NotEqual(leadEvent, modifiedEvent);
    }

    [Fact]
    public void LeadCreatedEvent_WithExpression_CreatesNewInstance()
    {
        // Arrange
        var original = new LeadCreatedEvent
        {
            TenantId = "tenant-123",
            CorrelationId = Guid.NewGuid().ToString(),
            Email = "test@example.com",
            FirstName = "John",
            Source = "website"
        };

        // Act
        var modified = original with { FirstName = "Jane" };

        // Assert
        Assert.NotSame(original, modified);
        Assert.Equal("John", original.FirstName);
        Assert.Equal("Jane", modified.FirstName);
        Assert.Equal(original.Email, modified.Email);
        Assert.Equal(original.TenantId, modified.TenantId);
    }

    #endregion

    #region ToString Tests

    [Fact]
    public void LeadCreatedEvent_ToString_ContainsPropertyValues()
    {
        // Arrange
        var leadEvent = new LeadCreatedEvent
        {
            TenantId = "tenant-123",
            CorrelationId = Guid.NewGuid().ToString(),
            Email = "test@example.com",
            Source = "website"
        };

        // Act
        var result = leadEvent.ToString();

        // Assert
        Assert.Contains("tenant-123", result);
        Assert.Contains("test@example.com", result);
        Assert.Contains("website", result);
    }

    #endregion
}

