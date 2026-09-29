using MediatR;

namespace Application.Features.Inventories.Commands.ChangeReorderLevel;

public sealed record ChangeReorderLevelCommand(Guid ProductId, int ReorderLevel) : IRequest;
