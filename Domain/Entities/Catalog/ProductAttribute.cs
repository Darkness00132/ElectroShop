using System.ComponentModel.DataAnnotations;
using Domain.Common;
using Domain.Exceptions;

namespace Domain.Entities.Catalog;

public sealed class ProductAttribute : IEntity
{
    public Guid ProductId { get; private set; }

    [MaxLength(50)]
    public string Name { get; private set; } = null!;

    [MaxLength(200)]
    public string Value { get; private set; } = null!;

    public Product Product { get; private set; } = null!;

    private ProductAttribute() { } // Required for EF Core

    internal ProductAttribute(Guid productId, string name, string value)
    {
        if (productId == Guid.Empty)
            throw new DomainException("Product ID cannot be empty.");

        ProductId = productId;
        Name = ValidateName(name);
        Value = ValidateValue(value);
    }

    internal void UpdateValue(string value)
    {
        Value = ValidateValue(value);
    }

    private static string ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Product attribute name is required.");

        var trimmedName = name.Trim();

        if (trimmedName.Length > 50)
            throw new DomainException("Product attribute name cannot exceed 50 characters.");

        return trimmedName;
    }

    private static string ValidateValue(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("Product attribute value is required.");

        var trimmedValue = value.Trim();

        if (trimmedValue.Length > 200)
            throw new DomainException("Product attribute value cannot exceed 200 characters.");

        return trimmedValue;
    }
}
