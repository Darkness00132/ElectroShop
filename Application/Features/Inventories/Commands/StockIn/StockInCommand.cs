using MediatR;

namespace Application.Features.Inventories.Commands.StockIn;

public sealed record StockInCommand(Guid ProductId, int Quantity, string? Notes) : IRequest;
