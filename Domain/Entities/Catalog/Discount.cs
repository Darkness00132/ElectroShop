using System.ComponentModel.DataAnnotations;
using Domain.Common;
using Domain.Enums;
using Domain.Exceptions;
using Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace Domain.Entities.Catalog;

public sealed class Discount : Entity
{
    private readonly List<Product> _products = new();

    [MaxLength(100)]
    public string Name { get; private set; } = null!;

    public DiscountType DiscountType { get; private set; }

    [Precision(18, 4)]
    public decimal Value { get; private set; }

    public DateRange ValidityPeriod { get; private set; } = null!;

    public bool IsVisible { get; private set; }

    public IReadOnlyCollection<Product> Products => _products.AsReadOnly();

    public DateOnly StartDate => ValidityPeriod.StartDate;

    public DateOnly EndDate => ValidityPeriod.EndDate;

    public bool IsActive => IsValidOn(DateOnly.FromDateTime(DateTime.UtcNow));

    private Discount() { } // Required for EF Core

    public Discount(
        string name,
        DiscountType discountType,
        decimal value,
        DateRange validityPeriod)
    {
        UpdateDetails(name, discountType, value, validityPeriod);
        Id = Guid.NewGuid();
        IsVisible = true;
    }

    public void UpdateDetails(
        string name,
        DiscountType discountType,
        decimal value,
        DateRange validityPeriod)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Discount name is required.");

        if (validityPeriod is null)
            throw new DomainException("Validity period is required.");

        ValidateValue(discountType, value);

        Name = name.Trim();
        DiscountType = discountType;
        Value = value;
        ValidityPeriod = validityPeriod;
    }

    public bool IsValidOn(DateOnly date)
    {
        return IsVisible && ValidityPeriod.IsValidOn(date);
    }

    public void Activate() => IsVisible = true;

    public void Deactivate() => IsVisible = false;

    public decimal CalculateDiscountAmount(decimal originalPrice)
    {
        if (originalPrice <= 0)
            throw new DomainException("Original price must be greater than zero.");

        return DiscountType switch {
            DiscountType.Percentage => originalPrice * (Value / 100),
            DiscountType.FixedAmount => Value,
            _ => throw new DomainException("Invalid discount type.")
        };
    }

    private static void ValidateValue(DiscountType discountType, decimal value)
    {
        if (value <= 0)
            throw new DomainException("Discount value must be greater than zero.");

        if (discountType == DiscountType.Percentage && value > 100)
            throw new DomainException("Percentage discount cannot exceed 100.");
    }
}
