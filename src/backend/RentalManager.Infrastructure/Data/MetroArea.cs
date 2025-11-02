// Copyright (c) RentalManager. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace RentalManager.Infrastructure.Data;

/// <summary>
/// Represents a major metropolitan area in the United States.
/// </summary>
public class MetroArea
{
    public string Name { get; set; } = string.Empty;

    public string City { get; set; } = string.Empty;

    public string State { get; set; } = string.Empty;

    public string StateCode { get; set; } = string.Empty;

    /// <summary>
    /// Rent multiplier for this metro area (1.0 = average US market rate).
    /// Higher values indicate more expensive markets.
    /// </summary>
    public decimal RentMultiplier { get; set; } = 1.0m;

    /// <summary>
    /// Zip code prefix for generating realistic zip codes in this area.
    /// </summary>
    public int ZipCodePrefix { get; set; }

    public MetroArea(string name, string city, string state, string stateCode, decimal rentMultiplier, int zipCodePrefix)
    {
        Name = name;
        City = city;
        State = state;
        StateCode = stateCode;
        RentMultiplier = rentMultiplier;
        ZipCodePrefix = zipCodePrefix;
    }
}

