using System.Linq.Expressions;
using Application.Abstractions.Repositories;
using Application.Abstractions.Services;
using Application.Features.Account.Services;
using Application.Settings;
using Domain.Entities.Identity;
using Domain.ValueObjects;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;

namespace Application.Test.Services;

public class AccountTokenServiceTests
{
    private readonly Mock<IRepository<RefreshToken>> _refreshTokenRepo;
    private readonly Mock<IUnitOfWork> _unitOfWork;
    private readonly Mock<IJwtTokenService> _jwtTokenService;
    private readonly IOptions<JwtSettings> _jwtOptions;
    private readonly AccountTokenService _sut;

    public AccountTokenServiceTests()
    {
        _refreshTokenRepo = new Mock<IRepository<RefreshToken>>();
        _unitOfWork = new Mock<IUnitOfWork>();
        _jwtTokenService = new Mock<IJwtTokenService>();

        _jwtOptions = Options.Create(new JwtSettings {
            Key = "ThisIsASecretKeyForJwtTokenGeneration12345",
            Audience = "TestAudience",
            Issuer = "TestIssuer",
            AccessTokenExpirationInMinutes = 15,
            RefreshTokenExpirationInDays = 7
        });

        _sut = new AccountTokenService(
            _refreshTokenRepo.Object,
            _unitOfWork.Object,
            _jwtTokenService.Object,
            _jwtOptions);
    }

    [Fact]
    public async Task A_User_Can_Start_A_Session()
    {
        // Arrange
        var user = CreateTestUser();
        var accessExpires = DateTime.UtcNow.AddMinutes(15);

        _jwtTokenService
            .Setup(x => x.GenerateAsync(
                user,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(("access-token", accessExpires));

        // Act
        var result = await _sut.CreateAsync(
            user,
            CancellationToken.None);

        // Assert
        result.AccessToken.Should().Be("access-token");
        result.AccessTokenExpiresAt.Should().Be(accessExpires);
        result.RefreshToken.Should().NotBeNullOrWhiteSpace();
        result.RefreshTokenExpiresAt.Should().BeAfter(DateTime.UtcNow);
    }

    [Fact]
    public async Task A_User_Can_Continue_A_Session()
    {
        // Arrange
        var user = CreateTestUser();

        var refreshToken = user.AddRefreshToken(
            "refresh-token",
            DateTime.UtcNow.AddDays(7));

        _refreshTokenRepo
            .Setup(x => x.SingleOrDefaultAsync(
                It.IsAny<Expression<Func<RefreshToken, bool>>>(),
                It.IsAny<CancellationToken>(),
                It.IsAny<Expression<Func<RefreshToken, object?>>[]>()))
            .ReturnsAsync(refreshToken);

        _jwtTokenService
            .Setup(x => x.GenerateAsync(
                user,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((
                "new-access-token",
                DateTime.UtcNow.AddMinutes(15)));

        // Act
        var result = await _sut.RefreshAsync(
            "refresh-token",
            CancellationToken.None);

        // Assert
        result.AccessToken.Should().Be("new-access-token");
        result.RefreshToken.Should().NotBeNullOrWhiteSpace();
        result.RefreshToken.Should().NotBe("refresh-token");

        refreshToken.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task A_User_Cannot_Continue_A_Session_With_Invalid_Authorization()
    {
        // Arrange
        _refreshTokenRepo
            .Setup(x => x.SingleOrDefaultAsync(
                It.IsAny<Expression<Func<RefreshToken, bool>>>(),
                It.IsAny<CancellationToken>(),
                It.IsAny<Expression<Func<RefreshToken, object?>>[]>()))
            .ReturnsAsync((RefreshToken?)null);

        // Act
        var act = () => _sut.RefreshAsync(
            "invalid-token",
            CancellationToken.None);

        // Assert
        await act.Should()
            .ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("The refresh token is invalid or expired.");
    }

    [Fact]
    public async Task A_User_Cannot_Continue_A_Session_After_Their_Authorization_Expires()
    {
        // Arrange
        var user = CreateTestUser();

        var refreshToken = user.AddRefreshToken(
            "expired-token",
            DateTime.UtcNow.AddMilliseconds(200));

        _refreshTokenRepo
            .Setup(x => x.SingleOrDefaultAsync(
                It.IsAny<Expression<Func<RefreshToken, bool>>>(),
                It.IsAny<CancellationToken>(),
                It.IsAny<Expression<Func<RefreshToken, object?>>[]>()))
            .ReturnsAsync(refreshToken);

        // Act
        await Task.Delay(200);

        var act = () => _sut.RefreshAsync(
            "expired-token",
            CancellationToken.None);

        // Assert
        await act.Should()
            .ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task A_User_Can_End_A_Session()
    {
        // Arrange
        var user = CreateTestUser();

        var refreshToken = user.AddRefreshToken(
            "refresh-token",
            DateTime.UtcNow.AddDays(7));

        _refreshTokenRepo
            .Setup(x => x.SingleOrDefaultAsync(
                It.IsAny<Expression<Func<RefreshToken, bool>>>(),
                It.IsAny<CancellationToken>(),
                It.IsAny<Expression<Func<RefreshToken, object?>>[]>()))
            .ReturnsAsync(refreshToken);

        // Act
        await _sut.RevokeAsync(
            "refresh-token",
            CancellationToken.None);

        // Assert
        refreshToken.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task Ending_A_Session_That_Does_Not_Exist_Has_No_Effect()
    {
        // Arrange
        _refreshTokenRepo
            .Setup(x => x.SingleOrDefaultAsync(
                It.IsAny<Expression<Func<RefreshToken, bool>>>(),
                It.IsAny<CancellationToken>(),
                It.IsAny<Expression<Func<RefreshToken, object?>>[]>()))
            .ReturnsAsync((RefreshToken?)null);

        // Act
        var act = () => _sut.RevokeAsync(
            "invalid-token",
            CancellationToken.None);

        // Assert
        await act.Should().NotThrowAsync();
    }

    private static AppUser CreateTestUser(
        string email = "john@example.com")
    {
        return new AppUser(new FullName("John", "Doe"), email);
    }
}
