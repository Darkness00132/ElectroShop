using System.ComponentModel.DataAnnotations;
using Domain.Common;
using Domain.Entities.Catalog;
using Domain.Enums;
using Domain.Exceptions;

namespace Domain.Entities.InventoryAggregate;

public sealed class Inventory : AggregateRoot
{
    private readonly List<InventoryTransaction> _transactions = new();

    public Guid ProductId { get; private set; }

    public Product Product { get; private set; } = null!;

    public int QuantityOnHand { get; private set; }

    public int ReorderLevel { get; private set; }

    [Timestamp]
    public byte[] RowVersion { get; private set; } = [];

    public IReadOnlyCollection<InventoryTransaction> Transactions => _transactions.AsReadOnly();

    private Inventory() { }

    public Inventory(Guid productId, int quantityOnHand, int reorderLevel)
        : base(Guid.NewGuid())
    {
        if (productId == Guid.Empty)
            throw new DomainException("Product ID cannot be empty.");

        if (quantityOnHand < 0)
            throw new DomainException("Quantity on hand cannot be negative.");

        if (reorderLevel < 0)
            throw new DomainException("Reorder level cannot be negative.");

        ProductId = productId;
        QuantityOnHand = quantityOnHand;
        ReorderLevel = reorderLevel;
    }

    public void IncreaseStock(
        int quantity,
        Guid? orderId = null,
        Guid? goodsReceiptId = null,
        string? notes = null)
    {
        if (quantity <= 0)
            throw new DomainException("Quantity must be greater than zero.");

        var quantityBefore = QuantityOnHand;
        QuantityOnHand += quantity;

        AddTransaction(
            InventoryTransactionType.StockIn,
            quantity,
            quantityBefore,
            QuantityOnHand,
            orderId,
            goodsReceiptId,
            notes
        );
    }

    public void DecreaseStock(
        int quantity,
        Guid? orderId = null,
        string? notes = null)
    {
        if (quantity <= 0)
            throw new DomainException("Quantity must be greater than zero.");

        if (QuantityOnHand < quantity)
            throw new DomainException("Insufficient stock.");

        var quantityBefore = QuantityOnHand;
        QuantityOnHand -= quantity;

        AddTransaction(InventoryTransactionType.StockOut,
            -quantity,
            quantityBefore,
            QuantityOnHand,
            orderId,
            null,
            notes
        );
    }

    public void AdjustStock(int newQuantity, string? notes = null)
    {
        if (newQuantity < 0)
            throw new DomainException("New quantity cannot be negative.");

        var quantityBefore = QuantityOnHand;
        var quantityChange = newQuantity - QuantityOnHand;

        QuantityOnHand = newQuantity;

        AddTransaction(
            InventoryTransactionType.Adjustment,
            quantityChange,
            quantityBefore,
            QuantityOnHand,
            null,
            null,
            notes
        );
    }

    public void ChangeReorderLevel(int reorderLevel)
    {
        if (reorderLevel < 0)
            throw new DomainException("Reorder level cannot be negative.");

        ReorderLevel = reorderLevel;
    }

    private void AddTransaction(
        InventoryTransactionType type,
        int quantityChange,
        int quantityBefore,
        int quantityAfter,
        Guid? orderId,
        Guid? goodsReceiptId,
        string? notes)
    {
        _transactions.Add(new InventoryTransaction(
            Id,
            type,
            quantityChange,
            quantityBefore,
            quantityAfter,
            orderId,
            goodsReceiptId,
            notes)
        );
    }
}
