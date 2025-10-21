using LeadProcessor.Domain.Entities;

namespace LeadProcessor.UnitTests.Domain.Entities;

/// <summary>
/// Unit tests for the Lead domain entity.
/// </summary>
public class LeadTests
{
    #region HasValidEmail Tests

    [Theory]
    [InlineData("user@example.com", true)]
    [InlineData("test.user+tag@example.co.uk", true)]
    [InlineData("user_name@example-domain.com", true)]
    [InlineData("123@test.com", true)]
    [InlineData("invalid-email", false)]
    [InlineData("@example.com", false)]
    [InlineData("user@", false)]
    [InlineData("user @example.com", false)]
    [InlineData("user@example .com", false)]
    public void HasValidEmail_WithVariousFormats_ReturnsExpectedResult(string email, bool expected)
    {
        // Arrange
        var lead = new Lead
        {
            Id = 1,
            TenantId = "tenant-123",
            CorrelationId = Guid.NewGuid().ToString(),
            Email = email,
            Source = "website",
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        // Act
        var result = lead.HasValidEmail();

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void HasValidEmail_WithNullOrWhitespace_ReturnsFalse(string? email)
    {
        // Arrange
        var lead = new Lead
        {
            Id = 1,
            TenantId = "tenant-123",
            CorrelationId = Guid.NewGuid().ToString(),
            Email = email ?? string.Empty,
            Source = "website",
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        // Act
        var result = lead.HasValidEmail();

        // Assert
        Assert.False(result);
    }

    #endregion

    #region IsCorrelationIdGuid Tests

    [Fact]
    public void IsCorrelationIdGuid_WithValidGuid_ReturnsTrue()
    {
        // Arrange
        var validGuid = Guid.NewGuid().ToString();
        var lead = new Lead
        {
            Id = 1,
            TenantId = "tenant-123",
            CorrelationId = validGuid,
            Email = "test@example.com",
            Source = "website",
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        // Act
        var result = lead.IsCorrelationIdGuid();

        // Assert
        Assert.True(result);
    }

    [Theory]
    [InlineData("not-a-guid")]
    [InlineData("123456")]
    [InlineData("")]
    [InlineData("00000000-0000-0000-0000-00000000000G")] // Invalid character
    public void IsCorrelationIdGuid_WithInvalidGuid_ReturnsFalse(string correlationId)
    {
        // Arrange
        var lead = new Lead
        {
            Id = 1,
            TenantId = "tenant-123",
            CorrelationId = correlationId,
            Email = "test@example.com",
            Source = "website",
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        // Act
        var result = lead.IsCorrelationIdGuid();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsCorrelationIdGuid_WithGuidInDifferentFormats_ReturnsTrue()
    {
        // Arrange - Test various valid GUID formats
        var testCases = new[]
        {
            "6F9619FF-8B86-D011-B42D-00C04FC964FF", // Standard format
            "6f9619ff-8b86-d011-b42d-00c04fc964ff", // Lowercase
            "6F9619FF8B86D011B42D00C04FC964FF"      // Without hyphens
        };

        foreach (var guidString in testCases)
        {
            var lead = new Lead
            {
                Id = 1,
                TenantId = "tenant-123",
                CorrelationId = guidString,
                Email = "test@example.com",
                Source = "website",
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            };

            // Act
            var result = lead.IsCorrelationIdGuid();

            // Assert
            Assert.True(result, $"Expected {guidString} to be valid GUID");
        }
    }

    #endregion

    #region GetFullName Tests

    [Fact]
    public void GetFullName_WithBothNames_ReturnsCombinedName()
    {
        // Arrange
        var lead = new Lead
        {
            Id = 1,
            TenantId = "tenant-123",
            CorrelationId = Guid.NewGuid().ToString(),
            Email = "test@example.com",
            FirstName = "John",
            LastName = "Doe",
            Source = "website",
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        // Act
        var result = lead.GetFullName();

        // Assert
        Assert.Equal("John Doe", result);
    }

    [Fact]
    public void GetFullName_WithOnlyFirstName_ReturnsFirstName()
    {
        // Arrange
        var lead = new Lead
        {
            Id = 1,
            TenantId = "tenant-123",
            CorrelationId = Guid.NewGuid().ToString(),
            Email = "test@example.com",
            FirstName = "John",
            Source = "website",
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        // Act
        var result = lead.GetFullName();

        // Assert
        Assert.Equal("John", result);
    }

    [Fact]
    public void GetFullName_WithOnlyLastName_ReturnsLastName()
    {
        // Arrange
        var lead = new Lead
        {
            Id = 1,
            TenantId = "tenant-123",
            CorrelationId = Guid.NewGuid().ToString(),
            Email = "test@example.com",
            LastName = "Doe",
            Source = "website",
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        // Act
        var result = lead.GetFullName();

        // Assert
        Assert.Equal("Doe", result);
    }

    [Fact]
    public void GetFullName_WithNoNames_ReturnsNull()
    {
        // Arrange
        var lead = new Lead
        {
            Id = 1,
            TenantId = "tenant-123",
            CorrelationId = Guid.NewGuid().ToString(),
            Email = "test@example.com",
            Source = "website",
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        // Act
        var result = lead.GetFullName();

        // Assert
        Assert.Null(result);
    }

    [Theory]
    [InlineData("", "")]
    [InlineData("   ", "   ")]
    [InlineData(null, null)]
    [InlineData("", null)]
    [InlineData(null, "")]
    public void GetFullName_WithEmptyOrWhitespaceNames_ReturnsNull(string? firstName, string? lastName)
    {
        // Arrange
        var lead = new Lead
        {
            Id = 1,
            TenantId = "tenant-123",
            CorrelationId = Guid.NewGuid().ToString(),
            Email = "test@example.com",
            FirstName = firstName,
            LastName = lastName,
            Source = "website",
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        // Act
        var result = lead.GetFullName();

        // Assert
        Assert.Null(result);
    }

    #endregion

    #region Record Equality and Immutability Tests

    [Fact]
    public void Lead_WithSameValues_AreEqual()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;
        var correlationId = Guid.NewGuid().ToString();

        var lead1 = new Lead
        {
            Id = 1,
            TenantId = "tenant-123",
            CorrelationId = correlationId,
            Email = "test@example.com",
            FirstName = "John",
            LastName = "Doe",
            Phone = "555-1234",
            Company = "Acme Corp",
            Source = "website",
            Metadata = "{\"key\":\"value\"}",
            CreatedAt = now,
            UpdatedAt = now
        };

        var lead2 = new Lead
        {
            Id = 1,
            TenantId = "tenant-123",
            CorrelationId = correlationId,
            Email = "test@example.com",
            FirstName = "John",
            LastName = "Doe",
            Phone = "555-1234",
            Company = "Acme Corp",
            Source = "website",
            Metadata = "{\"key\":\"value\"}",
            CreatedAt = now,
            UpdatedAt = now
        };

        // Act & Assert
        Assert.Equal(lead1, lead2);
        Assert.True(lead1 == lead2);
        Assert.False(lead1 != lead2);
    }

    [Fact]
    public void Lead_WithDifferentValues_AreNotEqual()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;

        var lead1 = new Lead
        {
            Id = 1,
            TenantId = "tenant-123",
            CorrelationId = Guid.NewGuid().ToString(),
            Email = "test1@example.com",
            Source = "website",
            CreatedAt = now,
            UpdatedAt = now
        };

        var lead2 = new Lead
        {
            Id = 2,
            TenantId = "tenant-456",
            CorrelationId = Guid.NewGuid().ToString(),
            Email = "test2@example.com",
            Source = "mobile",
            CreatedAt = now,
            UpdatedAt = now
        };

        // Act & Assert
        Assert.NotEqual(lead1, lead2);
        Assert.False(lead1 == lead2);
        Assert.True(lead1 != lead2);
    }

    [Fact]
    public void Lead_CanBeInitializedWithAllProperties()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;
        var correlationId = Guid.NewGuid().ToString();

        // Act
        var lead = new Lead
        {
            Id = 42,
            TenantId = "tenant-123",
            CorrelationId = correlationId,
            Email = "test@example.com",
            FirstName = "John",
            LastName = "Doe",
            Phone = "+1-555-1234",
            Company = "Acme Corp",
            Source = "website",
            Metadata = "{\"utm_source\":\"google\"}",
            CreatedAt = now,
            UpdatedAt = now
        };

        // Assert
        Assert.Equal(42, lead.Id);
        Assert.Equal("tenant-123", lead.TenantId);
        Assert.Equal(correlationId, lead.CorrelationId);
        Assert.Equal("test@example.com", lead.Email);
        Assert.Equal("John", lead.FirstName);
        Assert.Equal("Doe", lead.LastName);
        Assert.Equal("+1-555-1234", lead.Phone);
        Assert.Equal("Acme Corp", lead.Company);
        Assert.Equal("website", lead.Source);
        Assert.Equal("{\"utm_source\":\"google\"}", lead.Metadata);
        Assert.Equal(now, lead.CreatedAt);
        Assert.Equal(now, lead.UpdatedAt);
    }

    #endregion
}

