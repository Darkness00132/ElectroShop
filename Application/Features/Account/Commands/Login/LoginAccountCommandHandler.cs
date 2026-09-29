using Application.Features.Account.Dto;
using Application.Features.Account.Services;
using Domain.Entities.Identity;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Application.Features.Account.Commands.Login;

internal class LoginAccountCommandHandler
    : IRequestHandler<LoginAccountCommand, AccountTokenDto>
{
    private readonly UserManager<AppUser> _userManager;
    private readonly AccountTokenService _tokenService;

    public LoginAccountCommandHandler(UserManager<AppUser> userManager, AccountTokenService tokenService)
    {
        _userManager = userManager;
        _tokenService = tokenService;
    }

    public async Task<AccountTokenDto> Handle(LoginAccountCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);

        if (user is null || await _userManager.IsLockedOutAsync(user))
            throw new UnauthorizedAccessException("Invalid email or password.");

        if (!await _userManager.CheckPasswordAsync(user, request.Password)) {
            await _userManager.AccessFailedAsync(user);

            throw new UnauthorizedAccessException("Invalid email or password.");
        }

        await _userManager.ResetAccessFailedCountAsync(user);

        return await _tokenService.CreateAsync(user, cancellationToken);
    }
}
