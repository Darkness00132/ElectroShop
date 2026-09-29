using MediatR;

namespace Application.Features.PromoCodes.Commands.DeactivatePromoCode;

public sealed record DeactivatePromoCodeCommand(Guid Id) : IRequest;
