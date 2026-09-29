using Application.Abstractions.Repositories;
using Application.Abstractions.Services;
using Application.Exceptions;
using Application.Features.Account.Commands.Register;
using Domain.Entities.Carts;
using Domain.Entities.Identity;
using Domain.ValueObjects;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Moq;

namespace Application.Test.FeaturesHandlers.Account;

public class RegisterAccountCommandHandlerTests
{
    private readonly Mock<UserManager<AppUser>> _userManagerMock;
    private readonly Mock<IRepository<Cart>> _cartRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IBackgroundJobService> _backgroundJobsMock;
    private readonly IConfiguration _configuration;
    private readonly RegisterAccountCommand _command;
    private readonly RegisterAccountCommandHandler _sut;

    public RegisterAccountCommandHandlerTests()
    {
        _userManagerMock = TestMocks.CreateMockUserManager();
        _cartRepositoryMock = new Mock<IRepository<Cart>>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _backgroundJobsMock = new Mock<IBackgroundJobService>();

        _command = new RegisterAccountCommand(
            "mohamed",
            "Ahmed",
            "example@gmail.com",
            "Ahemd123!");

        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> {
                ["FrontendUrl"] = "https://example.com"
            })
            .Build();

        _sut = new RegisterAccountCommandHandler(
            _userManagerMock.Object,
            _cartRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _backgroundJobsMock.Object,
            _configuration);
    }

    [Fact]
    public async Task A_User_Can_Create_An_Account()
    {
        // Arrange
        _userManagerMock
            .Setup(x => x.FindByEmailAsync(_command.Email))
            .ReturnsAsync((AppUser?)null);

        _userManagerMock
            .Setup(x => x.CreateAsync(It.IsAny<AppUser>(), _command.Password))
            .ReturnsAsync(IdentityResult.Success);

        _userManagerMock
            .Setup(
                x => x.GenerateEmailConfirmationTokenAsync(It.IsAny<AppUser>())
            )
            .ReturnsAsync("confirmation-token");

        // Act
        var act = () => _sut.Handle(_command, CancellationToken.None);

        // Assert
        await act.Should().NotThrowAsync();

        _userManagerMock.Verify(
            x => x.CreateAsync(It.IsAny<AppUser>(), _command.Password),
            Times.Once);

        _userManagerMock.Verify(
            x => x.FindByEmailAsync(_command.Email),
            Times.Once);

        _userManagerMock.Verify(
            x => x.GenerateEmailConfirmationTokenAsync(It.IsAny<AppUser>()),
            Times.Once);

        _userManagerMock.Verify(
            x => x.AddToRoleAsync(It.IsAny<AppUser>(), Domain.Constants.AppRoles.Customer),
            Times.Once);

    }

    [Fact]
    public async Task A_User_Cannot_Create_An_Account_With_An_Existing_Email()
    {
        // Arrange
        var existingUser = CreateUser(_command.Email);

        _userManagerMock
            .Setup(x => x.FindByEmailAsync(_command.Email))
            .ReturnsAsync(existingUser);

        // Act
        var act = () => _sut.Handle(_command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>();

        _userManagerMock.Verify(
            x => x.FindByEmailAsync(_command.Email),
            Times.Once);
    }

    [Fact]
    public async Task A_Cart_Is_Created_For_A_New_User()
    {
        // Arrange
        AppUser? createdUser = null;
        Cart? createdCart = null;

        _userManagerMock
            .Setup(x => x.FindByEmailAsync(_command.Email))
            .ReturnsAsync((AppUser?)null);

        _userManagerMock
            .Setup(x => x.CreateAsync(It.IsAny<AppUser>(), _command.Password))
            .Callback<AppUser,string>((user, _) => createdUser = user)
            .ReturnsAsync(IdentityResult.Success);

        _userManagerMock
            .Setup(x => x.GenerateEmailConfirmationTokenAsync(It.IsAny<AppUser>()))
            .ReturnsAsync("confirmation-token");

        _cartRepositoryMock
            .Setup(
                x => x.AddAsync(It.IsAny<Cart>(), It.IsAny<CancellationToken>())
            )
            .Callback<Cart,CancellationToken>((cart,_) => createdCart = cart);

        // Act
        await _sut.Handle(_command, CancellationToken.None);

        // Assert
        createdUser.Should().NotBeNull();
        createdCart.Should().NotBeNull();
        createdCart.UserId.Should().Be(createdUser.Id);

        _userManagerMock.Verify(
            x => x.FindByEmailAsync(_command.Email),
            Times.Once);

        _userManagerMock.Verify(
            x => x.CreateAsync(It.IsAny<AppUser>(), _command.Password),
            Times.Once);

        _userManagerMock.Verify(
            x => x.GenerateEmailConfirmationTokenAsync(It.IsAny<AppUser>()),
            Times.Once);

        _cartRepositoryMock.Verify(
           x => x.AddAsync(It.IsAny<Cart>(), It.IsAny<CancellationToken>()),
           Times.Once);
    }

    private static AppUser CreateUser(string email = "test@example.com")
    {
        return new AppUser(new FullName("John", "Doe"), email);
    }
}
