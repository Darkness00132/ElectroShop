namespace Ecommerce.Api.Contracts.Products;

/// <summary>
/// Represents the multipart form for creating a product with its gallery images.
/// </summary>
/// <param name="NameEn">The English name of the product.</param>
/// <param name="NameAr">The Arabic name of the product.</param>
/// <param name="DescriptionEn">The English description of the product.</param>
/// <param name="DescriptionAr">The Arabic description of the product.</param>
/// <param name="SKU">The stock keeping unit of the product.</param>
/// <param name="Price">The price of the product.</param>
/// <param name="IsActive">Whether the product is visible in the storefront.</param>
/// <param name="CategoryId">The identifier of the category the product belongs to.</param>
/// <param name="BrandId">The identifier of the brand the product belongs to.</param>
/// <param name="DiscountId">The optional identifier of the discount assigned to the product.</param>
/// <param name="Attributes">The dynamic key-value attributes of the product, such as RAM or color.</param>
/// <param name="Images">The gallery images of the product; 1 to 5 images of at most 10 MB each.</param>
public sealed record CreateProductRequest(
    string NameEn,
    string NameAr,
    string DescriptionEn,
    string DescriptionAr,
    string SKU,
    decimal Price,
    bool IsActive,
    Guid CategoryId,
    Guid BrandId,
    Guid? DiscountId,
    Dictionary<string, string>? Attributes,
    IFormFileCollection Images);
