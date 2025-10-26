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
public class ApplicationsControllerTests
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
    public async Task SubmitApplication_Should_Return_Unauthorized_When_Not_Authenticated()
    {
        // Arrange
        var submitDto = new SubmitApplicationDto
        {
            PropertyId = Guid.NewGuid(),
            ApplicationData = new ApplicationDataDto
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "john@example.com",
                Phone = "555-0100",
                DateOfBirth = DateTime.UtcNow.AddYears(-25),
                EmployerName = "Test Corp",
                JobTitle = "Developer",
                AnnualIncome = 75000,
                YearsEmployed = 2,
                TermsAccepted = true,
            },
        };

        var content = new StringContent(
            JsonSerializer.Serialize(submitDto),
            Encoding.UTF8,
            "application/json");

        // Act
        var response = await _client.PostAsync("/api/applications", content);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [Test]
    public async Task GetMyApplications_Should_Return_Unauthorized_When_Not_Authenticated()
    {
        // Act
        var response = await _client.GetAsync("/api/applications/my");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [Test]
    public async Task ApproveApplication_Should_Return_Unauthorized_When_Not_Authenticated()
    {
        // Arrange
        var applicationId = Guid.NewGuid();
        var request = new { DecisionNotes = "Approved" };
        var content = new StringContent(
            JsonSerializer.Serialize(request),
            Encoding.UTF8,
            "application/json");

        // Act
        var response = await _client.PostAsync($"/api/applications/{applicationId}/approve", content);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [Test]
    public async Task RejectApplication_Should_Return_Unauthorized_When_Not_Authenticated()
    {
        // Arrange
        var applicationId = Guid.NewGuid();
        var request = new { DecisionNotes = "Rejected" };
        var content = new StringContent(
            JsonSerializer.Serialize(request),
            Encoding.UTF8,
            "application/json");

        // Act
        var response = await _client.PostAsync($"/api/applications/{applicationId}/reject", content);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }
}

