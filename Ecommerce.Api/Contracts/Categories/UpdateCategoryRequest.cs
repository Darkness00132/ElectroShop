namespace Ecommerce.Api.Contracts.Categories;

/// <summary>
/// Represents the request body for updating an existing category.
/// All category data must be supplied; the image is only replaced when a new one is provided.
/// </summary>
/// <param name="NameEn">The English name of the category.</param>
/// <param name="NameAr">The Arabic name of the category.</param>
/// <param name="DescriptionEn">The English description of the category.</param>
/// <param name="DescriptionAr">The Arabic description of the category.</param>
/// <param name="NewImage">The optional replacement image for the category.</param>
public sealed record UpdateCategoryRequest(
    string NameEn,
    string NameAr,
    string DescriptionEn,
    string DescriptionAr,
    IFormFile? NewImage);
