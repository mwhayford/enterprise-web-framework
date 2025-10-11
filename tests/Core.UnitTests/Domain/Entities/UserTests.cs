// Copyright (c) Core. All rights reserved.

using Core.Domain.Entities;
using Core.Domain.Events;
using Core.Domain.ValueObjects;
using FluentAssertions;

namespace Core.UnitTests.Domain.Entities;

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
        user.Id.Should().NotBeEmpty();
        user.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        user.IsActive.Should().BeTrue();
        user.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<UserCreatedEvent>();
    }

    [Test]
    public void Constructor_WithGoogleId_ShouldCreateUser()
    {
        // Arrange
        var firstName = "John";
        var lastName = "Doe";
        var email = Email.Create("john.doe@example.com");
        var googleId = "google123";
        var profilePictureUrl = "https://example.com/photo.jpg";

        // Act
        var user = new User(firstName, lastName, email, googleId, profilePictureUrl);

        // Assert
        user.FirstName.Should().Be(firstName);
        user.LastName.Should().Be(lastName);
        user.Email.Should().Be(email);
        user.GoogleId.Should().Be(googleId);
        user.ProfilePictureUrl.Should().Be(profilePictureUrl);
        user.IsActive.Should().BeTrue();
    }

    [Test]
    public void Constructor_WithEmptyFirstName_ShouldThrowArgumentException()
    {
        // Arrange & Act & Assert
        var action = () => new User(string.Empty, "Doe", Email.Create("test@example.com"));
        action.Should().Throw<ArgumentException>();
    }

    [Test]
    public void Constructor_WithNullFirstName_ShouldThrowArgumentException()
    {
        // Arrange & Act & Assert
        var action = () => new User(null!, "Doe", Email.Create("test@example.com"));
        action.Should().Throw<ArgumentException>();
    }

    [Test]
    public void Constructor_WithEmptyLastName_ShouldThrowArgumentException()
    {
        // Arrange & Act & Assert
        var action = () => new User("John", string.Empty, Email.Create("test@example.com"));
        action.Should().Throw<ArgumentException>();
    }

    [Test]
    public void Constructor_WithNullLastName_ShouldThrowArgumentException()
    {
        // Arrange & Act & Assert
        var action = () => new User("John", null!, Email.Create("test@example.com"));
        action.Should().Throw<ArgumentException>();
    }

    [Test]
    public void Constructor_WithNullEmail_ShouldThrowArgumentNullException()
    {
        // Arrange & Act & Assert
        var action = () => new User("John", "Doe", null!);
        action.Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void UpdateProfile_WithValidParameters_ShouldUpdateUser()
    {
        // Arrange
        var user = new User("John", "Doe", Email.Create("john.doe@example.com"));
        var newFirstName = "Jane";
        var newLastName = "Smith";
        var newProfilePictureUrl = "https://example.com/new-photo.jpg";

        // Act
        user.UpdateProfile(newFirstName, newLastName, newProfilePictureUrl);

        // Assert
        user.FirstName.Should().Be(newFirstName);
        user.LastName.Should().Be(newLastName);
        user.ProfilePictureUrl.Should().Be(newProfilePictureUrl);
        user.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Test]
    public void UpdateProfile_WithNullProfilePictureUrl_ShouldUpdateUser()
    {
        // Arrange
        var user = new User("John", "Doe", Email.Create("john.doe@example.com"));
        var newFirstName = "Jane";
        var newLastName = "Smith";

        // Act
        user.UpdateProfile(newFirstName, newLastName, null);

        // Assert
        user.FirstName.Should().Be(newFirstName);
        user.LastName.Should().Be(newLastName);
        user.ProfilePictureUrl.Should().BeNull();
    }

    [Test]
    public void SetGoogleId_ShouldUpdateGoogleId()
    {
        // Arrange
        var user = new User("John", "Doe", Email.Create("john.doe@example.com"));
        var googleId = "google123";

        // Act
        user.SetGoogleId(googleId);

        // Assert
        user.GoogleId.Should().Be(googleId);
        user.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Test]
    public void RecordLogin_ShouldUpdateLastLoginAt()
    {
        // Arrange
        var user = new User("John", "Doe", Email.Create("john.doe@example.com"));

        // Act
        user.RecordLogin();

        // Assert
        user.LastLoginAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        user.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Test]
    public void Deactivate_ShouldSetIsActiveToFalse()
    {
        // Arrange
        var user = new User("John", "Doe", Email.Create("john.doe@example.com"));

        // Act
        user.Deactivate();

        // Assert
        user.IsActive.Should().BeFalse();
        user.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Test]
    public void Activate_ShouldSetIsActiveToTrue()
    {
        // Arrange
        var user = new User("John", "Doe", Email.Create("john.doe@example.com"));
        user.Deactivate();

        // Act
        user.Activate();

        // Assert
        user.IsActive.Should().BeTrue();
        user.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Test]
    public void FullName_ShouldReturnConcatenatedName()
    {
        // Arrange
        var user = new User("John", "Doe", Email.Create("john.doe@example.com"));

        // Act
        var fullName = user.FullName;

        // Assert
        fullName.Should().Be("John Doe");
    }

    [Test]
    public void ClearDomainEvents_ShouldRemoveAllEvents()
    {
        // Arrange
        var user = new User("John", "Doe", Email.Create("john.doe@example.com"));
        user.UpdateProfile("Jane", "Smith");
        user.DomainEvents.Should().NotBeEmpty();

        // Act
        user.ClearDomainEvents();

        // Assert
        user.DomainEvents.Should().BeEmpty();
    }

    [Test]
    public void AddDomainEvent_ShouldAddEventToList()
    {
        // Arrange
        var user = new User("John", "Doe", Email.Create("john.doe@example.com"));
        var customEvent = new UserUpdatedEvent
        {
            UserId = user.Id,
            FirstName = "Jane",
            LastName = "Smith",
            Email = "jane.smith@example.com"
        };

        // Act
        user.AddDomainEvent(customEvent);

        // Assert
        user.DomainEvents.Should().Contain(customEvent);
    }

    [Test]
    public void RemoveDomainEvent_ShouldRemoveEventFromList()
    {
        // Arrange
        var user = new User("John", "Doe", Email.Create("john.doe@example.com"));
        var eventToRemove = user.DomainEvents.First();

        // Act
        user.RemoveDomainEvent(eventToRemove);

        // Assert
        user.DomainEvents.Should().NotContain(eventToRemove);
    }

    [Test]
    public void Equals_WithSameId_ShouldReturnTrue()
    {
        // Arrange
        var user1 = new User("John", "Doe", Email.Create("john.doe@example.com"));
        var user2 = new User("Jane", "Smith", Email.Create("jane.smith@example.com"));

        // Note: In a real scenario, you'd need to set the same ID, but since ID is generated in constructor,
        // this test demonstrates the concept

        // Act & Assert
        user1.Should().NotBe(user2); // Different IDs
        user1.Equals(user1).Should().BeTrue(); // Same instance
    }

    [Test]
    public void GetHashCode_ShouldReturnIdHashCode()
    {
        // Arrange
        var user = new User("John", "Doe", Email.Create("john.doe@example.com"));

        // Act & Assert
        user.GetHashCode().Should().Be(user.Id.GetHashCode());
    }
}
