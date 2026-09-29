namespace Ecommerce.Api.Contracts.Inventories;

/// <summary>
/// Represents the request body for receiving stock into the inventory.
/// </summary>
/// <param name="Quantity">The quantity to add; must be greater than zero.</param>
/// <param name="Notes">Optional notes recorded on the inventory transaction.</param>
public sealed record StockInRequest(int Quantity, string? Notes);

/// <summary>
/// Represents the request body for removing stock from the inventory.
/// </summary>
/// <param name="Quantity">The quantity to remove; must not exceed the quantity on hand.</param>
/// <param name="Notes">Optional notes recorded on the inventory transaction.</param>
public sealed record StockOutRequest(int Quantity, string? Notes);

/// <summary>
/// Represents the request body for adjusting the stock to an absolute quantity.
/// </summary>
/// <param name="NewQuantity">The new quantity on hand; cannot be negative.</param>
/// <param name="Notes">Optional notes recorded on the inventory transaction.</param>
public sealed record AdjustStockRequest(int NewQuantity, string? Notes);

/// <summary>
/// Represents the request body for changing the reorder level of a product.
/// </summary>
/// <param name="ReorderLevel">The new reorder level; cannot be negative.</param>
public sealed record ChangeReorderLevelRequest(int ReorderLevel);
