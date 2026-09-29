using Application.Abstractions.Repositories;
using Application.Abstractions.Services;
using Application.Exceptions;
using Application.Features.Account.Dto;
using Domain.Constants;
using Domain.Entities.Carts;
using Domain.Entities.Identity;
using Domain.ValueObjects;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;

namespace Application.Features.Account.Commands.Register;

internal class RegisterAccountCommandHandler : IRequestHandler<RegisterAccountCommand>
{
    private readonly UserManager<AppUser> _userManager;
    private readonly IRepository<Cart> _cartRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IBackgroundJobService _backgroundJobs;
    private readonly IConfiguration _configuration;

    public RegisterAccountCommandHandler(UserManager<AppUser> userManager, IRepository<Cart> cartRepository, IUnitOfWork unitOfWork, IBackgroundJobService backgroundJobs, IConfiguration configuration)
    {
        _userManager = userManager;
        _cartRepository = cartRepository;
        _unitOfWork = unitOfWork;
        _backgroundJobs = backgroundJobs;
        _configuration = configuration;
    }

    public async Task Handle(RegisterAccountCommand request, CancellationToken cancellationToken)
    {
        if (await _userManager.FindByEmailAsync(request.Email) is not null)
            throw new ConflictException("A user with this email already exists.");

        var user = new AppUser(new FullName(request.FirstName, request.LastName), request.Email);

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded) {
            throw new ValidationException(result.Errors.GroupBy(x => x.Code).ToDictionary(x => x.Key, x => x.Select(e => e.Description).ToArray()));
        }

        await _userManager.AddToRoleAsync(user, AppRoles.Customer);

        var cart = new Cart(user.Id);
        await _cartRepository.AddAsync(cart);
        await _unitOfWork.SaveChangesAsync();

        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        var frontendUrl = _configuration["FrontendUrl"] ?? throw new InvalidOperationException("FrontendUrl configuration is required.");

        var url = $"{frontendUrl.TrimEnd('/')}/account/confirm-email?email={Uri.EscapeDataString(user.Email!)}&token={Uri.EscapeDataString(token)}";

        var email = user.Email!;
        var model = new EmailConfirmationEmailModel(user.DisplayName, url);

        _backgroundJobs.Enqueue<IEmailSender>(sender => sender.SendAsync(email, "Confirm your email", "ConfirmEmail", model));
    }
}


