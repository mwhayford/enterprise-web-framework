# Integration Testing

## Overview

This document describes the integration testing strategy for the Core application. Integration tests verify that different components of the system work together correctly, including database, caching, search, and messaging infrastructure.

## Technology Stack

- **Testing Framework**: NUnit 4.2.2
- **Test Containers**: Testcontainers 4.0.0
- **Assertions**: FluentAssertions 6.12.0
- **Database Cleanup**: Respawn 6.2.1
- **Mocking**: Moq 4.20.72
- **Test Host**: Microsoft.AspNetRentalManager.Mvc.Testing 9.0.0

## Test Infrastructure

### TestContainers

We use TestContainers to spin up real instances of dependencies during tests:

- **PostgreSQL 16**: Primary database
- **Redis 7**: Caching layer
- **Elasticsearch 8.11**: Full-text search
- **Kafka**: Event streaming (when needed)

### Base Test Class

All integration tests inherit from `IntegrationTestBase`, which provides:

- Automatic container lifecycle management
- Database setup and migrations
- Database cleanup between tests using Respawn
- Service provider configuration
- Connection string access

```csharp
[TestFixture]
public class MyIntegrationTests : IntegrationTestBase
{
    [Test]
    public async Task MyTest()
    {
        // DbContext is available
        // ServiceProvider is configured
        // Database is clean
    }
}
```

## Test Categories

### 1. Database Tests (`DatabaseTests.cs`)

Tests that verify database operations:

- ✅ Database connectivity
- ✅ Entity persistence (Payment, Subscription, PaymentMethod)
- ✅ Query operations
- ✅ Transaction management
- ✅ Database cleanup between tests

**Example**:
```csharp
[Test]
public async Task Database_ShouldPersistPayment()
{
    // Arrange
    var amount = Money.Create(100.50m, "USD");
    var payment = new Payment(...);

    // Act
    DbContext!.Payments.Add(payment);
    await DbContext.SaveChangesAsync();

    // Assert
    var saved = await DbContext.Payments.FindAsync(payment.Id);
    saved.Should().NotBeNull();
}
```

### 2. Elasticsearch Tests (`ElasticsearchTests.cs`)

Tests that verify search functionality:

- ✅ Document indexing
- ✅ Search queries
- ✅ Index creation/deletion
- ✅ Pagination

**Example**:
```csharp
[Test]
public async Task Elasticsearch_ShouldIndexDocument()
{
    // Arrange
    var document = new { Id = "1", Name = "Test" };

    // Act
    await _elasticsearchService.IndexDocumentAsync("test", "1", document);
    await Task.Delay(1000); // Wait for indexing

    // Assert
    var result = await _elasticsearchService.SearchAsync(...);
    result.TotalCount.Should().BeGreaterThan(0);
}
```

### 3. Redis Tests (`RedisTests.cs`)

Tests that verify caching operations:

- ✅ String operations (get/set)
- ✅ Key expiration
- ✅ Hash operations
- ✅ List operations (queue)
- ✅ Set operations (unique values)
- ✅ Atomic counters

**Example**:
```csharp
[Test]
public async Task Redis_ShouldSetWithExpiration()
{
    // Arrange
    var key = "test:key";
    var expiration = TimeSpan.FromSeconds(2);

    // Act
    await _database.StringSetAsync(key, "value", expiration);
    await Task.Delay(2500);

    // Assert
    var retrieved = await _database.StringGetAsync(key);
    retrieved.IsNullOrEmpty.Should().BeTrue();
}
```

## Running Integration Tests

### Prerequisites

1. **Docker Desktop** must be running
2. **Port Availability**:
   - 5434: PostgreSQL
   - 6381: Redis
   - 9200: Elasticsearch

### Run All Tests

```bash
dotnet test tests/RentalManager.IntegrationTests/RentalManager.IntegrationTests.csproj
```

### Run Specific Test Class

```bash
dotnet test tests/RentalManager.IntegrationTests/RentalManager.IntegrationTests.csproj --filter "FullyQualifiedName~DatabaseTests"
```

### Run with Detailed Output

```bash
dotnet test tests/RentalManager.IntegrationTests/RentalManager.IntegrationTests.csproj --logger "console;verbosity=detailed"
```

## Test Lifecycle

### One-Time Setup (Before All Tests)
1. Start required containers (PostgreSQL, Redis, etc.)
2. Apply database migrations
3. Configure services
4. Initialize Respawner for database cleanup

### Per-Test Setup
1. Reset database to clean state using Respawner
2. Flush Redis (if used)
3. Clean Elasticsearch indices (if used)

### Per-Test Teardown
- Automatic cleanup via `[TearDown]` methods

### One-Time Teardown (After All Tests)
1. Dispose DbContext
2. Dispose ServiceProvider
3. Stop and dispose containers

## Best Practices

### 1. Test Isolation

Each test should be independent and not rely on the state from previous tests:

```csharp
[SetUp]
public async Task SetUp()
{
    // Reset database to clean state
    await _respawner.ResetAsync(connectionString);
}
```

### 2. Arrange-Act-Assert Pattern

Use the AAA pattern for clarity:

```csharp
[Test]
public async Task Example()
{
    // Arrange - Set up test data
    var data = CreateTestData();

    // Act - Execute the operation
    var result = await PerformOperation(data);

    // Assert - Verify the outcome
    result.Should().NotBeNull();
}
```

### 3. Descriptive Test Names

Test names should clearly describe what is being tested:

```csharp
// Good
[Test]
public async Task Database_ShouldPersistPayment()

// Bad
[Test]
public async Task Test1()
```

### 4. Use FluentAssertions

FluentAssertions provides readable assertions:

```csharp
// Preferred
result.Should().NotBeNull();
result.Should().HaveCount(3);
result.Status.Should().Be(PaymentStatus.Succeeded);

// Avoid
Assert.NotNull(result);
Assert.AreEqual(3, result.Count);
Assert.AreEqual(PaymentStatus.Succeeded, result.Status);
```

### 5. Wait for Asynchronous Operations

Some operations (like Elasticsearch indexing) are asynchronous:

```csharp
await _elasticsearch.IndexDocumentAsync(...);
await Task.Delay(1000); // Wait for indexing
var result = await _elasticsearch.SearchAsync(...);
```

### 6. Clean Up Resources

Always dispose of resources in teardown methods:

```csharp
[OneTimeTearDown]
public async Task OneTimeTearDown()
{
    _dbContext?.Dispose();
    await _container?.DisposeAsync();
}
```

## Troubleshooting

### Docker Connection Issues

**Problem**: "Cannot connect to Docker daemon"

**Solution**:
```bash
# Ensure Docker Desktop is running
docker ps

# On Windows, check Docker Desktop settings
# - Ensure "Expose daemon on tcp://localhost:2375" is enabled
```

### Port Conflicts

**Problem**: "Port already in use"

**Solution**:
```bash
# Find and kill process using the port (Windows)
netstat -ano | findstr :5434
taskkill /PID <process_id> /F

# Or change the port mapping in test code
_postgresContainer = new PostgreSqlBuilder()
    .WithPortBinding(5435, 5432) // Use different host port
    .Build();
```

### Slow Test Execution

**Problem**: Integration tests are slow

**Optimization Tips**:
- Use `[OneTimeSetUp]` for expensive operations (container startup)
- Reuse containers across tests in the same fixture
- Use Respawn for database cleanup instead of recreating the database
- Run tests in parallel where possible (be careful with shared resources)

### Container Startup Timeouts

**Problem**: Container fails to start within timeout

**Solution**:
```csharp
_postgresContainer = new PostgreSqlBuilder()
    .WithWaitStrategy(
        Wait.ForUnixContainer()
            .UntilPortIsAvailable(5432)
            .WithTimeout(TimeSpan.FromMinutes(2)) // Increase timeout
    )
    .Build();
```

## CI/CD Integration

Integration tests should run in CI/CD pipelines:

```yaml
# .github/workflows/test.yml
name: Integration Tests

on: [push, pull_request]

jobs:
  integration-tests:
    runs-on: ubuntu-latest

    steps:
      - uses: actions/checkout@v3

      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '9.0.x'

      - name: Restore dependencies
        run: dotnet restore

      - name: Run Integration Tests
        run: dotnet test tests/RentalManager.IntegrationTests --logger trx --collect:"XPlat Code Coverage"

      - name: Upload Test Results
        uses: actions/upload-artifact@v3
        if: always()
        with:
          name: test-results
          path: '**/*.trx'
```

## Performance Benchmarks

Expected test execution times:

| Test Suite | Tests | Duration |
|-----------|-------|----------|
| DatabaseTests | 8 | ~15s |
| ElasticsearchTests | 4 | ~10s |
| RedisTests | 8 | ~8s |
| **Total** | **20** | **~33s** |

*Note: First run will be slower due to Docker image pulls*

## Future Enhancements

- [ ] Add Kafka integration tests
- [ ] Add API endpoint integration tests using `WebApplicationFactory`
- [ ] Add Stripe webhook integration tests
- [ ] Add authentication/authorization integration tests
- [ ] Add load testing with NBomber
- [ ] Add database migration tests
- [ ] Add distributed tracing validation tests

## References

- [TestContainers for .NET](https://github.com/testcontainers/testcontainers-dotnet)
- [NUnit Documentation](https://docs.nunit.org/)
- [FluentAssertions Documentation](https://fluentassertions.com/)
- [Respawn Documentation](https://github.com/jbogard/Respawn)

