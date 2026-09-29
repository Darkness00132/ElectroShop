using Application.Features.Account.Commands.Login;
using Application.Features.Account.Dto;
using Application.Features.Account.Services;
using Domain.Entities.Identity;
using Domain.ValueObjects;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace Application.Test.FeaturesHandlers.Account;

public class LoginAccountCommandHandlerTests
{
    private readonly Mock<UserManager<AppUser>> _userManager;
    private readonly AccountTokenService _tokenService;
    private readonly LoginAccountCommand _command;
    private readonly LoginAccountCommandHandler _sut;

    public LoginAccountCommandHandlerTests()
    {
        _userManager = TestMocks.CreateMockUserManager();
        _tokenService = TestMocks.CreateMockAccountTokenService();

        _command = new LoginAccountCommand(
            "test@gmail.com",
            "Testing123!");

        _sut = new LoginAccountCommandHandler(_userManager.Object, _tokenService);
    }

    [Fact]
    public async Task A_User_Can_Log_In_With_Valid_Credentials()
    {
        // Arrange
        var user = CreateUser(_command.Email);

        _userManager
            .Setup(x => x.FindByEmailAsync(_command.Email))
            .ReturnsAsync(user);

        _userManager
            .Setup(x => x.CheckPasswordAsync(user, _command.Password))
            .ReturnsAsync(true);

        // Act
        var result = await _sut.Handle(_command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task A_User_Cannot_Log_In_When_The_Email_Does_Not_Exist()
    {
        // Arrange
        _userManager
            .Setup(x => x.FindByEmailAsync(_command.Email))
            .ReturnsAsync((AppUser?)null);

        // Act
        var act = () => _sut.Handle(_command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task A_User_Cannot_Log_In_With_An_Incorrect_Password()
    {
        // Arrange
        var user = CreateUser(_command.Email);

        _userManager
            .Setup(x => x.FindByEmailAsync(_command.Email))
            .ReturnsAsync(user);

        _userManager
            .Setup(x => x.CheckPasswordAsync(user, _command.Password))
            .ReturnsAsync(false);

        // Act
        var act = () => _sut.Handle(_command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>();

        _userManager.Verify(
            x => x.AccessFailedAsync(user),
            Times.Once);
    }

    [Fact]
    public async Task A_Locked_Out_User_Cannot_Log_In()
    {
        // Arrange
        var user = CreateUser(_command.Email);

        _userManager
            .Setup(x => x.FindByEmailAsync(_command.Email))
            .ReturnsAsync(user);

        _userManager
            .Setup(x => x.IsLockedOutAsync(user))
            .ReturnsAsync(true);

        // Act
        var act = () => _sut.Handle(_command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>();

        _userManager.Verify(
            x => x.CheckPasswordAsync(It.IsAny<AppUser>(), It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task A_Successful_Login_Resets_The_Failed_Access_Count()
    {
        // Arrange
        var user = CreateUser(_command.Email);

        _userManager
            .Setup(x => x.FindByEmailAsync(_command.Email))
            .ReturnsAsync(user);

        _userManager
            .Setup(x => x.CheckPasswordAsync(user, _command.Password))
            .ReturnsAsync(true);

        // Act
        await _sut.Handle(_command, CancellationToken.None);

        // Assert
        _userManager.Verify(
            x => x.ResetAccessFailedCountAsync(user),
            Times.Once);
    }

    private static AppUser CreateUser(string email)
    {
        return new AppUser(new FullName("John", "Doe"), email);
    }
}
