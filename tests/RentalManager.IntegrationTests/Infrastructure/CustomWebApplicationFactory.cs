// Copyright (c) RentalManager. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace RentalManager.IntegrationTests.Infrastructure;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((context, config) =>
        {
            // Add in-memory configuration (will override appsettings.json values)
            // Configuration sources are evaluated in reverse order, so adding last means it's checked first
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = "Host=localhost;Port=5433;Database=RentalManagerDb;Username=postgres;Password=password",
                // Disable Kafka by setting empty bootstrap servers (prevents connection attempts)
                // Program.cs will automatically register NullEventBus instead of KafkaEventBus
                ["Kafka:BootstrapServers"] = string.Empty,
                ["Kafka:GroupId"] = string.Empty,
                ["Kafka:SecurityProtocol"] = string.Empty,
                // Add JWT configuration for authentication tests
                ["Jwt:Key"] = "TestJwtSecretKeyForIntegrationTestsAtLeast32CharsLong",
                ["Jwt:Issuer"] = "RentalManager.API",
                ["Jwt:Audience"] = "RentalManager.API.Users",
            });
        });

        builder.UseEnvironment("Testing");
    }
}
