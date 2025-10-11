# Unit Testing Documentation

## Overview

This project implements comprehensive unit testing using NUnit, Moq, FluentAssertions, and AutoFixture to ensure code quality and reliability.

## Test Structure

### Test Projects

- **Core.UnitTests**: Contains unit tests for domain entities, value objects, and application layer components
- **Core.IntegrationTests**: Contains integration tests (to be implemented)
- **Core.E2ETests**: Contains end-to-end tests (to be implemented)

### Test Categories

#### Domain Layer Tests
- **Value Objects**: `Money`, `Email` validation and behavior
- **Entities**: `User`, `Payment` business logic and state management
- **Domain Events**: Event publishing and handling

#### Application Layer Tests
- **Command Handlers**: CQRS command processing logic
- **Query Handlers**: Data retrieval and transformation
- **Validators**: Input validation rules
- **Mappings**: Object-to-object transformations

## Testing Tools and Libraries

### Core Testing Framework
- **NUnit 4.2.2**: Primary testing framework
- **NUnit3TestAdapter 4.6.0**: Visual Studio integration
- **NUnit.Analyzers 4.4.0**: Code analysis for test quality

### Mocking and Assertions
- **Moq 4.20.72**: Mocking framework for dependencies
- **FluentAssertions 6.12.0**: Fluent assertion library for readable tests
- **AutoFixture 4.18.1**: Test data generation
- **AutoFixture.NUnit3 4.18.1**: NUnit integration for AutoFixture

### Code Coverage
- **coverlet.collector 6.0.2**: Code coverage collection

## Test Implementation Examples

### Domain Entity Testing

```csharp
[TestFixture]
public class UserTests
{
    [Test]
    public void Constructor_WithValidParameters_ShouldCreateUser()
    {
        // Arrange
        var firstName = "John";
        var lastName = "Doe";
        var email = Email.Create("john.doe@example.com");

        // Act
        var user = new User(firstName, lastName, email);

        // Assert
        user.FirstName.Should().Be(firstName);
        user.LastName.Should().Be(lastName);
        user.Email.Should().Be(email);
        user.IsActive.Should().BeTrue();
        user.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<UserCreatedEvent>();
    }
}
```

### Value Object Testing

```csharp
[TestFixture]
public class MoneyTests
{
    [Test]
    public void Create_WithValidAmountAndCurrency_ShouldCreateMoney()
    {
        // Arrange
        var amount = 100.50m;
        var currency = "USD";

        // Act
        var money = Money.Create(amount, currency);

        // Assert
        money.Amount.Should().Be(amount);
        money.Currency.Should().Be(currency);
    }

    [Test]
    public void Add_WithSameCurrency_ShouldReturnCorrectSum()
    {
        // Arrange
        var money1 = Money.Create(100.50m, "USD");
        var money2 = Money.Create(50.25m, "USD");

        // Act
        var result = money1 + money2;

        // Assert
        result.Amount.Should().Be(150.75m);
        result.Currency.Should().Be("USD");
    }
}
```

### Command Handler Testing

```csharp
[TestFixture]
public class ProcessPaymentCommandHandlerTests
{
    private Mock<IPaymentService> _paymentServiceMock;
    private ProcessPaymentCommandHandler _handler;

    [SetUp]
    public void SetUp()
    {
        _paymentServiceMock = new Mock<IPaymentService>();
        _handler = new ProcessPaymentCommandHandler(_paymentServiceMock.Object);
    }

    [Test]
    public async Task Handle_WithValidCommand_ShouldReturnPayment()
    {
        // Arrange
        var command = new ProcessPaymentCommand
        {
            UserId = Guid.NewGuid(),
            Amount = 100.50m,
            Currency = "USD",
            PaymentMethodType = PaymentMethodType.Card,
            Description = "Test payment"
        };

        var expectedPayment = new Payment(
            command.UserId,
            Money.Create(command.Amount, command.Currency),
            command.PaymentMethodType,
            command.Description);

        _paymentServiceMock
            .Setup(x => x.ProcessPaymentAsync(
                command.UserId,
                It.IsAny<Money>(),
                command.PaymentMethodType,
                It.IsAny<string?>(),
                It.IsAny<string?>()))
            .ReturnsAsync(expectedPayment);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.UserId.Should().Be(command.UserId);
        result.Amount.Should().Be(command.Amount);
        result.Currency.Should().Be(command.Currency);
    }
}
```

## Test Coverage Areas

### Domain Layer (100% Coverage Target)
- ✅ Value object validation and behavior
- ✅ Entity business logic and state transitions
- ✅ Domain event publishing
- ✅ Business rule enforcement
- ⚠️ Input validation (needs implementation)

### Application Layer (90% Coverage Target)
- ✅ Command handler logic
- ✅ Query handler logic
- ✅ Service integration
- ✅ Error handling
- ⚠️ Validation logic (needs implementation)

## Current Test Results

### Test Statistics
- **Total Tests**: 113
- **Passed**: 93 (82.3%)
- **Failed**: 20 (17.7%)

### Test Categories
- **Domain Tests**: 67 tests (59.3%)
- **Application Tests**: 46 tests (40.7%)

### Common Test Patterns

#### Arrange-Act-Assert (AAA)
All tests follow the AAA pattern for clarity and consistency.

#### Test Naming Convention
- `MethodName_Scenario_ExpectedBehavior`
- Example: `Create_WithValidAmountAndCurrency_ShouldCreateMoney`

#### Mock Setup Patterns
- Use `It.IsAny<T>()` for optional parameters
- Use `It.Is<T>(predicate)` for specific value matching
- Verify method calls with `Times.Once`, `Times.Never`, etc.

#### Exception Testing
```csharp
[Test]
public void Create_WithInvalidInput_ShouldThrowArgumentException()
{
    // Arrange & Act & Assert
    var action = () => Money.Create(-100m, "USD");
    action.Should().Throw<ArgumentException>()
        .WithMessage("Amount cannot be negative (Parameter 'amount')");
}
```

## Running Tests

### Command Line
```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"

# Run specific test class
dotnet test --filter "ClassName=MoneyTests"

# Run with detailed output
dotnet test --verbosity normal
```

### Visual Studio
- Use Test Explorer to run individual tests or test suites
- Set breakpoints in tests for debugging
- View test results and coverage reports

## Test Configuration

### Project Configuration
- Target Framework: .NET 9.0
- Nullable reference types enabled
- Implicit usings enabled
- Treat warnings as errors: false (for test projects)

### StyleCop Integration
- File headers required
- Naming conventions enforced
- Code formatting standards applied

## Best Practices

### Test Organization
- Group related tests in test classes
- Use descriptive test names
- Follow consistent naming patterns
- Organize tests by functionality

### Mock Usage
- Mock external dependencies only
- Verify important interactions
- Use realistic test data
- Avoid over-mocking

### Assertions
- Use FluentAssertions for readable assertions
- Test both positive and negative scenarios
- Verify all important properties
- Test edge cases and boundary conditions

### Test Data
- Use AutoFixture for generating test data
- Create realistic test scenarios
- Test with various input combinations
- Include edge cases (null, empty, boundary values)

## Future Improvements

### Test Coverage Goals
- Achieve 90%+ code coverage
- Add integration tests for service interactions
- Implement end-to-end tests for critical user journeys

### Test Infrastructure
- Add test containers for database testing
- Implement test data builders
- Add performance testing capabilities
- Create test utilities and helpers

### Continuous Integration
- Integrate tests into CI/CD pipeline
- Add test result reporting
- Implement test coverage reporting
- Add test quality metrics

## Known Issues and Limitations

### Current Failures
1. **Domain Validation**: Some domain entities lack input validation
2. **Email Regex**: Current regex allows some invalid email formats
3. **Money Formatting**: Currency formatting uses default locale
4. **Hash Code Implementation**: Entities don't override GetHashCode properly

### Recommendations
1. Add input validation to domain constructors
2. Improve email validation regex
3. Implement proper currency formatting
4. Override GetHashCode in domain entities
5. Add more comprehensive error handling tests

## Test Maintenance

### Regular Tasks
- Review and update tests when requirements change
- Refactor tests to maintain readability
- Add tests for new features
- Remove obsolete tests
- Monitor test execution time

### Code Quality
- Keep tests simple and focused
- Avoid test interdependencies
- Use meaningful test data
- Document complex test scenarios
- Regular code reviews for test quality
