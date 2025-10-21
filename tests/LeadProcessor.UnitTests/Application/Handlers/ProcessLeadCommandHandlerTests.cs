using FluentValidation;
using FluentValidation.Results;
using LeadProcessor.Application.Commands;
using LeadProcessor.Application.Handlers;
using LeadProcessor.Domain.Entities;
using LeadProcessor.Domain.Exceptions;
using LeadProcessor.Domain.Repositories;
using LeadProcessor.Domain.Services;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;

namespace LeadProcessor.UnitTests.Application.Handlers;

/// <summary>
/// Unit tests for ProcessLeadCommandHandler.
/// </summary>
public class ProcessLeadCommandHandlerTests
{
    private readonly Mock<ILeadRepository> _repositoryMock;
    private readonly Mock<IDateTimeProvider> _dateTimeProviderMock;
    private readonly Mock<IValidator<ProcessLeadCommand>> _validatorMock;
    private readonly Mock<ILogger<ProcessLeadCommandHandler>> _loggerMock;
    private readonly ProcessLeadCommandHandler _handler;
    private readonly DateTimeOffset _fixedDateTime;

    public ProcessLeadCommandHandlerTests()
    {
        _repositoryMock = new Mock<ILeadRepository>();
        _dateTimeProviderMock = new Mock<IDateTimeProvider>();
        _validatorMock = new Mock<IValidator<ProcessLeadCommand>>();
        _loggerMock = new Mock<ILogger<ProcessLeadCommandHandler>>();
        
        _fixedDateTime = new DateTimeOffset(2025, 10, 21, 12, 30, 45, TimeSpan.Zero);
        _dateTimeProviderMock.Setup(x => x.UtcNow).Returns(_fixedDateTime);

        _handler = new ProcessLeadCommandHandler(
            _repositoryMock.Object,
            _dateTimeProviderMock.Object,
            _validatorMock.Object,
            _loggerMock.Object);
    }

    #region Successful Processing Tests

    [Fact]
    public async Task Handle_WithValidCommand_ShouldSaveLeadSuccessfully()
    {
        // Arrange
        var command = CreateValidCommand();
        SetupValidationSuccess();
        SetupRepositoryNotExists();
        SetupRepositorySaveSuccess();

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(Unit.Value, result);
        
        _validatorMock.Verify(v => v.ValidateAsync(command, It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(r => r.ExistsByCorrelationIdAsync(command.CorrelationId, It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(r => r.SaveLeadAsync(It.IsAny<Lead>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldMapAllFieldsToLead()
    {
        // Arrange
        var command = CreateValidCommand();
        Lead? capturedLead = null;

        SetupValidationSuccess();
        SetupRepositoryNotExists();
        _repositoryMock
            .Setup(r => r.SaveLeadAsync(It.IsAny<Lead>(), It.IsAny<CancellationToken>()))
            .Callback<Lead, CancellationToken>((lead, _) => capturedLead = lead)
            .ReturnsAsync((Lead lead, CancellationToken _) => lead with { Id = 42 });

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(capturedLead);
        Assert.Equal(command.TenantId, capturedLead.TenantId);
        Assert.Equal(command.CorrelationId, capturedLead.CorrelationId);
        Assert.Equal(command.Email, capturedLead.Email);
        Assert.Equal(command.FirstName, capturedLead.FirstName);
        Assert.Equal(command.LastName, capturedLead.LastName);
        Assert.Equal(command.Phone, capturedLead.Phone);
        Assert.Equal(command.Company, capturedLead.Company);
        Assert.Equal(command.Source, capturedLead.Source);
        Assert.Equal(command.Metadata, capturedLead.Metadata);
        Assert.Equal(_fixedDateTime, capturedLead.CreatedAt);
        Assert.Equal(_fixedDateTime, capturedLead.UpdatedAt);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldUseFixedTimestampFromProvider()
    {
        // Arrange
        var command = CreateValidCommand();
        Lead? capturedLead = null;

        SetupValidationSuccess();
        SetupRepositoryNotExists();
        _repositoryMock
            .Setup(r => r.SaveLeadAsync(It.IsAny<Lead>(), It.IsAny<CancellationToken>()))
            .Callback<Lead, CancellationToken>((lead, _) => capturedLead = lead)
            .ReturnsAsync((Lead lead, CancellationToken _) => lead with { Id = 42 });

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(capturedLead);
        Assert.Equal(_fixedDateTime, capturedLead.CreatedAt);
        Assert.Equal(_fixedDateTime, capturedLead.UpdatedAt);
        _dateTimeProviderMock.Verify(d => d.UtcNow, Times.Once);
    }

    [Fact]
    public async Task Handle_WithMinimalCommand_ShouldSaveWithNullOptionalFields()
    {
        // Arrange
        var command = new ProcessLeadCommand
        {
            TenantId = "tenant-123",
            CorrelationId = Guid.NewGuid().ToString(),
            Email = "test@example.com",
            Source = "website"
        };
        Lead? capturedLead = null;

        SetupValidationSuccess();
        SetupRepositoryNotExists();
        _repositoryMock
            .Setup(r => r.SaveLeadAsync(It.IsAny<Lead>(), It.IsAny<CancellationToken>()))
            .Callback<Lead, CancellationToken>((lead, _) => capturedLead = lead)
            .ReturnsAsync((Lead lead, CancellationToken _) => lead with { Id = 42 });

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(capturedLead);
        Assert.Null(capturedLead.FirstName);
        Assert.Null(capturedLead.LastName);
        Assert.Null(capturedLead.Phone);
        Assert.Null(capturedLead.Company);
        Assert.Null(capturedLead.Metadata);
    }

    [Fact]
    public async Task Handle_WithValidMessageTimestamp_ShouldLogTimestampParsing()
    {
        // Arrange
        var command = CreateValidCommand() with 
        { 
            MessageTimestamp = "2025-10-21T12:00:00Z" 
        };
        SetupValidationSuccess();
        SetupRepositoryNotExists();
        SetupRepositorySaveSuccess();

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert - handler should process successfully and log the timestamp
        _repositoryMock.Verify(r => r.SaveLeadAsync(It.IsAny<Lead>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region Validation Tests

    [Fact]
    public async Task Handle_WithInvalidCommand_ShouldThrowValidationException()
    {
        // Arrange
        var command = CreateValidCommand();
        var validationErrors = new List<ValidationFailure>
        {
            new ValidationFailure("Email", "Email is required")
        };
        SetupValidationFailure(validationErrors);

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => 
            _handler.Handle(command, CancellationToken.None));

        _repositoryMock.Verify(r => r.ExistsByCorrelationIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        _repositoryMock.Verify(r => r.SaveLeadAsync(It.IsAny<Lead>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithValidationErrors_ShouldNotPersistLead()
    {
        // Arrange
        var command = CreateValidCommand();
        var validationErrors = new List<ValidationFailure>
        {
            new ValidationFailure("Email", "Invalid email format"),
            new ValidationFailure("Phone", "Invalid phone format")
        };
        SetupValidationFailure(validationErrors);

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => 
            _handler.Handle(command, CancellationToken.None));

        _repositoryMock.Verify(r => r.SaveLeadAsync(It.IsAny<Lead>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion

    #region Idempotency Tests

    [Fact]
    public async Task Handle_WithDuplicateCorrelationId_ShouldThrowDuplicateLeadException()
    {
        // Arrange
        var command = CreateValidCommand();
        SetupValidationSuccess();
        _repositoryMock
            .Setup(r => r.ExistsByCorrelationIdAsync(command.CorrelationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<DuplicateLeadException>(() => 
            _handler.Handle(command, CancellationToken.None));

        Assert.Equal(command.CorrelationId, exception.CorrelationId);
        _repositoryMock.Verify(r => r.SaveLeadAsync(It.IsAny<Lead>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithDuplicateCorrelationId_ShouldNotAttemptSave()
    {
        // Arrange
        var command = CreateValidCommand();
        SetupValidationSuccess();
        _repositoryMock
            .Setup(r => r.ExistsByCorrelationIdAsync(command.CorrelationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act & Assert
        await Assert.ThrowsAsync<DuplicateLeadException>(() => 
            _handler.Handle(command, CancellationToken.None));

        _repositoryMock.Verify(r => r.SaveLeadAsync(It.IsAny<Lead>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ChecksIdempotencyBeforeSaving()
    {
        // Arrange
        var command = CreateValidCommand();
        var callOrder = new List<string>();

        SetupValidationSuccess();
        
        _repositoryMock
            .Setup(r => r.ExistsByCorrelationIdAsync(command.CorrelationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false)
            .Callback(() => callOrder.Add("ExistsByCorrelationId"));

        _repositoryMock
            .Setup(r => r.SaveLeadAsync(It.IsAny<Lead>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Lead lead, CancellationToken _) => lead with { Id = 42 })
            .Callback(() => callOrder.Add("SaveLead"));

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(2, callOrder.Count);
        Assert.Equal("ExistsByCorrelationId", callOrder[0]);
        Assert.Equal("SaveLead", callOrder[1]);
    }

    #endregion

    #region Exception Handling Tests

    [Fact]
    public async Task Handle_WhenRepositoryThrowsException_ShouldPropagateException()
    {
        // Arrange
        var command = CreateValidCommand();
        SetupValidationSuccess();
        SetupRepositoryNotExists();
        
        _repositoryMock
            .Setup(r => r.SaveLeadAsync(It.IsAny<Lead>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database error"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => 
            _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WhenValidatorThrowsException_ShouldPropagateException()
    {
        // Arrange
        var command = CreateValidCommand();
        _validatorMock
            .Setup(v => v.ValidateAsync(command, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Validator error"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => 
            _handler.Handle(command, CancellationToken.None));

        _repositoryMock.Verify(r => r.SaveLeadAsync(It.IsAny<Lead>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion

    #region Cancellation Tests

    [Fact]
    public async Task Handle_WhenCancellationRequested_ShouldPassCancellationToken()
    {
        // Arrange
        var command = CreateValidCommand();
        var cts = new CancellationTokenSource();

        SetupValidationSuccess();
        SetupRepositoryNotExists();
        
        // Cancel after setup but before execution
        cts.Cancel();

        _repositoryMock
            .Setup(r => r.SaveLeadAsync(It.IsAny<Lead>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());

        // Act & Assert - should propagate cancellation
        await Assert.ThrowsAsync<OperationCanceledException>(async () => 
        {
            await _handler.Handle(command, cts.Token);
        });
    }

    [Fact]
    public async Task Handle_ShouldPassCancellationTokenToAllAsyncOperations()
    {
        // Arrange
        var command = CreateValidCommand();
        var cancellationToken = new CancellationToken();
        CancellationToken capturedValidationToken = default;
        CancellationToken capturedExistsToken = default;
        CancellationToken capturedSaveToken = default;

        _validatorMock
            .Setup(v => v.ValidateAsync(command, It.IsAny<CancellationToken>()))
            .Callback<ProcessLeadCommand, CancellationToken>((_, token) => capturedValidationToken = token)
            .ReturnsAsync(new ValidationResult());

        _repositoryMock
            .Setup(r => r.ExistsByCorrelationIdAsync(command.CorrelationId, It.IsAny<CancellationToken>()))
            .Callback<string, CancellationToken>((_, token) => capturedExistsToken = token)
            .ReturnsAsync(false);

        _repositoryMock
            .Setup(r => r.SaveLeadAsync(It.IsAny<Lead>(), It.IsAny<CancellationToken>()))
            .Callback<Lead, CancellationToken>((_, token) => capturedSaveToken = token)
            .ReturnsAsync((Lead lead, CancellationToken _) => lead with { Id = 42 });

        // Act
        await _handler.Handle(command, cancellationToken);

        // Assert
        Assert.Equal(cancellationToken, capturedValidationToken);
        Assert.Equal(cancellationToken, capturedExistsToken);
        Assert.Equal(cancellationToken, capturedSaveToken);
    }

    #endregion

    #region Message Timestamp Parsing Tests

    [Theory]
    [InlineData("2025-10-21T12:30:45Z")]
    [InlineData("2025-10-21T12:30:45+00:00")]
    [InlineData("2025-10-21T12:30:45.123Z")]
    public async Task Handle_WithValidMessageTimestamp_ShouldProcessSuccessfully(string timestamp)
    {
        // Arrange
        var command = CreateValidCommand() with { MessageTimestamp = timestamp };
        SetupValidationSuccess();
        SetupRepositoryNotExists();
        SetupRepositorySaveSuccess();

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(Unit.Value, result);
        _repositoryMock.Verify(r => r.SaveLeadAsync(It.IsAny<Lead>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData("invalid-timestamp")]
    [InlineData("not-a-date")]
    [InlineData("")]
    public async Task Handle_WithInvalidMessageTimestamp_ShouldStillProcessSuccessfully(string timestamp)
    {
        // Arrange
        var command = CreateValidCommand() with { MessageTimestamp = timestamp };
        SetupValidationSuccess();
        SetupRepositoryNotExists();
        SetupRepositorySaveSuccess();

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert - invalid timestamp should not prevent processing
        Assert.Equal(Unit.Value, result);
        _repositoryMock.Verify(r => r.SaveLeadAsync(It.IsAny<Lead>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNullMessageTimestamp_ShouldProcessSuccessfully()
    {
        // Arrange
        var command = CreateValidCommand() with { MessageTimestamp = null };
        SetupValidationSuccess();
        SetupRepositoryNotExists();
        SetupRepositorySaveSuccess();

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(Unit.Value, result);
        _repositoryMock.Verify(r => r.SaveLeadAsync(It.IsAny<Lead>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region Helper Methods

    private static ProcessLeadCommand CreateValidCommand()
    {
        return new ProcessLeadCommand
        {
            TenantId = "tenant-123",
            CorrelationId = Guid.NewGuid().ToString(),
            Email = "test@example.com",
            FirstName = "John",
            LastName = "Doe",
            Phone = "+1-555-1234",
            Company = "Acme Corp",
            Source = "website",
            Metadata = "{\"utm_source\":\"google\"}"
        };
    }

    private void SetupValidationSuccess()
    {
        _validatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<ProcessLeadCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
    }

    private void SetupValidationFailure(List<ValidationFailure> errors)
    {
        _validatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<ProcessLeadCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(errors));
    }

    private void SetupRepositoryNotExists()
    {
        _repositoryMock
            .Setup(r => r.ExistsByCorrelationIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
    }

    private void SetupRepositorySaveSuccess()
    {
        _repositoryMock
            .Setup(r => r.SaveLeadAsync(It.IsAny<Lead>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Lead lead, CancellationToken _) => lead with { Id = 42 });
    }

    #endregion
}

