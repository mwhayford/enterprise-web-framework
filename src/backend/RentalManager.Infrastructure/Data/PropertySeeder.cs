// Copyright (c) RentalManager. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RentalManager.Application.Interfaces;
using RentalManager.Domain.Entities;
using RentalManager.Domain.ValueObjects;

namespace RentalManager.Infrastructure.Data;

public class PropertySeeder
{
    private static readonly string[] Streets =
    {
        "Main Street", "Oak Avenue", "Elm Drive", "Park Road", "Maple Lane",
        "Cedar Boulevard", "Pine Street", "Washington Avenue", "Lincoln Drive",
        "Jefferson Road", "Madison Lane", "First Street", "Second Avenue",
        "Third Street", "Broadway", "Church Street", "Market Street",
        "High Street", "School Street", "Garden Avenue", "Sunset Drive",
        "Hill Road", "Valley Lane", "River Road", "Lake Street",
        "Forest Avenue", "Meadow Lane", "Spring Street", "Summer Drive",
        "Winter Avenue", "Autumn Road", "North Street", "South Avenue",
        "East Drive", "West Road", "Center Street", "Park Avenue"
    };

    private static readonly string[] Cities =
    {
        "New York", "Los Angeles", "Chicago", "Houston", "Phoenix",
        "Philadelphia", "San Antonio", "San Diego", "Dallas", "San Jose",
        "Austin", "Jacksonville", "Fort Worth", "Columbus", "Charlotte",
        "San Francisco", "Indianapolis", "Seattle", "Denver", "Washington",
        "Boston", "Nashville", "Detroit", "Oklahoma City", "Portland",
        "Las Vegas", "Memphis", "Louisville", "Baltimore", "Milwaukee",
        "Atlanta", "Miami", "Tulsa", "Oakland", "Minneapolis",
        "Cleveland", "Wichita", "Arlington", "New Orleans", "Tampa"
    };

    private static readonly string[] States =
    {
        "AL", "AK", "AZ", "AR", "CA", "CO", "CT", "DE", "FL", "GA",
        "HI", "ID", "IL", "IN", "IA", "KS", "KY", "LA", "ME", "MD",
        "MA", "MI", "MN", "MS", "MO", "MT", "NE", "NV", "NH", "NJ",
        "NM", "NY", "NC", "ND", "OH", "OK", "OR", "PA", "RI", "SC",
        "SD", "TN", "TX", "UT", "VT", "VA", "WA", "WV", "WI", "WY"
    };

    private static readonly string[] StreetTypes =
    {
        "St", "Ave", "Rd", "Dr", "Ln", "Blvd", "Ct", "Pl", "Way", "Trl"
    };

    private static readonly string[] Amenities =
    {
        "Parking", "Pet Friendly", "Laundry", "Dishwasher", "Air Conditioning",
        "Heating", "Fireplace", "Balcony", "Patio", "Gym", "Pool", "Elevator",
        "Hardwood Floors", "Carpet", "Central Air", "Storage", "Garage",
        "Walk-in Closet", "High Ceilings", "Granite Countertops", "Stainless Steel Appliances",
        "Washer/Dryer", "Walk to Transit", "Near Schools", "Shopping Nearby"
    };

    private static readonly string[] PropertyDescriptions =
    {
        "Beautiful {0} bedroom {1} in the heart of {2}. Recently renovated with modern amenities.",
        "Spacious {0} bedroom {1} located in {2}. Perfect for families or professionals.",
        "Charming {0} bedroom {1} in {2}. Close to schools, shopping, and transportation.",
        "Modern {0} bedroom {1} in {2}. Features include {3}.",
        "Cozy {0} bedroom {1} in desirable {2} neighborhood. Well-maintained and move-in ready.",
        "Elegant {0} bedroom {1} in {2}. Bright and airy with plenty of natural light.",
        "Stylish {0} bedroom {1} in {2}. Updated kitchen and bathrooms.",
        "Comfortable {0} bedroom {1} in {2}. Perfect location with easy access to everything."
    };

    private readonly IApplicationDbContext _context;
    private readonly ILogger<PropertySeeder> _logger;
    private readonly Random _random;

    public PropertySeeder(IApplicationDbContext context, ILogger<PropertySeeder> logger)
    {
        _context = context;
        _logger = logger;
        _random = new Random();
    }

    public async Task SeedPropertiesAsync(int count, Guid defaultOwnerId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting to seed {Count} properties", count);

        const int batchSize = 100;
        var seeded = 0;

        for (int i = 0; i < count; i += batchSize)
        {
            var batchCount = Math.Min(batchSize, count - i);
            var batch = new List<Property>();

            for (int j = 0; j < batchCount; j++)
            {
                try
                {
                    // Ensure first property is always available for testing
                    var isFirstProperty = (i == 0 && j == 0);
                    var property = GenerateProperty(defaultOwnerId, forceAvailable: isFirstProperty);
                    batch.Add(property);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to generate property {Index}", i + j);
                }
            }

            if (batch.Count > 0)
            {
                _context.Properties.AddRange(batch);
                await _context.SaveChangesAsync(cancellationToken);
                seeded += batch.Count;
                _logger.LogInformation("Seeded {Seeded} of {Total} properties", seeded, count);
            }
        }

        _logger.LogInformation("Completed seeding {Count} properties", seeded);
    }

    /// <summary>
    /// Seeds properties distributed across all major US metro areas.
    /// Generates 1,000 properties per metro area, covering all 50 states.
    /// </summary>
    /// <param name="propertiesPerMetro">Number of properties to generate per metro area (default: 1,000).</param>
    /// <param name="defaultOwnerId">Owner ID to assign to all properties.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Total number of properties seeded.</returns>
    public async Task<int> SeedPropertiesByMetroAreaAsync(int propertiesPerMetro = 1000, Guid defaultOwnerId = default, CancellationToken cancellationToken = default)
    {
        var metroAreas = MetroAreaData.GetAllMetroAreas();
        var totalProperties = metroAreas.Count * propertiesPerMetro;

        _logger.LogInformation("Starting to seed {PropertiesPerMetro} properties across {MetroCount} metro areas (Total: {TotalProperties})", 
            propertiesPerMetro, metroAreas.Count, totalProperties);

        const int batchSize = 100;
        var totalSeeded = 0;
        var metroIndex = 0;

        foreach (var metro in metroAreas)
        {
            metroIndex++;
            _logger.LogInformation("Seeding metro area {Index}/{Total}: {MetroName}, {City}, {State} ({PropertiesPerMetro} properties)", 
                metroIndex, metroAreas.Count, metro.Name, metro.City, metro.State, propertiesPerMetro);

            var metroSeeded = 0;

            for (int i = 0; i < propertiesPerMetro; i += batchSize)
            {
                var batchCount = Math.Min(batchSize, propertiesPerMetro - i);
                var batch = new List<Property>();

                for (int j = 0; j < batchCount; j++)
                {
                    try
                    {
                        // Ensure first property across all metros is always available for testing
                        var isFirstProperty = (metroIndex == 1 && i == 0 && j == 0);
                        var property = GeneratePropertyForMetro(defaultOwnerId, metro, forceAvailable: isFirstProperty);
                        batch.Add(property);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to generate property {Index} for metro {MetroName}", i + j, metro.Name);
                    }
                }

                if (batch.Count > 0)
                {
                    _context.Properties.AddRange(batch);
                    await _context.SaveChangesAsync(cancellationToken);
                    metroSeeded += batch.Count;
                    totalSeeded += batch.Count;

                    if ((i + batchCount) % 500 == 0 || (i + batchCount) == propertiesPerMetro)
                    {
                        _logger.LogInformation("  Seeded {MetroSeeded}/{PropertiesPerMetro} properties for {MetroName} ({TotalSeeded}/{TotalProperties} total)", 
                            metroSeeded, propertiesPerMetro, metro.Name, totalSeeded, totalProperties);
                    }
                }
            }

            _logger.LogInformation("Completed seeding {MetroSeeded} properties for {MetroName}, {State}", metroSeeded, metro.Name, metro.State);
        }

        _logger.LogInformation("Completed seeding {TotalSeeded} properties across all metro areas", totalSeeded);
        return totalSeeded;
    }

    private Property GenerateProperty(Guid ownerId, bool forceAvailable = false)
    {
        var streetNumber = _random.Next(1, 9999);
        var streetName = Streets[_random.Next(Streets.Length)];
        var streetType = StreetTypes[_random.Next(StreetTypes.Length)];
        var street = $"{streetNumber} {streetName} {streetType}";

        var city = Cities[_random.Next(Cities.Length)];
        var state = States[_random.Next(States.Length)];
        var zipCode = GenerateZipCode();
        
        return GeneratePropertyWithLocation(ownerId, street, city, state, zipCode, null, null, forceAvailable);
    }

    private Property GeneratePropertyForMetro(Guid ownerId, MetroArea metro, bool forceAvailable = false)
    {
        var streetNumber = _random.Next(1, 9999);
        var streetName = Streets[_random.Next(Streets.Length)];
        var streetType = StreetTypes[_random.Next(StreetTypes.Length)];
        var street = $"{streetNumber} {streetName} {streetType}";

        // Generate zip code based on metro area prefix
        var zipCode = GenerateZipCodeForMetro(metro.ZipCodePrefix);

        // Determine unit number for apartments/condos
        string? unit = null;
        var propertyType = (PropertyType)_random.Next(1, 8); // Exclude "Other"
        if (propertyType == PropertyType.Apartment || propertyType == PropertyType.Condo || propertyType == PropertyType.Studio)
        {
            if (_random.Next(100) < 60) // 60% chance of having a unit
            {
                unit = $"#{_random.Next(1, 200)}";
            }
        }

        return GeneratePropertyWithLocation(ownerId, street, metro.City, metro.State, zipCode, unit, metro.RentMultiplier, forceAvailable);
    }

    private Property GeneratePropertyWithLocation(Guid ownerId, string street, string city, string state, string zipCode, string? unit, decimal? rentMultiplier = null, bool forceAvailable = false)
    {
        var address = PropertyAddress.Create(street, city, state, zipCode, unit);

        // Determine property type
        var propertyType = (PropertyType)_random.Next(1, 8); // Exclude "Other"

        // Bedrooms: weighted toward 1-3 bedrooms
        var bedrooms = _random.Next(100) switch
        {
            < 10 => 0,  // 10% Studio
            < 30 => 1,  // 20% 1BR
            < 60 => 2,  // 30% 2BR
            < 85 => 3,  // 25% 3BR
            < 95 => 4,  // 10% 4BR
            _ => 5 // 5% 5BR
        };

        // Bathrooms: typically bedrooms - 0.5 to bedrooms + 1
        var bathroomBase = Math.Max(1, bedrooms);
        var bathrooms = _random.Next(3) switch
        {
            0 => bathroomBase - (bedrooms > 0 ? 1 : 0), // Can have 0.5 baths for studios
            1 => bathroomBase,
            _ => bathroomBase + 1
        };

        // Convert to decimal for half baths
        decimal bathroomsDecimal = bathrooms + (_random.Next(2) == 0 ? 0.5m : 0m);
        var bathroomsInt = (int)Math.Ceiling(bathroomsDecimal); // Store as int, but can represent 1.5 as 2 for simplicity

        // Square feet based on bedrooms (rough estimates)
        var squareFeet = bedrooms switch
        {
            0 => _random.Next(400, 700),      // Studio: 400-700 sqft
            1 => _random.Next(600, 1000),    // 1BR: 600-1000 sqft
            2 => _random.Next(900, 1400),     // 2BR: 900-1400 sqft
            3 => _random.Next(1200, 2000),    // 3BR: 1200-2000 sqft
            4 => _random.Next(1800, 2800),    // 4BR: 1800-2800 sqft
            _ => _random.Next(2200, 3500) // 5BR+: 2200-3500 sqft
        };

        // Rent based on location and size (rough market rates)
        var baseRent = squareFeet * (decimal)((_random.NextDouble() * 1.5) + 0.8); // $0.80-$2.30 per sqft

        // Use provided rent multiplier, or calculate from city if not provided
        decimal locationMultiplier;
        if (rentMultiplier.HasValue)
        {
            locationMultiplier = rentMultiplier.Value;
        }
        else
        {
            // Fallback to city-based multiplier for backward compatibility
            locationMultiplier = city switch
            {
                "New York" => 2.5m,
                "San Francisco" => 2.3m,
                "Los Angeles" => 2.0m,
                "Boston" => 1.8m,
                "Seattle" => 1.7m,
                "Chicago" => 1.4m,
                "Denver" => 1.3m,
                _ => 1.0m
            };
        }
        
        var monthlyRent = Math.Round(baseRent * locationMultiplier, 2);

        // Security deposit: 1-2x monthly rent
        var securityDepositMultiplier = (decimal)(_random.NextDouble() + 1.0); // 1.0-2.0
        var securityDeposit = Math.Round(monthlyRent * securityDepositMultiplier, 2);

        var monthlyRentMoney = Money.Create(monthlyRent, "USD");
        var securityDepositMoney = Money.Create(securityDeposit, "USD");

        // Available date: mix of past, present, and future dates
        var daysOffset = _random.Next(-30, 90);
        var availableDate = DateTime.UtcNow.AddDays(daysOffset);

        // Generate description
        var selectedAmenities = GetRandomAmenities();
        var amenitiesText = selectedAmenities.Count > 0
            ? string.Join(", ", selectedAmenities.Take(3))
            : "modern amenities";
        var propertyTypeText = propertyType.ToString().ToLower();
        var descriptionTemplate = PropertyDescriptions[_random.Next(PropertyDescriptions.Length)];
        var description = string.Format(descriptionTemplate, bedrooms, propertyTypeText, city, amenitiesText);

        var property = new Property(
            ownerId,
            address,
            propertyType,
            bedrooms,
            bathroomsInt,
            squareFeet,
            monthlyRentMoney,
            securityDepositMoney,
            availableDate,
            description);

        // Add amenities
        foreach (var amenity in selectedAmenities)
        {
            property.AddAmenity(amenity);
        }

        // Application fee: 70% have one, between $25-$50
        if (_random.Next(100) < 70)
        {
            var appFee = (decimal)_random.Next(25, 51);
            property.UpdateApplicationFee(Money.Create(appFee, "USD"));
        }

        // Property status: 60% Available, 30% Rented, 10% Maintenance
        // If forceAvailable is true, always mark as available (for first property in seed)
        if (forceAvailable)
        {
            property.MarkAsAvailable();
        }
        else
        {
            var statusRoll = _random.Next(100);
            if (statusRoll < 60)
            {
                property.MarkAsAvailable();
            }
            else if (statusRoll < 90)
            {
                property.MarkAsRented();
            }
            else
            {
                property.MarkAsInMaintenance();
            }
        }

        return property;
    }

    private string GenerateZipCode()
    {
        return $"{_random.Next(10000, 99999)}";
    }

    private string GenerateZipCodeForMetro(int zipCodePrefix)
    {
        // Generate a zip code using the metro area's prefix
        // Format: prefix + random 2 digits (e.g., 90210-90299 for Beverly Hills area)
        var suffix = _random.Next(10, 99);
        return $"{zipCodePrefix + suffix}";
    }

    private List<string> GetRandomAmenities()
    {
        var count = _random.Next(3, 8); // 3-7 amenities
        var selected = new List<string>();
        var available = Amenities.ToList();

        for (int i = 0; i < count && available.Count > 0; i++)
        {
            var index = _random.Next(available.Count);
            selected.Add(available[index]);
            available.RemoveAt(index);
        }

        return selected;
    }
}
