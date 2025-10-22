# Lead Processor Lambda

A .NET 8 AWS Lambda function that consumes lead creation events from SQS (published by a PHP Laravel CRM Gateway) and persists them to MySQL RDS using Clean Architecture principles.

## Architecture

This project implements Clean Architecture with the following layers:

- **Domain** - Core business entities and interfaces
- **Application** - MediatR handlers, DTOs, and FluentValidation validators (CQRS pattern)
- **Infrastructure** - EF Core, AWS SDK integrations, and repository implementations
- **Lambda** - AWS Lambda function entry point

## Project Structure

```
src/
├── LeadProcessor.Domain/           # Domain entities & interfaces
├── LeadProcessor.Application/      # MediatR handlers, DTOs, validators
├── LeadProcessor.Infrastructure/   # EF Core, AWS SDK, repositories
└── LeadProcessor.Lambda/           # Lambda function entry point

tests/
├── LeadProcessor.UnitTests/        # Unit tests for handlers & validators
├── LeadProcessor.IntegrationTests/ # Integration tests with Testcontainers
└── LeadProcessor.TestHelpers/      # Shared test utilities
```

## Package Management

This solution uses **Central Package Management** (CPM) via `Directory.Packages.props` for consistent package versioning across all projects.

### Benefits
- ✅ Single source of truth for all package versions
- ✅ Easy to update versions across entire solution
- ✅ Prevents version conflicts between projects
- ✅ Cleaner project files without inline versions

### How It Works

All package versions are defined in `Directory.Packages.props`:

```xml
<Project>
  <PropertyGroup>
    <ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>
  </PropertyGroup>
  
  <ItemGroup>
    <PackageVersion Include="MediatR" Version="12.4.1" />
    <PackageVersion Include="FluentValidation" Version="11.10.0" />
    <!-- ... other packages -->
  </ItemGroup>
</Project>
```

Individual projects reference packages without version attributes:

```xml
<ItemGroup>
  <PackageReference Include="MediatR" />
  <PackageReference Include="FluentValidation" />
</ItemGroup>
```

To update a package version, simply modify `Directory.Packages.props` and all projects automatically use the new version.

## Key Technologies

- **.NET 8** - Runtime platform
- **AWS Lambda** - Serverless compute
- **AWS SQS** - Message queue
- **MySQL (RDS)** - Relational database
- **MediatR** - CQRS pattern implementation
- **FluentValidation** - Input validation
- **Entity Framework Core** - ORM
- **Pomelo.EntityFrameworkCore.MySql** - MySQL provider
- **xUnit** - Unit testing framework
- **Testcontainers** - Integration testing with Docker

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [AWS CLI](https://aws.amazon.com/cli/) (for deployment)
- [Docker](https://www.docker.com/) (for integration tests with Testcontainers)

## Getting Started

### Build the Solution

```bash
dotnet restore
dotnet build
```

### Run Tests

```bash
# Run all tests
dotnet test

# Run only unit tests
dotnet test --filter "Category=Unit"

# Run only integration tests
dotnet test --filter "Category=Integration"
```

### Package Lambda Function

```bash
dotnet publish src/LeadProcessor.Lambda/LeadProcessor.Lambda.csproj \
  --configuration Release \
  --runtime linux-x64 \
  --self-contained false \
  --output ./publish

cd ./publish
zip -r ../lambda-deployment.zip .
```

## Integration with PHP CRM Gateway

This Lambda function consumes lead events from the [PHP CRM Gateway](https://github.com/sl-cloud/php-crm-gateway/).

### SQS Message Format

```json
{
  "tenant_id": "1",
  "correlation_id": "550e8400-e29b-41d4-a716-446655440000",
  "email": "john.doe@example.com",
  "first_name": "John",
  "last_name": "Doe",
  "phone": "+61412345678",
  "company": "Acme Corp",
  "source": "website",
  "metadata": {
    "utm_source": "google",
    "utm_medium": "cpc",
    "utm_campaign": "summer-sale"
  }
}
```

### SQS Message Attributes

| Attribute      | Type   | Description              |
|----------------|--------|--------------------------|
| EventType      | String | "LeadCreated"            |
| CorrelationId  | String | Unique message ID (UUID) |
| TenantId       | String | Tenant/User identifier   |
| Timestamp      | String | ISO 8601 timestamp       |

## Key Features

### Idempotency

Uses `correlation_id` as a unique constraint to prevent duplicate lead creation when SQS delivers the same message multiple times.

### Error Handling

- **Validation Errors** - Logged and moved to DLQ immediately
- **Transient Errors** - Automatic Lambda retry (max 3 attempts)
- **Unknown Errors** - Logged and moved to DLQ after retries exhausted

### Dead Letter Queue (DLQ)

Failed messages are moved to a DLQ after 3 retry attempts for manual investigation and reprocessing.

## CI/CD Pipeline

The project uses GitHub Actions for continuous integration:

- Builds on every push and pull request
- Runs unit and integration tests
- Packages Lambda deployment artifact
- Uploads deployment package for main/develop branches

See [`.github/workflows/ci.yml`](.github/workflows/ci.yml) for details.

## Development Guidelines

This project follows strict coding conventions:

- **Clean Architecture** - Dependencies point inward (Lambda → Application → Domain)
- **CQRS** - Commands and queries separated via MediatR
- **Async/Await** - All I/O operations are async with `CancellationToken` support
- **Dependency Injection** - Constructor injection throughout
- **`IDateTimeProvider`** - For testable time-dependent code
- **Structured Logging** - Using `ILogger<T>` with correlation IDs
- **XML Documentation** - All public APIs documented

See [`.cursor/rules/ai-agent.mdc`](.cursor/rules/ai-agent.mdc) for complete coding standards.

## Next Steps

### Phase 4: Infrastructure Layer
- Configure EF Core DbContext
- Implement repository pattern
- Set up AWS Secrets Manager service

### Phase 5: Lambda Function
- Implement SQS event handler
- Configure dependency injection
- Add error handling and DLQ logic

### Phase 6: Database Migrations
- Create initial migration for Leads table
- Add indexes for performance

### Phase 7: Testing
- Write unit tests with mocked dependencies
- Create integration tests with Testcontainers
- Build test helper utilities

### Phase 8: Deployment
- Create AWS SAM template
- Configure IAM permissions
- Set up CloudWatch monitoring

## Configuration

### AWS Resources Required

- **SQS Queue** - `leads-queue` (event source)
- **SQS DLQ** - `leads-queue-dlq` (failed messages)
- **RDS MySQL** - Database for storing leads
- **Secrets Manager** - RDS credentials
- **Lambda Function** - Lead processor function
- **IAM Role** - With permissions for SQS, RDS, Secrets Manager

### Environment Variables

- `AWS_REGION` - AWS region
- `AWS_SECRET_NAME` - Secrets Manager secret name for RDS credentials

## License

MIT

## Related Projects

- [PHP CRM Gateway](https://github.com/sl-cloud/php-crm-gateway/) - Laravel API that publishes lead events to SQS

