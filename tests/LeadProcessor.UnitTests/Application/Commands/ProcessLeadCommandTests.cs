using LeadProcessor.Application.Commands;
using MediatR;

namespace LeadProcessor.UnitTests.Application.Commands;

/// <summary>
/// Unit tests for the ProcessLeadCommand.
/// </summary>
public class ProcessLeadCommandTests
{
    #region Initialization Tests

    [Fact]
    public void ProcessLeadCommand_CanBeInitializedWithAllProperties()
    {
        // Arrange & Act
        var correlationId = Guid.NewGuid().ToString();
        var timestamp = DateTimeOffset.UtcNow.ToString("o");

        var command = new ProcessLeadCommand
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
        Assert.Equal("tenant-123", command.TenantId);
        Assert.Equal(correlationId, command.CorrelationId);
        Assert.Equal("test@example.com", command.Email);
        Assert.Equal("John", command.FirstName);
        Assert.Equal("Doe", command.LastName);
        Assert.Equal("+1-555-1234", command.Phone);
        Assert.Equal("Acme Corp", command.Company);
        Assert.Equal("website", command.Source);
        Assert.Equal("{\"utm_source\":\"google\"}", command.Metadata);
        Assert.Equal(timestamp, command.MessageTimestamp);
    }

    [Fact]
    public void ProcessLeadCommand_CanBeInitializedWithRequiredFieldsOnly()
    {
        // Arrange & Act
        var command = new ProcessLeadCommand
        {
            TenantId = "tenant-123",
            CorrelationId = Guid.NewGuid().ToString(),
            Email = "test@example.com",
            Source = "website"
        };

        // Assert
        Assert.NotNull(command.TenantId);
        Assert.NotNull(command.CorrelationId);
        Assert.NotNull(command.Email);
        Assert.NotNull(command.Source);
        Assert.Null(command.FirstName);
        Assert.Null(command.LastName);
        Assert.Null(command.Phone);
        Assert.Null(command.Company);
        Assert.Null(command.Metadata);
        Assert.Null(command.MessageTimestamp);
    }

    #endregion

    #region MediatR Interface Tests

    [Fact]
    public void ProcessLeadCommand_ImplementsIRequest()
    {
        // Arrange
        var command = new ProcessLeadCommand
        {
            TenantId = "tenant-123",
            CorrelationId = Guid.NewGuid().ToString(),
            Email = "test@example.com",
            Source = "website"
        };

        // Act & Assert
        Assert.IsAssignableFrom<IRequest<Unit>>(command);
    }

    #endregion

    #region Record Equality Tests

    [Fact]
    public void ProcessLeadCommand_WithSameValues_AreEqual()
    {
        // Arrange
        var correlationId = Guid.NewGuid().ToString();
        var timestamp = DateTimeOffset.UtcNow.ToString("o");

        var command1 = new ProcessLeadCommand
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

        var command2 = new ProcessLeadCommand
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
        Assert.Equal(command1, command2);
        Assert.True(command1 == command2);
        Assert.False(command1 != command2);
        Assert.Equal(command1.GetHashCode(), command2.GetHashCode());
    }

    [Fact]
    public void ProcessLeadCommand_WithDifferentValues_AreNotEqual()
    {
        // Arrange
        var command1 = new ProcessLeadCommand
        {
            TenantId = "tenant-123",
            CorrelationId = Guid.NewGuid().ToString(),
            Email = "test1@example.com",
            Source = "website"
        };

        var command2 = new ProcessLeadCommand
        {
            TenantId = "tenant-456",
            CorrelationId = Guid.NewGuid().ToString(),
            Email = "test2@example.com",
            Source = "mobile"
        };

        // Act & Assert
        Assert.NotEqual(command1, command2);
        Assert.False(command1 == command2);
        Assert.True(command1 != command2);
    }

    [Fact]
    public void ProcessLeadCommand_WithDifferentCorrelationId_AreNotEqual()
    {
        // Arrange
        var command1 = new ProcessLeadCommand
        {
            TenantId = "tenant-123",
            CorrelationId = Guid.NewGuid().ToString(),
            Email = "test@example.com",
            Source = "website"
        };

        var command2 = new ProcessLeadCommand
        {
            TenantId = "tenant-123",
            CorrelationId = Guid.NewGuid().ToString(),
            Email = "test@example.com",
            Source = "website"
        };

        // Act & Assert
        Assert.NotEqual(command1, command2);
    }

    #endregion

    #region Immutability Tests

    [Fact]
    public void ProcessLeadCommand_IsImmutable_CannotModifyAfterInitialization()
    {
        // Arrange
        var command = new ProcessLeadCommand
        {
            TenantId = "tenant-123",
            CorrelationId = Guid.NewGuid().ToString(),
            Email = "test@example.com",
            Source = "website"
        };

        // Act - Create a modified copy using 'with' expression
        var modifiedCommand = command with { Email = "modified@example.com" };

        // Assert - Original is unchanged
        Assert.Equal("test@example.com", command.Email);
        Assert.Equal("modified@example.com", modifiedCommand.Email);
        Assert.NotEqual(command, modifiedCommand);
    }

    [Fact]
    public void ProcessLeadCommand_WithExpression_CreatesNewInstance()
    {
        // Arrange
        var original = new ProcessLeadCommand
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

    [Fact]
    public void ProcessLeadCommand_WithExpression_CanModifyMultipleProperties()
    {
        // Arrange
        var original = new ProcessLeadCommand
        {
            TenantId = "tenant-123",
            CorrelationId = Guid.NewGuid().ToString(),
            Email = "test@example.com",
            FirstName = "John",
            LastName = "Doe",
            Source = "website"
        };

        // Act
        var modified = original with 
        { 
            FirstName = "Jane",
            LastName = "Smith",
            Company = "New Corp"
        };

        // Assert
        Assert.Equal("John", original.FirstName);
        Assert.Equal("Doe", original.LastName);
        Assert.Null(original.Company);
        
        Assert.Equal("Jane", modified.FirstName);
        Assert.Equal("Smith", modified.LastName);
        Assert.Equal("New Corp", modified.Company);
    }

    #endregion

    #region ToString Tests

    [Fact]
    public void ProcessLeadCommand_ToString_ContainsPropertyValues()
    {
        // Arrange
        var command = new ProcessLeadCommand
        {
            TenantId = "tenant-123",
            CorrelationId = Guid.NewGuid().ToString(),
            Email = "test@example.com",
            Source = "website"
        };

        // Act
        var result = command.ToString();

        // Assert
        Assert.Contains("tenant-123", result);
        Assert.Contains("test@example.com", result);
        Assert.Contains("website", result);
    }

    #endregion
}

