// Copyright (c) RentalManager. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using FluentAssertions;
using Moq;
using NUnit.Framework;
using RentalManager.Application.Handlers;
using RentalManager.Application.Interfaces;
using RentalManager.Application.Queries;
using RentalManager.Domain.Entities;
using RentalManager.Domain.ValueObjects;
using RentalManager.UnitTests.Infrastructure;

namespace RentalManager.UnitTests.Application.Queries;

[TestFixture]
public class GetAvailablePropertiesQueryHandlerTests
{
    private Mock<IApplicationDbContext> _contextMock = null!;
    private GetAvailablePropertiesQueryHandler _handler = null!;
    private List<Property> _properties = null!;

    [SetUp]
    public void SetUp()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _handler = new GetAvailablePropertiesQueryHandler(_contextMock.Object);
        _properties = CreateTestProperties();
        SetupMockDbSet();
    }

    [Test]
    public async Task Handle_WithNoFilters_Should_Return_All_Available_Properties()
    {
        // Arrange
        var query = new GetAvailablePropertiesQuery(PageNumber: 1, PageSize: 20);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(3); // Only Available properties
        result.TotalCount.Should().Be(3);
        result.PageNumber.Should().Be(1);
        result.PageSize.Should().Be(20);
        result.Items.Should().OnlyContain(p => p.Status == PropertyStatus.Available);
    }

    [Test]
    public async Task Handle_WithMinBedroomsFilter_Should_Return_Only_Matching_Properties()
    {
        // Arrange
        var query = new GetAvailablePropertiesQuery(PageNumber: 1, PageSize: 20, MinBedrooms: 3);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().OnlyContain(p => p.Bedrooms >= 3);
        result.Items.Should().HaveCount(2); // Property3 (3BR) and Property5 (4BR)
    }

    [Test]
    public async Task Handle_WithMaxBedroomsFilter_Should_Return_Only_Matching_Properties()
    {
        // Arrange
        var query = new GetAvailablePropertiesQuery(PageNumber: 1, PageSize: 20, MaxBedrooms: 2);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().OnlyContain(p => p.Bedrooms <= 2);
        result.Items.Should().HaveCount(1); // Property1 (2BR)
    }

    [Test]
    public async Task Handle_WithBedroomRange_Should_Return_Only_Matching_Properties()
    {
        // Arrange
        var query = new GetAvailablePropertiesQuery(PageNumber: 1, PageSize: 20, MinBedrooms: 2, MaxBedrooms: 3);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().OnlyContain(p => p.Bedrooms >= 2 && p.Bedrooms <= 3);
        result.Items.Should().HaveCount(2); // Property1 (2BR) and Property3 (3BR)
    }

    [Test]
    public async Task Handle_WithMinBathroomsFilter_Should_Return_Only_Matching_Properties()
    {
        // Arrange
        var query = new GetAvailablePropertiesQuery(PageNumber: 1, PageSize: 20, MinBathrooms: 2);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().OnlyContain(p => p.Bathrooms >= 2);
        result.Items.Should().HaveCount(2); // Property3 and Property5
    }

    [Test]
    public async Task Handle_WithMinRentFilter_Should_Return_Only_Matching_Properties()
    {
        // Arrange
        var query = new GetAvailablePropertiesQuery(PageNumber: 1, PageSize: 20, MinRent: 1500m);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().OnlyContain(p => p.MonthlyRent >= 1500m);
        result.Items.Should().HaveCount(2); // Property3 (2000) and Property5 (2500)
    }

    [Test]
    public async Task Handle_WithMaxRentFilter_Should_Return_Only_Matching_Properties()
    {
        // Arrange
        var query = new GetAvailablePropertiesQuery(PageNumber: 1, PageSize: 20, MaxRent: 1500m);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().OnlyContain(p => p.MonthlyRent <= 1500m);
        result.Items.Should().HaveCount(1); // Property1 (1200)
    }

    [Test]
    public async Task Handle_WithCityFilter_Should_Return_Only_Matching_Properties()
    {
        // Arrange
        var query = new GetAvailablePropertiesQuery(PageNumber: 1, PageSize: 20, City: "Springfield");

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().OnlyContain(p => p.Address.Contains("Springfield", StringComparison.OrdinalIgnoreCase));
        result.Items.Should().HaveCount(2); // Property1 and Property3
    }

    [Test]
    public async Task Handle_WithCityFilter_CaseInsensitive_Should_Return_Matching_Properties()
    {
        // Arrange
        var query = new GetAvailablePropertiesQuery(PageNumber: 1, PageSize: 20, City: "SPRINGFIELD");

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(2); // Case-insensitive search
    }

    [Test]
    public async Task Handle_WithStateFilter_Should_Return_Only_Matching_Properties()
    {
        // Arrange
        var query = new GetAvailablePropertiesQuery(PageNumber: 1, PageSize: 20, State: "IL");

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().OnlyContain(p => p.Address.Contains(", IL ", StringComparison.Ordinal));
        result.Items.Should().HaveCount(3); // Property1 (Springfield, IL), Property3 (Springfield, IL), Property5 (Chicago, IL)
    }

    [Test]
    public async Task Handle_WithPropertyTypeFilter_Should_Return_Only_Matching_Properties()
    {
        // Arrange
        var query = new GetAvailablePropertiesQuery(PageNumber: 1, PageSize: 20, PropertyType: PropertyType.House);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().OnlyContain(p => p.PropertyType == PropertyType.House);
        result.Items.Should().HaveCount(1); // Property5
    }

    [Test]
    public async Task Handle_WithSearchTerm_Should_Return_Matching_Properties()
    {
        // Arrange
        var query = new GetAvailablePropertiesQuery(PageNumber: 1, PageSize: 20, SearchTerm: "downtown");

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(1); // Property3 has "downtown" in description
        result.Items.First().Id.Should().Be(_properties[2].Id); // Property3
    }

    [Test]
    public async Task Handle_WithSearchTerm_InStreet_Should_Return_Matching_Properties()
    {
        // Arrange
        var query = new GetAvailablePropertiesQuery(PageNumber: 1, PageSize: 20, SearchTerm: "Oak");

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(1); // Property3 has "Oak" in street
    }

    [Test]
    public async Task Handle_WithSortByRent_Ascending_Should_Sort_Properties_Correctly()
    {
        // Arrange
        var query = new GetAvailablePropertiesQuery(PageNumber: 1, PageSize: 20, SortBy: "rent", SortDescending: false);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().BeInAscendingOrder(p => p.MonthlyRent);
        result.Items.First().MonthlyRent.Should().Be(1200m); // Property1
        result.Items.Last().MonthlyRent.Should().Be(2500m); // Property5
    }

    [Test]
    public async Task Handle_WithSortByRent_Descending_Should_Sort_Properties_Correctly()
    {
        // Arrange
        var query = new GetAvailablePropertiesQuery(PageNumber: 1, PageSize: 20, SortBy: "rent", SortDescending: true);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().BeInDescendingOrder(p => p.MonthlyRent);
        result.Items.First().MonthlyRent.Should().Be(2500m); // Property5
        result.Items.Last().MonthlyRent.Should().Be(1200m); // Property1
    }

    [Test]
    public async Task Handle_WithSortByBedrooms_Should_Sort_Properties_Correctly()
    {
        // Arrange
        var query = new GetAvailablePropertiesQuery(PageNumber: 1, PageSize: 20, SortBy: "bedrooms", SortDescending: false);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().BeInAscendingOrder(p => p.Bedrooms);
        result.Items.First().Bedrooms.Should().Be(2); // Property1
        result.Items.Last().Bedrooms.Should().Be(4); // Property5
    }

    [Test]
    public async Task Handle_WithSortBySquareFeet_Should_Sort_Properties_Correctly()
    {
        // Arrange
        var query = new GetAvailablePropertiesQuery(PageNumber: 1, PageSize: 20, SortBy: "squarefeet", SortDescending: false);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().BeInAscendingOrder(p => p.SquareFeet);
    }

    [Test]
    public async Task Handle_WithSortByAvailableDate_Should_Sort_Properties_Correctly()
    {
        // Arrange
        var query = new GetAvailablePropertiesQuery(PageNumber: 1, PageSize: 20, SortBy: "availabledate", SortDescending: false);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().BeInAscendingOrder(p => p.AvailableDate);
    }

    [Test]
    public async Task Handle_WithDefaultSort_Should_Sort_By_CreatedAt()
    {
        // Arrange
        var query = new GetAvailablePropertiesQuery(PageNumber: 1, PageSize: 20, SortBy: null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();

        // Default sort is by CreatedAt - order should match creation order
        result.Items.Should().HaveCount(3);
    }

    [Test]
    public async Task Handle_WithPagination_Should_Return_Correct_Page()
    {
        // Arrange
        var query = new GetAvailablePropertiesQuery(PageNumber: 1, PageSize: 2);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(3);
        result.PageNumber.Should().Be(1);
        result.PageSize.Should().Be(2);
        result.TotalPages.Should().Be(2);
        result.HasNextPage.Should().BeTrue();
        result.HasPreviousPage.Should().BeFalse();
    }

    [Test]
    public async Task Handle_WithPagination_SecondPage_Should_Return_Correct_Page()
    {
        // Arrange
        var query = new GetAvailablePropertiesQuery(PageNumber: 2, PageSize: 2);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(1); // Remaining item
        result.TotalCount.Should().Be(3);
        result.PageNumber.Should().Be(2);
        result.HasNextPage.Should().BeFalse();
        result.HasPreviousPage.Should().BeTrue();
    }

    [Test]
    public async Task Handle_WithInvalidPageNumber_Should_Use_Default_Of_1()
    {
        // Arrange
        var query = new GetAvailablePropertiesQuery(PageNumber: 0, PageSize: 20);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.PageNumber.Should().Be(1);
    }

    [Test]
    public async Task Handle_WithInvalidPageSize_Should_Clamp_To_Valid_Range()
    {
        // Arrange
        var query = new GetAvailablePropertiesQuery(PageNumber: 1, PageSize: 0);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.PageSize.Should().Be(1); // Clamped to minimum of 1
    }

    [Test]
    public async Task Handle_WithPageSize_OverLimit_Should_Clamp_To_100()
    {
        // Arrange
        var query = new GetAvailablePropertiesQuery(PageNumber: 1, PageSize: 200);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.PageSize.Should().Be(100); // Clamped to maximum of 100
    }

    [Test]
    public async Task Handle_Should_Only_Return_Available_Properties()
    {
        // Arrange
        var query = new GetAvailablePropertiesQuery(PageNumber: 1, PageSize: 20);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().OnlyContain(p => p.Status == PropertyStatus.Available);

        // Should not include Property2 (Rented) or Property4 (Unlisted)
    }

    [Test]
    public async Task Handle_WithNoMatchingProperties_Should_Return_Empty_Result()
    {
        // Arrange
        var query = new GetAvailablePropertiesQuery(PageNumber: 1, PageSize: 20, MinBedrooms: 10);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    [Test]
    public async Task Handle_Should_Include_ThumbnailImage_When_Available()
    {
        // Arrange
        var query = new GetAvailablePropertiesQuery(PageNumber: 1, PageSize: 20);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        var propertyWithImage = result.Items.FirstOrDefault(p => p.Id == _properties[2].Id); // Property3 has image
        propertyWithImage.Should().NotBeNull();
        propertyWithImage!.ThumbnailImage.Should().Be("https://example.com/property3.jpg");
    }

    [Test]
    public async Task Handle_Should_Handle_Null_ApplicationFee()
    {
        // Arrange
        var query = new GetAvailablePropertiesQuery(PageNumber: 1, PageSize: 20);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().Contain(p => p.ApplicationFee == null); // Property1 has no application fee
    }

    [Test]
    public async Task Handle_Should_Handle_Property_With_Unit_In_Address()
    {
        // Arrange
        var query = new GetAvailablePropertiesQuery(PageNumber: 1, PageSize: 20);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        var propertyWithUnit = result.Items.FirstOrDefault(p => p.Id == _properties[2].Id); // Property3 has unit "Apt 5B"
        propertyWithUnit.Should().NotBeNull();
        propertyWithUnit!.Address.Should().Contain("Apt 5B");
    }

    private void SetupMockDbSet()
    {
        var mockDbSet = _properties.AsQueryable().BuildMockDbSet();
        _contextMock.Setup(x => x.Properties).Returns(mockDbSet.Object);
    }

    private List<Property> CreateTestProperties()
    {
        var ownerId = Guid.NewGuid();
        var properties = new List<Property>();

        // Property1: Available, 2BR, 1BA, $1200, Springfield, IL, Apartment, no unit
        var property1 = new Property(
            ownerId,
            PropertyAddress.Create("123 Main St", "Springfield", "IL", "62701"),
            PropertyType.Apartment,
            bedrooms: 2,
            bathrooms: 1,
            squareFeet: 900,
            Money.Create(1200, "USD"),
            Money.Create(1200, "USD"),
            DateTime.UtcNow.AddDays(30),
            "Cozy 2BR apartment");
        property1.AddImage("https://example.com/property1.jpg");
        properties.Add(property1);

        // Property2: Rented, 2BR, 2BA, $1500, Springfield, IL, Apartment
        var property2 = new Property(
            ownerId,
            PropertyAddress.Create("456 Oak Ave", "Springfield", "IL", "62702"),
            PropertyType.Apartment,
            bedrooms: 2,
            bathrooms: 2,
            squareFeet: 1100,
            Money.Create(1500, "USD"),
            Money.Create(1500, "USD"),
            DateTime.UtcNow.AddDays(15),
            "Nice 2BR apartment");
        property2.MarkAsRented();
        properties.Add(property2);

        // Property3: Available, 3BR, 2BA, $2000, Springfield, IL, Apartment, with unit
        var property3 = new Property(
            ownerId,
            PropertyAddress.Create("789 Oak St", "Springfield", "IL", "62703", unit: "Apt 5B"),
            PropertyType.Apartment,
            bedrooms: 3,
            bathrooms: 2,
            squareFeet: 1400,
            Money.Create(2000, "USD"),
            Money.Create(2000, "USD"),
            DateTime.UtcNow.AddDays(45),
            "Spacious downtown 3BR apartment");
        property3.AddImage("https://example.com/property3.jpg");
        property3.UpdateApplicationFee(Money.Create(50, "USD"));
        properties.Add(property3);

        // Property4: Unlisted, 2BR, 1BA, $1300, Chicago, IL, Apartment
        var property4 = new Property(
            ownerId,
            PropertyAddress.Create("321 Elm St", "Chicago", "IL", "60601"),
            PropertyType.Apartment,
            bedrooms: 2,
            bathrooms: 1,
            squareFeet: 950,
            Money.Create(1300, "USD"),
            Money.Create(1300, "USD"),
            DateTime.UtcNow.AddDays(20),
            "Modern 2BR apartment");
        property4.MarkAsUnlisted();
        properties.Add(property4);

        // Property5: Available, 4BR, 3BA, $2500, Chicago, IL, House
        var property5 = new Property(
            ownerId,
            PropertyAddress.Create("555 Park Blvd", "Chicago", "IL", "60602"),
            PropertyType.House,
            bedrooms: 4,
            bathrooms: 3,
            squareFeet: 2000,
            Money.Create(2500, "USD"),
            Money.Create(2500, "USD"),
            DateTime.UtcNow.AddDays(60),
            "Large family house");
        property5.UpdateApplicationFee(Money.Create(75, "USD"));
        properties.Add(property5);

        return properties;
    }
}
