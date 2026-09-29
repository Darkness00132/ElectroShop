namespace Ecommerce.Api.Contracts.Products;

/// <summary>
/// Represents the multipart form for partially updating an existing product.
/// Omitted fields keep their current values.
/// </summary>
/// <param name="NameEn">The new English name, or null to keep the current one.</param>
/// <param name="NameAr">The new Arabic name, or null to keep the current one.</param>
/// <param name="SKU">The new stock keeping unit, or null to keep the current one.</param>
/// <param name="DescriptionEn">The new English description, or null to keep the current one.</param>
/// <param name="DescriptionAr">The new Arabic description, or null to keep the current one.</param>
/// <param name="Price">The new price, or null to keep the current one.</param>
/// <param name="CategoryId">The new category identifier, or null to keep the current one.</param>
/// <param name="BrandId">The new brand identifier, or null to keep the current one.</param>
/// <param name="DeletedImages">The image keys to remove from the product gallery.</param>
/// <param name="NewImages">The images to add to the product gallery.</param>
/// <param name="Attributes">
/// The dynamic attributes that replace the current set, or null to keep the current ones.
/// Existing attribute names not present are removed and the supplied ones are added or updated.
/// </param>
public sealed record UpdateProductRequest(
    string? NameEn,
    string? NameAr,
    string? SKU,
    string? DescriptionEn,
    string? DescriptionAr,
    decimal? Price,
    Guid? CategoryId,
    Guid? BrandId,
    List<string>? DeletedImages,
    IFormFileCollection? NewImages,
    Dictionary<string, string>? Attributes);
