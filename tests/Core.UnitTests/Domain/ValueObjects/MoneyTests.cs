using Core.Domain.ValueObjects;
using FluentAssertions;

namespace Core.UnitTests.Domain.ValueObjects;

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
    public void Create_WithZeroAmount_ShouldCreateMoney()
    {
        // Arrange & Act
        var money = Money.Create(0m, "USD");

        // Assert
        money.Amount.Should().Be(0m);
        money.Currency.Should().Be("USD");
    }

    [Test]
    public void Create_WithNegativeAmount_ShouldThrowArgumentException()
    {
        // Arrange & Act & Assert
        var action = () => Money.Create(-50.25m, "EUR");
        action.Should().Throw<ArgumentException>()
            .WithMessage("Amount cannot be negative (Parameter 'amount')");
    }

    [Test]
    public void Create_WithEmptyCurrency_ShouldThrowArgumentException()
    {
        // Arrange & Act & Assert
        var action = () => Money.Create(100m, string.Empty);
        action.Should().Throw<ArgumentException>()
            .WithMessage("Currency cannot be null or empty (Parameter 'currency')");
    }

    [Test]
    public void Create_WithNullCurrency_ShouldThrowArgumentException()
    {
        // Arrange & Act & Assert
        var action = () => Money.Create(100m, null!);
        action.Should().Throw<ArgumentException>()
            .WithMessage("Currency cannot be null or empty (Parameter 'currency')");
    }

    [Test]
    public void Create_WithWhitespaceCurrency_ShouldThrowArgumentException()
    {
        // Arrange & Act & Assert
        var action = () => Money.Create(100m, "   ");
        action.Should().Throw<ArgumentException>()
            .WithMessage("Currency cannot be null or empty (Parameter 'currency')");
    }

    [Test]
    public void Create_WithDefaultCurrency_ShouldUseUSD()
    {
        // Arrange & Act
        var money = Money.Create(100m);

        // Assert
        money.Amount.Should().Be(100m);
        money.Currency.Should().Be("USD");
    }

    [Test]
    public void Create_WithLowercaseCurrency_ShouldConvertToUppercase()
    {
        // Arrange & Act
        var money = Money.Create(100m, "eur");

        // Assert
        money.Currency.Should().Be("EUR");
    }

    [Test]
    public void Zero_WithDefaultCurrency_ShouldCreateZeroMoney()
    {
        // Arrange & Act
        var money = Money.Zero();

        // Assert
        money.Amount.Should().Be(0m);
        money.Currency.Should().Be("USD");
    }

    [Test]
    public void Zero_WithSpecificCurrency_ShouldCreateZeroMoney()
    {
        // Arrange & Act
        var money = Money.Zero("EUR");

        // Assert
        money.Amount.Should().Be(0m);
        money.Currency.Should().Be("EUR");
    }

    [Test]
    public void Equals_WithSameAmountAndCurrency_ShouldReturnTrue()
    {
        // Arrange
        var money1 = Money.Create(100.50m, "USD");
        var money2 = Money.Create(100.50m, "USD");

        // Act & Assert
        money1.Should().Be(money2);
        money1.Equals(money2).Should().BeTrue();
        (money1 == money2).Should().BeTrue();
        (money1 != money2).Should().BeFalse();
    }

    [Test]
    public void Equals_WithDifferentAmount_ShouldReturnFalse()
    {
        // Arrange
        var money1 = Money.Create(100.50m, "USD");
        var money2 = Money.Create(200.50m, "USD");

        // Act & Assert
        money1.Should().NotBe(money2);
        money1.Equals(money2).Should().BeFalse();
        (money1 == money2).Should().BeFalse();
        (money1 != money2).Should().BeTrue();
    }

    [Test]
    public void Equals_WithDifferentCurrency_ShouldReturnFalse()
    {
        // Arrange
        var money1 = Money.Create(100.50m, "USD");
        var money2 = Money.Create(100.50m, "EUR");

        // Act & Assert
        money1.Should().NotBe(money2);
        money1.Equals(money2).Should().BeFalse();
        (money1 == money2).Should().BeFalse();
        (money1 != money2).Should().BeTrue();
    }

    [Test]
    public void Equals_WithNull_ShouldReturnFalse()
    {
        // Arrange
        var money = Money.Create(100.50m, "USD");

        // Act & Assert
        money.Equals(null).Should().BeFalse();
        (money == null).Should().BeFalse();
        (money != null).Should().BeTrue();
    }

    [Test]
    public void GetHashCode_WithSameAmountAndCurrency_ShouldReturnSameHashCode()
    {
        // Arrange
        var money1 = Money.Create(100.50m, "USD");
        var money2 = Money.Create(100.50m, "USD");

        // Act & Assert
        money1.GetHashCode().Should().Be(money2.GetHashCode());
    }

    [Test]
    public void GetHashCode_WithDifferentAmount_ShouldReturnDifferentHashCode()
    {
        // Arrange
        var money1 = Money.Create(100.50m, "USD");
        var money2 = Money.Create(200.50m, "USD");

        // Act & Assert
        money1.GetHashCode().Should().NotBe(money2.GetHashCode());
    }

    [Test]
    public void GetHashCode_WithDifferentCurrency_ShouldReturnDifferentHashCode()
    {
        // Arrange
        var money1 = Money.Create(100.50m, "USD");
        var money2 = Money.Create(100.50m, "EUR");

        // Act & Assert
        money1.GetHashCode().Should().NotBe(money2.GetHashCode());
    }

    [Test]
    public void ToString_ShouldReturnFormattedString()
    {
        // Arrange
        var money = Money.Create(100.50m, "USD");

        // Act
        var result = money.ToString();

        // Assert
        result.Should().Be("$100.50 USD");
    }

    [Test]
    public void ToString_WithZeroAmount_ShouldReturnFormattedString()
    {
        // Arrange
        var money = Money.Create(0m, "EUR");

        // Act
        var result = money.ToString();

        // Assert
        result.Should().Be("€0.00 EUR");
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

    [Test]
    public void Add_WithDifferentCurrencies_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var money1 = Money.Create(100.50m, "USD");
        var money2 = Money.Create(50.25m, "EUR");

        // Act & Assert
        var action = () => money1 + money2;
        action.Should().Throw<InvalidOperationException>()
            .WithMessage("Cannot add money with different currencies");
    }

    [Test]
    public void Subtract_WithSameCurrency_ShouldReturnCorrectDifference()
    {
        // Arrange
        var money1 = Money.Create(100.50m, "USD");
        var money2 = Money.Create(50.25m, "USD");

        // Act
        var result = money1 - money2;

        // Assert
        result.Amount.Should().Be(50.25m);
        result.Currency.Should().Be("USD");
    }

    [Test]
    public void Subtract_WithDifferentCurrencies_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var money1 = Money.Create(100.50m, "USD");
        var money2 = Money.Create(50.25m, "EUR");

        // Act & Assert
        var action = () => money1 - money2;
        action.Should().Throw<InvalidOperationException>()
            .WithMessage("Cannot subtract money with different currencies");
    }
}