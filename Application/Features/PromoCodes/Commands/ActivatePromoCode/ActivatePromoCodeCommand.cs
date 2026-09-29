using MediatR;

namespace Application.Features.PromoCodes.Commands.ActivatePromoCode;

public sealed record ActivatePromoCodeCommand(Guid Id) : IRequest;
