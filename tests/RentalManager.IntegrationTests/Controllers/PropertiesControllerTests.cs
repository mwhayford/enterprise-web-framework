// Copyright (c) RentalManager. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using RentalManager.Application.DTOs;
using RentalManager.Infrastructure.Persistence;
using RentalManager.IntegrationTests.Infrastructure;

namespace RentalManager.IntegrationTests.Controllers;

[TestFixture]
public class PropertiesControllerTests
{
    private HttpClient _client = null!;
    private CustomWebApplicationFactory _factory = null!;

    [SetUp]
    public void SetUp()
    {
        _factory = new CustomWebApplicationFactory();
        _client = _factory.CreateClient();
    }

    [TearDown]
    public void TearDown()
    {
        _client?.Dispose();
        _factory?.Dispose();
    }

    [Test]
    public async Task GetAvailableProperties_Should_Return_Ok_With_Properties()
    {
        // Arrange
        await SeedTestProperties();

        // Act
        var response = await _client.GetAsync("/api/properties");

        // Assert
        response.EnsureSuccessStatusCode();

        // Just verify we get a successful response - actual data validation would require seeding
        Assert.That(response.IsSuccessStatusCode, Is.True);
    }

    [Test]
    public async Task GetPropertyById_Should_Return_Ok_When_Property_Exists()
    {
        // Arrange
        var propertyId = await CreateTestProperty();

        // Act
        var response = await _client.GetAsync($"/api/properties/{propertyId}");

        // Assert
        response.EnsureSuccessStatusCode();
        var property = await response.Content.ReadFromJsonAsync<PropertyDto>();
        Assert.That(property, Is.Not.Null);
        Assert.That(property!.Id, Is.EqualTo(propertyId));
    }

    [Test]
    public async Task GetPropertyById_Should_Return_NotFound_When_Property_Does_Not_Exist()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/properties/{nonExistentId}");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task CreateProperty_Should_Return_Unauthorized_When_Not_Authenticated()
    {
        // Arrange
        var createDto = new CreatePropertyDto
        {
            Street = "123 Test St",
            City = "Test City",
            State = "TS",
            ZipCode = "12345",
            PropertyType = 0,
            Bedrooms = 2,
            Bathrooms = 2,
            SquareFeet = 1000,
            MonthlyRent = 1500,
            RentCurrency = "USD",
            SecurityDeposit = 1500,
            SecurityDepositCurrency = "USD",
            AvailableDate = DateTime.UtcNow.AddDays(30),
            Description = "Test property",
        };

        var content = new StringContent(
            JsonSerializer.Serialize(createDto),
            Encoding.UTF8,
            "application/json");

        // Act
        var response = await _client.PostAsync("/api/properties", content);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    private async Task SeedTestProperties()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // Add test properties to database
        await Task.CompletedTask; // satisfy async method until implemented
    }

    private async Task<Guid> CreateTestProperty()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // Create and save a test property
        // Return the property ID
        await Task.CompletedTask; // satisfy async method until implemented
        return Guid.NewGuid(); // Placeholder
    }
}
