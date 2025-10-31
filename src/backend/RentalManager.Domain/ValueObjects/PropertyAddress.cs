// Copyright (c) RentalManager. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
namespace RentalManager.Domain.ValueObjects;

public record PropertyAddress
{
    private PropertyAddress(
        string street,
        string city,
        string state,
        string zipCode,
        string? unit = null,
        string? country = null)
    {
        Street = street;
        City = city;
        State = state;
        ZipCode = zipCode;
        Unit = unit;
        Country = country ?? "USA";
    }

    public string Street { get; init; }

    public string? Unit { get; init; }

    public string City { get; init; }

    public string State { get; init; }

    public string ZipCode { get; init; }

    public string Country { get; init; }

    public string FullAddress => Unit != null
        ? $"{Street}, {Unit}, {City}, {State} {ZipCode}, {Country}"
        : $"{Street}, {City}, {State} {ZipCode}, {Country}";

    public static PropertyAddress Create(
        string street,
        string city,
        string state,
        string zipCode,
        string? unit = null,
        string? country = null)
    {
        if (string.IsNullOrWhiteSpace(street))
        {
            throw new ArgumentException("Street cannot be empty", nameof(street));
        }

        if (string.IsNullOrWhiteSpace(city))
        {
            throw new ArgumentException("City cannot be empty", nameof(city));
        }

        if (string.IsNullOrWhiteSpace(state))
        {
            throw new ArgumentException("State cannot be empty", nameof(state));
        }

        if (string.IsNullOrWhiteSpace(zipCode))
        {
            throw new ArgumentException("Zip code cannot be empty", nameof(zipCode));
        }

        return new PropertyAddress(street, city, state, zipCode, unit, country);
    }
}

