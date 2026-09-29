using MediatR;

namespace Application.Features.Inventories.Commands.AdjustStock;

public sealed record AdjustStockCommand(Guid ProductId, int NewQuantity, string? Notes) : IRequest;
