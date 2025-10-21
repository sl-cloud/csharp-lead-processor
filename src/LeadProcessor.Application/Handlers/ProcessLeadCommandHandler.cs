using FluentValidation;
using LeadProcessor.Application.Commands;
using LeadProcessor.Domain.Entities;
using LeadProcessor.Domain.Exceptions;
using LeadProcessor.Domain.Repositories;
using LeadProcessor.Domain.Services;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Globalization;

namespace LeadProcessor.Application.Handlers;

/// <summary>
/// Handler for processing lead commands.
/// Implements idempotency, validation, and persistence logic for incoming leads.
/// </summary>
public class ProcessLeadCommandHandler(
    ILeadRepository repository,
    IDateTimeProvider dateTimeProvider,
    IValidator<ProcessLeadCommand> validator,
    ILogger<ProcessLeadCommandHandler> logger) : IRequestHandler<ProcessLeadCommand, Unit>
{
    /// <summary>
    /// Handles the ProcessLeadCommand by validating, checking for duplicates, and persisting the lead.
    /// </summary>
    /// <param name="request">The command containing lead data.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>Unit value indicating successful completion.</returns>
    /// <exception cref="ValidationException">Thrown when the command fails validation.</exception>
    /// <exception cref="DuplicateLeadException">Thrown when a lead with the same correlation ID already exists.</exception>
    public async Task<Unit> Handle(ProcessLeadCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Processing lead command for correlation ID {CorrelationId}, tenant {TenantId}, email {Email}",
            request.CorrelationId,
            request.TenantId,
            request.Email);

        try
        {
            // 1. Validate the command
            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                logger.LogWarning(
                    "Validation failed for correlation ID {CorrelationId}: {Errors}",
                    request.CorrelationId,
                    string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)));

                throw new ValidationException(validationResult.Errors);
            }

            // 2. Check for idempotency - prevent duplicate processing
            var exists = await repository.ExistsByCorrelationIdAsync(request.CorrelationId, cancellationToken);
            if (exists)
            {
                logger.LogInformation(
                    "Lead with correlation ID {CorrelationId} already exists. Skipping duplicate processing.",
                    request.CorrelationId);

                throw new DuplicateLeadException(request.CorrelationId);
            }

            // 3. Get current timestamp for entity creation
            var now = dateTimeProvider.UtcNow;

            // 4. Parse message timestamp for future audit trail enhancement
            DateTimeOffset? messageTimestamp = null;
            if (!string.IsNullOrWhiteSpace(request.MessageTimestamp))
            {
                if (DateTimeOffset.TryParse(
                    request.MessageTimestamp,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeUniversal,
                    out var parsedTimestamp))
                {
                    messageTimestamp = parsedTimestamp;
                    logger.LogDebug(
                        "Parsed message timestamp {MessageTimestamp} for correlation ID {CorrelationId}",
                        messageTimestamp,
                        request.CorrelationId);
                }
                else
                {
                    logger.LogWarning(
                        "Failed to parse message timestamp '{MessageTimestamp}' for correlation ID {CorrelationId}",
                        request.MessageTimestamp,
                        request.CorrelationId);
                }
            }

            // 5. Map command to domain entity
            var lead = new Lead
            {
                TenantId = request.TenantId,
                CorrelationId = request.CorrelationId,
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Phone = request.Phone,
                Company = request.Company,
                Source = request.Source,
                Metadata = request.Metadata,
                CreatedAt = now,
                UpdatedAt = now
            };

            // 6. Persist to repository
            var savedLead = await repository.SaveLeadAsync(lead, cancellationToken);

            logger.LogInformation(
                "Successfully processed lead {LeadId} for correlation ID {CorrelationId}, tenant {TenantId}",
                savedLead.Id,
                savedLead.CorrelationId,
                savedLead.TenantId);

            return Unit.Value;
        }
        catch (ValidationException)
        {
            // Re-throw validation exceptions without logging as error
            // (already logged as warning above)
            throw;
        }
        catch (DuplicateLeadException)
        {
            // Re-throw duplicate exceptions without logging as error
            // (already logged as information above - this is expected behavior)
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Unexpected error processing lead for correlation ID {CorrelationId}, tenant {TenantId}",
                request.CorrelationId,
                request.TenantId);
            throw;
        }
    }
}

