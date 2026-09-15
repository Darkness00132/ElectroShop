namespace Ecommerce.Api.Contracts.Brands;

/// <summary>
/// Represents the request body for updating an existing brand.
/// Omitted names keep their current values.
/// </summary>
/// <param name="NameEn">The new English name, or null to keep the current one.</param>
/// <param name="NameAr">The new Arabic name, or null to keep the current one.</param>
public sealed record UpdateBrandRequest(string NameEn,string NameAr);
