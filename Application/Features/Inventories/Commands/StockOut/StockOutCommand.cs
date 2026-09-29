using MediatR;

namespace Application.Features.Inventories.Commands.StockOut;

public sealed record StockOutCommand(Guid ProductId, int Quantity, string? Notes) : IRequest;
