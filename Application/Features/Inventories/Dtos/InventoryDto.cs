namespace Application.Features.Inventories.Dtos;

public sealed record InventoryDto(
    Guid Id,
    Guid ProductId,
    string ProductNameEn,
    string ProductNameAr,
    string SKU,
    int QuantityOnHand,
    int ReorderLevel,
    bool IsLowStock);
