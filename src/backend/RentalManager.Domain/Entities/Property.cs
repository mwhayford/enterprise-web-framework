// Copyright (c) RentalManager. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
using RentalManager.Domain.Events;
using RentalManager.Domain.ValueObjects;

namespace RentalManager.Domain.Entities;

public class Property : BaseEntity
{
    private readonly List<string> _amenities = new();
    private readonly List<string> _images = new();

    public Property(
        Guid ownerId,
        PropertyAddress address,
        PropertyType propertyType,
        int bedrooms,
        int bathrooms,
        decimal squareFeet,
        Money monthlyRent,
        Money securityDeposit,
        DateTime availableDate,
        string description)
        : base()
    {
        if (ownerId == Guid.Empty)
        {
            throw new ArgumentException("Owner ID cannot be empty", nameof(ownerId));
        }

        ArgumentNullException.ThrowIfNull(address);
        ArgumentNullException.ThrowIfNull(monthlyRent);
        ArgumentNullException.ThrowIfNull(securityDeposit);

        if (bedrooms < 0)
        {
            throw new ArgumentException("Bedrooms cannot be negative", nameof(bedrooms));
        }

        if (bathrooms < 0)
        {
            throw new ArgumentException("Bathrooms cannot be negative", nameof(bathrooms));
        }

        if (squareFeet <= 0)
        {
            throw new ArgumentException("Square feet must be positive", nameof(squareFeet));
        }

        if (string.IsNullOrWhiteSpace(description))
        {
            throw new ArgumentException("Description cannot be empty", nameof(description));
        }

        OwnerId = ownerId;
        Address = address;
        PropertyType = propertyType;
        Bedrooms = bedrooms;
        Bathrooms = bathrooms;
        SquareFeet = squareFeet;
        MonthlyRent = monthlyRent;
        SecurityDeposit = securityDeposit;
        AvailableDate = availableDate;
        Description = description;
        Status = PropertyStatus.Available;

        AddDomainEvent(new PropertyListedEvent
        {
            PropertyId = Id,
            OwnerId = ownerId,
            Address = address.FullAddress,
            MonthlyRent = monthlyRent.Amount,
            AvailableDate = availableDate
        });
    }

    private Property()
    {
        Address = default!;
        MonthlyRent = default!;
        SecurityDeposit = default!;
        Description = default!;
    } // For EF Core

    public Guid OwnerId { get; private set; }

    public PropertyAddress Address { get; private set; }

    public PropertyType PropertyType { get; private set; }

    public int Bedrooms { get; private set; }

    public int Bathrooms { get; private set; }

    public decimal SquareFeet { get; private set; }

    public Money MonthlyRent { get; private set; }

    public Money SecurityDeposit { get; private set; }

    public DateTime AvailableDate { get; private set; }

    public PropertyStatus Status { get; private set; }

    public string Description { get; private set; }

    public Money? ApplicationFee { get; private set; }

    public IReadOnlyCollection<string> Amenities => _amenities.AsReadOnly();

    public IReadOnlyCollection<string> Images => _images.AsReadOnly();

    public void MarkAsAvailable()
    {
        Status = PropertyStatus.Available;
        UpdateTimestamp();
    }

    public void MarkAsRented()
    {
        Status = PropertyStatus.Rented;
        UpdateTimestamp();
    }

    public void MarkAsUnlisted()
    {
        Status = PropertyStatus.Unlisted;
        UpdateTimestamp();
    }

    public void MarkAsInMaintenance()
    {
        Status = PropertyStatus.Maintenance;
        UpdateTimestamp();
    }

    public void UpdateApplicationFee(Money? applicationFee)
    {
        ApplicationFee = applicationFee;
        UpdateTimestamp();
    }

    public void UpdateDetails(
        int bedrooms,
        int bathrooms,
        decimal squareFeet,
        Money monthlyRent,
        Money securityDeposit,
        DateTime availableDate,
        string description)
    {
        if (bedrooms < 0)
        {
            throw new ArgumentException("Bedrooms cannot be negative", nameof(bedrooms));
        }

        if (bathrooms < 0)
        {
            throw new ArgumentException("Bathrooms cannot be negative", nameof(bathrooms));
        }

        if (squareFeet <= 0)
        {
            throw new ArgumentException("Square feet must be positive", nameof(squareFeet));
        }

        if (string.IsNullOrWhiteSpace(description))
        {
            throw new ArgumentException("Description cannot be empty", nameof(description));
        }

        Bedrooms = bedrooms;
        Bathrooms = bathrooms;
        SquareFeet = squareFeet;
        MonthlyRent = monthlyRent;
        SecurityDeposit = securityDeposit;
        AvailableDate = availableDate;
        Description = description;
        UpdateTimestamp();
    }

    public void AddAmenity(string amenity)
    {
        if (string.IsNullOrWhiteSpace(amenity))
        {
            throw new ArgumentException("Amenity cannot be empty", nameof(amenity));
        }

        if (!_amenities.Contains(amenity))
        {
            _amenities.Add(amenity);
            UpdateTimestamp();
        }
    }

    public void RemoveAmenity(string amenity)
    {
        _amenities.Remove(amenity);
        UpdateTimestamp();
    }

    public void AddImage(string imageUrl)
    {
        if (string.IsNullOrWhiteSpace(imageUrl))
        {
            throw new ArgumentException("Image URL cannot be empty", nameof(imageUrl));
        }

        if (!_images.Contains(imageUrl))
        {
            _images.Add(imageUrl);
            UpdateTimestamp();
        }
    }

    public void RemoveImage(string imageUrl)
    {
        _images.Remove(imageUrl);
        UpdateTimestamp();
    }
}

