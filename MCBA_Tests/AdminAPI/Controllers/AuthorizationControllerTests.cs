using MCBA_AdminWebApi.Controllers;
using MCBA_AdminWebApi.Models;
using MCBA_AdminWebApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace MCBA_Tests.AdminAPI.Controllers;

public class AuthorizationControllerTests
{
    private readonly AuthorizationController _controller;

    public AuthorizationControllerTests()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["AdminCredentials:Username"] = "admin",
                ["AdminCredentials:Password"] = "admin"
            })
            .Build();
        _controller = new AuthorizationController(configuration);
    }

    [Fact]
    public void Login_ReturnsOkWithToken_WhenCredentialsAreValid()
    {
        var credentials = new LoginRequest { username = "admin", password = "admin" };

        var result = _controller.Login(credentials);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var token = Assert.IsType<string>(okResult.Value);
        Assert.NotEmpty(token);
        Assert.True(TokenStore.IsValidToken(token));
    }

    [Fact]
    public void Login_ReturnsUnauthorized_WhenCredentialsAreInvalid()
    {
        var credentials = new LoginRequest { username = "wrong", password = "wrong" };

        var result = _controller.Login(credentials);

        var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
        Assert.Equal("Invalid username or password", unauthorizedResult.Value);
    }

    [Fact]
    public void Login_GeneratesUniqueTokens()
    {
        var credentials = new LoginRequest { username = "admin", password = "admin" };

        var result1 = _controller.Login(credentials);
        var result2 = _controller.Login(credentials);

        var token1 = (result1 as OkObjectResult)?.Value as string;
        var token2 = (result2 as OkObjectResult)?.Value as string;
        
        Assert.NotEqual(token1, token2);
        Assert.True(TokenStore.IsValidToken(token1));
        Assert.True(TokenStore.IsValidToken(token2));
    }

    [Fact]
    public void Logout_RemovesTokenFromStore()
    {
        var credentials = new LoginRequest { username = "admin", password = "admin" };
        var loginResult = _controller.Login(credentials);
        var token = (loginResult as OkObjectResult)?.Value as string;

        _controller.Logout(token);

        Assert.False(TokenStore.IsValidToken(token));
    }

    [Theory]
    [InlineData("admin", "admin", true)]
    [InlineData("Admin", "admin", false)]
    [InlineData("admin", "wrong", false)]
    [InlineData("", "", false)]
    public void Login_ValidatesCredentials(string username, string password, bool shouldSucceed)
    {
        var credentials = new LoginRequest { username = username, password = password };

        var result = _controller.Login(credentials);

        if (shouldSucceed)
            Assert.IsType<OkObjectResult>(result);
        else
            Assert.IsType<UnauthorizedObjectResult>(result);
    }
}