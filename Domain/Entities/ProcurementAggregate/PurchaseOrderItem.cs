using Domain.Common;
using Domain.Entities.Catalog;
using Domain.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Domain.Entities.ProcurementAggregate;

public sealed class PurchaseOrderItem : Entity
{
    public Guid PurchaseOrderId { get; private set; }

    public PurchaseOrder PurchaseOrder { get; private set; } = null!;

    public Guid ProductId { get; private set; }

    public Product Product { get; private set; } = null!;

    public int OrderedQuantity { get; private set; }

    public int ReceivedQuantity { get; private set; }

    [Precision(18, 2)]
    public decimal UnitCost { get; private set; }

    private PurchaseOrderItem() { }

    internal PurchaseOrderItem(
        Guid purchaseOrderId,
        Guid productId,
        int orderedQuantity,
        decimal unitCost)
        : base(Guid.NewGuid())
    {
        if (purchaseOrderId == Guid.Empty)
            throw new DomainException("Purchase order id is required.");

        if (productId == Guid.Empty)
            throw new DomainException("Product id is required.");

        if (orderedQuantity <= 0)
            throw new DomainException(
                "Ordered quantity must be greater than zero.");

        if (unitCost < 0)
            throw new DomainException(
                "Unit cost cannot be negative.");

        PurchaseOrderId = purchaseOrderId;
        ProductId = productId;
        OrderedQuantity = orderedQuantity;
        UnitCost = unitCost;
    }

    internal void IncreaseOrderedQuantity(int quantity)
    {
        if (quantity <= 0) {
            throw new DomainException(
                "Quantity must be greater than zero.");
        }

        OrderedQuantity += quantity;
    }

    public void Receive(int quantity)
    {
        if (quantity <= 0) {
            throw new DomainException(
                "Received quantity must be greater than zero.");
        }

        if (ReceivedQuantity + quantity > OrderedQuantity) {
            throw new DomainException(
                "Received quantity cannot exceed ordered quantity.");
        }

        ReceivedQuantity += quantity;
    }
}
