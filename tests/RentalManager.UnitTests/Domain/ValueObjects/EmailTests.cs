// Copyright (c) Core. All rights reserved.

using RentalManager.Domain.ValueObjects;
using FluentAssertions;

namespace RentalManager.UnitTests.Domain.ValueObjects;

[TestFixture]
public class EmailTests
{
    [Test]
    public void Create_WithValidEmail_ShouldCreateEmail()
    {
        // Arrange
        var emailValue = "test@example.com";

        // Act
        var email = Email.Create(emailValue);

        // Assert
        email.Value.Should().Be(emailValue);
    }

    [Test]
    [TestCase("user@domain.com")]
    [TestCase("user.name@domain.com")]
    [TestCase("user+tag@domain.com")]
    [TestCase("user123@domain123.com")]
    [TestCase("user@sub.domain.com")]
    [TestCase("user@domain.co.uk")]
    [TestCase("user@domain-name.com")]
    [TestCase("user_name@domain.com")]
    public void Create_WithValidEmailFormats_ShouldCreateEmail(string emailValue)
    {
        // Act
        var email = Email.Create(emailValue);

        // Assert
        email.Value.Should().Be(emailValue.ToLowerInvariant());
    }

    [Test]
    public void Create_WithEmptyEmail_ShouldThrowArgumentException()
    {
        // Arrange & Act & Assert
        var action = () => Email.Create(string.Empty);
        action.Should().Throw<ArgumentException>()
            .WithMessage("Email cannot be null or empty (Parameter 'value')");
    }

    [Test]
    public void Create_WithNullEmail_ShouldThrowArgumentException()
    {
        // Arrange & Act & Assert
        var action = () => Email.Create(null!);
        action.Should().Throw<ArgumentException>()
            .WithMessage("Email cannot be null or empty (Parameter 'value')");
    }

    [Test]
    public void Create_WithWhitespaceEmail_ShouldThrowArgumentException()
    {
        // Arrange & Act & Assert
        var action = () => Email.Create("   ");
        action.Should().Throw<ArgumentException>()
            .WithMessage("Email cannot be null or empty (Parameter 'value')");
    }

    [Test]
    [TestCase("invalid-email")]
    [TestCase("@domain.com")]
    [TestCase("user@")]
    [TestCase("user@domain")]
    [TestCase("user..name@domain.com")]
    [TestCase("user@domain..com")]
    [TestCase("user@.domain.com")]
    [TestCase("user@domain.")]
    [TestCase("user name@domain.com")]
    [TestCase("user@domain com")]
    public void Create_WithInvalidEmailFormats_ShouldThrowArgumentException(string invalidEmail)
    {
        // Arrange & Act & Assert
        var action = () => Email.Create(invalidEmail);
        action.Should().Throw<ArgumentException>()
            .WithMessage("Invalid email format (Parameter 'value')");
    }

    [Test]
    public void Create_WithUppercaseEmail_ShouldConvertToLowercase()
    {
        // Arrange
        var emailValue = "TEST@EXAMPLE.COM";

        // Act
        var email = Email.Create(emailValue);

        // Assert
        email.Value.Should().Be("test@example.com");
    }

    [Test]
    public void Equals_WithSameEmail_ShouldReturnTrue()
    {
        // Arrange
        var email1 = Email.Create("test@example.com");
        var email2 = Email.Create("test@example.com");

        // Act & Assert
        email1.Should().Be(email2);
        email1.Equals(email2).Should().BeTrue();
        (email1 == email2).Should().BeTrue();
        (email1 != email2).Should().BeFalse();
    }

    [Test]
    public void Equals_WithDifferentEmail_ShouldReturnFalse()
    {
        // Arrange
        var email1 = Email.Create("test1@example.com");
        var email2 = Email.Create("test2@example.com");

        // Act & Assert
        email1.Should().NotBe(email2);
        email1.Equals(email2).Should().BeFalse();
        (email1 == email2).Should().BeFalse();
        (email1 != email2).Should().BeTrue();
    }

    [Test]
    public void Equals_WithNull_ShouldReturnFalse()
    {
        // Arrange
        var email = Email.Create("test@example.com");

        // Act & Assert
        email.Equals(null).Should().BeFalse();
        (email == null).Should().BeFalse();
        (email != null).Should().BeTrue();
    }

    [Test]
    public void GetHashCode_WithSameEmail_ShouldReturnSameHashCode()
    {
        // Arrange
        var email1 = Email.Create("test@example.com");
        var email2 = Email.Create("test@example.com");

        // Act & Assert
        email1.GetHashCode().Should().Be(email2.GetHashCode());
    }

    [Test]
    public void GetHashCode_WithDifferentEmail_ShouldReturnDifferentHashCode()
    {
        // Arrange
        var email1 = Email.Create("test1@example.com");
        var email2 = Email.Create("test2@example.com");

        // Act & Assert
        email1.GetHashCode().Should().NotBe(email2.GetHashCode());
    }

    [Test]
    public void ToString_ShouldReturnEmailValue()
    {
        // Arrange
        var email = Email.Create("test@example.com");

        // Act
        var result = email.ToString();

        // Assert
        result.Should().Be("test@example.com");
    }

    [Test]
    public void ImplicitConversion_ToString_ShouldReturnEmailValue()
    {
        // Arrange
        var email = Email.Create("test@example.com");

        // Act
        string emailValue = email;

        // Assert
        emailValue.Should().Be("test@example.com");
    }
}
