using MediatR;

namespace Application.Features.PromoCodes.Commands.DeletePromoCode;

public sealed record DeletePromoCodeCommand(Guid Id) : IRequest;
