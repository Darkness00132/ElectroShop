using Application.Abstractions.Repositories;
using Application.Exceptions;
using Application.Features.Procurement.GoodsReceipts.Commands.ConfirmGoodsReceipt;
using Application.Features.Procurement.GoodsReceipts.Commands.CreateGoodsReceipt;
using Application.Features.Procurement.PurchaseOrders.Commands.ApprovePurchaseOrder;
using Application.Features.Procurement.PurchaseOrders.Commands.CreatePurchaseOrder;
using Application.Features.Procurement.Suppliers.Commands.CreateSupplier;
using Domain.Entities.Catalog;
using Domain.Entities.InventoryAggregate;
using Domain.Entities.ProcurementAggregate;
using Domain.Enums;
using Domain.Exceptions;
using FluentAssertions;
using Moq;

namespace Application.Test.FeaturesHandlers.Procurement;

public class ProcurementHandlerTests
{
    private readonly Mock<IRepository<Supplier>> _supplierRepositoryMock;
    private readonly Mock<IRepository<PurchaseOrder>> _purchaseOrderRepositoryMock;
    private readonly Mock<IRepository<GoodsReceipt>> _goodsReceiptRepositoryMock;
    private readonly Mock<IRepository<Inventory>> _inventoryRepositoryMock;
    private readonly Mock<IRepository<Product>> _productRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;

    public ProcurementHandlerTests()
    {
        _supplierRepositoryMock = new Mock<IRepository<Supplier>>();
        _purchaseOrderRepositoryMock = new Mock<IRepository<PurchaseOrder>>();
        _goodsReceiptRepositoryMock = new Mock<IRepository<GoodsReceipt>>();
        _inventoryRepositoryMock = new Mock<IRepository<Inventory>>();
        _productRepositoryMock = new Mock<IRepository<Product>>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
    }

    [Fact]
    public async Task A_Supplier_Can_Be_Created()
    {
        // Arrange
        Supplier? addedSupplier = null;

        _supplierRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Supplier>(), It.IsAny<CancellationToken>()))
            .Callback<Supplier, CancellationToken>((supplier, _) => addedSupplier = supplier);

        var handler = new CreateSupplierHandler(_supplierRepositoryMock.Object, _unitOfWorkMock.Object);

        // Act
        await handler.Handle(new CreateSupplierCommand(
            "Tech Distributor", "John Smith", "john@tech.com", null, null, null, null), CancellationToken.None);

        // Assert
        addedSupplier.Should().NotBeNull();
        addedSupplier!.Name.Should().Be("Tech Distributor");
        addedSupplier.IsActive.Should().BeTrue();

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task A_Purchase_Order_Cannot_Be_Created_With_A_Duplicate_Number()
    {
        // Arrange
        _supplierRepositoryMock
            .Setup(x => x.ExistsAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Supplier, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _purchaseOrderRepositoryMock
            .Setup(x => x.ExistsAsync(It.IsAny<System.Linq.Expressions.Expression<Func<PurchaseOrder, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var handler = new CreatePurchaseOrderHandler(_purchaseOrderRepositoryMock.Object, _supplierRepositoryMock.Object, _unitOfWorkMock.Object);

        // Act
        var act = () => handler.Handle(new CreatePurchaseOrderCommand(
            "PO-001", Guid.NewGuid(), DateOnly.FromDateTime(DateTime.UtcNow), null, null), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task A_Purchase_Order_Cannot_Be_Approved_When_It_Does_Not_Exist()
    {
        // Arrange
        _purchaseOrderRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((PurchaseOrder?)null);

        var handler = new ApprovePurchaseOrderHandler(_purchaseOrderRepositoryMock.Object, _unitOfWorkMock.Object);

        // Act
        var act = () => handler.Handle(new ApprovePurchaseOrderCommand(Guid.NewGuid()), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Confirming_A_Goods_Receipt_Records_Received_Quantities_And_Increases_Stock()
    {
        // Arrange
        var product = CreateProduct();
        var purchaseOrder = CreateApprovedPurchaseOrder(product.Id, orderedQuantity: 10);
        var goodsReceipt = CreateDraftReceipt(purchaseOrder.Id, product.Id, receivedQuantity: 4);
        var inventory = CreateInventory(product.Id, quantityOnHand: 6);

        SetupReceiptFound(goodsReceipt);
        SetupPurchaseOrderFound(purchaseOrder);
        SetupInventoriesFound(inventory);

        var handler = new ConfirmGoodsReceiptHandler(
            _goodsReceiptRepositoryMock.Object,
            _purchaseOrderRepositoryMock.Object,
            _inventoryRepositoryMock.Object,
            _unitOfWorkMock.Object);

        // Act
        await handler.Handle(new ConfirmGoodsReceiptCommand(goodsReceipt.Id), CancellationToken.None);

        // Assert
        goodsReceipt.Status.Should().Be(GoodsReceiptStatus.Confirmed);
        purchaseOrder.Items.Single().ReceivedQuantity.Should().Be(4);
        purchaseOrder.Status.Should().Be(PurchaseOrderStatus.PartiallyReceived);

        inventory.QuantityOnHand.Should().Be(10);
        inventory.Transactions.Should().ContainSingle(t =>
            t.Type == InventoryTransactionType.StockIn &&
            t.GoodsReceiptId == goodsReceipt.Id);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Confirming_A_Full_Receipt_Completes_The_Purchase_Order()
    {
        // Arrange
        var product = CreateProduct();
        var purchaseOrder = CreateApprovedPurchaseOrder(product.Id, orderedQuantity: 4);
        var goodsReceipt = CreateDraftReceipt(purchaseOrder.Id, product.Id, receivedQuantity: 4);
        var inventory = CreateInventory(product.Id, quantityOnHand: 0);

        SetupReceiptFound(goodsReceipt);
        SetupPurchaseOrderFound(purchaseOrder);
        SetupInventoriesFound(inventory);

        var handler = new ConfirmGoodsReceiptHandler(
            _goodsReceiptRepositoryMock.Object,
            _purchaseOrderRepositoryMock.Object,
            _inventoryRepositoryMock.Object,
            _unitOfWorkMock.Object);

        // Act
        await handler.Handle(new ConfirmGoodsReceiptCommand(goodsReceipt.Id), CancellationToken.None);

        // Assert
        purchaseOrder.Status.Should().Be(PurchaseOrderStatus.Completed);
        inventory.QuantityOnHand.Should().Be(4);
    }

    [Fact]
    public async Task Confirming_A_Receipt_For_A_Product_Outside_The_Purchase_Order_Is_Rejected()
    {
        // Arrange
        var product = CreateProduct();
        var purchaseOrder = CreateApprovedPurchaseOrder(product.Id, orderedQuantity: 10);
        var goodsReceipt = CreateDraftReceipt(purchaseOrder.Id, Guid.NewGuid(), receivedQuantity: 4);

        SetupReceiptFound(goodsReceipt);
        SetupPurchaseOrderFound(purchaseOrder);

        var handler = new ConfirmGoodsReceiptHandler(
            _goodsReceiptRepositoryMock.Object,
            _purchaseOrderRepositoryMock.Object,
            _inventoryRepositoryMock.Object,
            _unitOfWorkMock.Object);

        // Act
        var act = () => handler.Handle(new ConfirmGoodsReceiptCommand(goodsReceipt.Id), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>();

        goodsReceipt.Status.Should().Be(GoodsReceiptStatus.Draft);
    }

    [Fact]
    public async Task A_Goods_Receipt_Cannot_Be_Created_With_A_Duplicate_Number()
    {
        // Arrange
        _purchaseOrderRepositoryMock
            .Setup(x => x.ExistsAsync(It.IsAny<System.Linq.Expressions.Expression<Func<PurchaseOrder, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _goodsReceiptRepositoryMock
            .Setup(x => x.ExistsAsync(It.IsAny<System.Linq.Expressions.Expression<Func<GoodsReceipt, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var handler = new CreateGoodsReceiptHandler(_goodsReceiptRepositoryMock.Object, _purchaseOrderRepositoryMock.Object, _unitOfWorkMock.Object);

        // Act
        var act = () => handler.Handle(new CreateGoodsReceiptCommand(
            "GR-001", Guid.NewGuid(), DateTime.UtcNow, null, null), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>();
    }

    private static Product CreateProduct()
    {
        return new Product(
            "Gaming Laptop",
            "لابتوب ألعاب",
            "A laptop for gaming.",
            "لابتوب للألعاب.",
            "SKU-001",
            200m,
            Guid.NewGuid(),
            Guid.NewGuid());
    }

    private static PurchaseOrder CreateApprovedPurchaseOrder(Guid productId, int orderedQuantity)
    {
        var purchaseOrder = new PurchaseOrder(
            "PO-001",
            Guid.NewGuid(),
            DateOnly.FromDateTime(DateTime.UtcNow));

        purchaseOrder.AddItem(productId, orderedQuantity, 150m);
        purchaseOrder.SubmitForApproval();
        purchaseOrder.Approve();

        return purchaseOrder;
    }

    private static GoodsReceipt CreateDraftReceipt(Guid purchaseOrderId, Guid productId, int receivedQuantity)
    {
        var goodsReceipt = new GoodsReceipt(
            "GR-001",
            purchaseOrderId,
            DateTime.UtcNow);

        goodsReceipt.AddItem(productId, receivedQuantity);

        return goodsReceipt;
    }

    private static Inventory CreateInventory(Guid productId, int quantityOnHand)
    {
        return new Inventory(productId, quantityOnHand, reorderLevel: 2);
    }

    private void SetupReceiptFound(GoodsReceipt goodsReceipt)
    {
        _goodsReceiptRepositoryMock
            .Setup(x => x.SingleOrDefaultAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<GoodsReceipt, bool>>>(),
                It.IsAny<CancellationToken>(),
                It.IsAny<System.Linq.Expressions.Expression<Func<GoodsReceipt, object?>>[]>()))
            .ReturnsAsync(goodsReceipt);
    }

    private void SetupPurchaseOrderFound(PurchaseOrder purchaseOrder)
    {
        _purchaseOrderRepositoryMock
            .Setup(x => x.SingleOrDefaultAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<PurchaseOrder, bool>>>(),
                It.IsAny<CancellationToken>(),
                It.IsAny<System.Linq.Expressions.Expression<Func<PurchaseOrder, object?>>[]>()))
            .ReturnsAsync(purchaseOrder);
    }

    private void SetupInventoriesFound(params Inventory[] inventories)
    {
        _inventoryRepositoryMock
            .Setup(x => x.ListAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<Inventory, bool>>>(),
                It.IsAny<CancellationToken>(),
                It.IsAny<System.Linq.Expressions.Expression<Func<Inventory, object?>>[]>()))
            .ReturnsAsync(inventories.ToList());
    }
}
