// Copyright (c) RentalManager. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using NUnit.Framework;
using RentalManager.Domain.Entities;
using RentalManager.Domain.ValueObjects;

namespace RentalManager.UnitTests.Domain;

public class PropertyTests
{
    [Test]
    public void Property_Creation_Should_Set_Properties_Correctly()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var address = PropertyAddress.Create("123 Main St", "Springfield", "IL", "62701", country: "USA");
        var rent = Money.Create(1500, "USD");
        var deposit = Money.Create(1500, "USD");
        var availableDate = DateTime.UtcNow.AddDays(30);

        // Act
        var property = new Property(
            ownerId,
            address,
            PropertyType.Apartment,
            bedrooms: 2,
            bathrooms: 2,
            squareFeet: 1000,
            rent,
            deposit,
            availableDate,
            "Beautiful 2BR apartment");

        // Assert
        Assert.That(property.Id, Is.Not.EqualTo(Guid.Empty));
        Assert.That(property.OwnerId, Is.EqualTo(ownerId));
        Assert.That(property.Address, Is.EqualTo(address));
        Assert.That(property.PropertyType, Is.EqualTo(PropertyType.Apartment));
        Assert.That(property.Bedrooms, Is.EqualTo(2));
        Assert.That(property.Bathrooms, Is.EqualTo(2));
        Assert.That(property.SquareFeet, Is.EqualTo(1000));
        Assert.That(property.MonthlyRent, Is.EqualTo(rent));
        Assert.That(property.SecurityDeposit, Is.EqualTo(deposit));
        Assert.That(property.AvailableDate.Date, Is.EqualTo(availableDate.Date));
        Assert.That(property.Description, Is.EqualTo("Beautiful 2BR apartment"));
        Assert.That(property.Status, Is.EqualTo(PropertyStatus.Available));
    }

    [Test]
    public void MarkAsRented_Should_Change_Status_To_Rented()
    {
        // Arrange
        var property = CreateTestProperty();

        // Act
        property.MarkAsRented();

        // Assert
        Assert.That(property.Status, Is.EqualTo(PropertyStatus.Rented));
    }

    [Test]
    public void MarkAsAvailable_Should_Change_Status_To_Available()
    {
        // Arrange
        var property = CreateTestProperty();
        property.MarkAsRented();

        // Act
        property.MarkAsAvailable();

        // Assert
        Assert.That(property.Status, Is.EqualTo(PropertyStatus.Available));
    }

    [Test]
    public void MarkAsUnlisted_Should_Change_Status_To_Unlisted()
    {
        // Arrange
        var property = CreateTestProperty();

        // Act
        property.MarkAsUnlisted();

        // Assert
        Assert.That(property.Status, Is.EqualTo(PropertyStatus.Unlisted));
    }

    [Test]
    public void UpdateApplicationFee_Should_Set_Application_Fee()
    {
        // Arrange
        var property = CreateTestProperty();
        var fee = Money.Create(50, "USD");

        // Act
        property.UpdateApplicationFee(fee);

        // Assert
        Assert.That(property.ApplicationFee, Is.EqualTo(fee));
    }

    [Test]
    public void AddAmenity_Should_Add_Amenity_To_List()
    {
        // Arrange
        var property = CreateTestProperty();

        // Act
        property.AddAmenity("Pool");
        property.AddAmenity("Gym");

        // Assert
        Assert.That(property.Amenities, Does.Contain("Pool"));
        Assert.That(property.Amenities, Does.Contain("Gym"));
        Assert.That(property.Amenities.Count, Is.EqualTo(2));
    }

    [Test]
    public void AddImage_Should_Add_Image_To_List()
    {
        // Arrange
        var property = CreateTestProperty();
        var imageUrl = "https://example.com/image1.jpg";

        // Act
        property.AddImage(imageUrl);

        // Assert
        Assert.That(property.Images, Does.Contain(imageUrl));
        Assert.That(property.Images.Count, Is.EqualTo(1));
    }

    private static Property CreateTestProperty()
    {
        var ownerId = Guid.NewGuid();
        var address = PropertyAddress.Create("123 Main St", "Springfield", "IL", "62701", country: "USA");
        var rent = Money.Create(1500, "USD");
        var deposit = Money.Create(1500, "USD");
        var availableDate = DateTime.UtcNow.AddDays(30);

        return new Property(
            ownerId,
            address,
            PropertyType.Apartment,
            bedrooms: 2,
            bathrooms: 2,
            squareFeet: 1000,
            rent,
            deposit,
            availableDate,
            "Beautiful 2BR apartment");
    }
}
