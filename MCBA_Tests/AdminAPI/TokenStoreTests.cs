using MCBA_AdminWebApi.Services;
using Xunit;

namespace MCBA_Tests.AdminAPI;

public class TokenStoreTests
{
    [Fact]
    public void AddToken_MakesTokenValid()
    {
        var token = Guid.NewGuid().ToString();

        TokenStore.AddToken(token);

        Assert.True(TokenStore.IsValidToken(token));
        TokenStore.RemoveToken(token);
    }

    [Fact]
    public void IsValidToken_ReturnsFalseForNonExistentToken()
    {
        var token = Guid.NewGuid().ToString();

        var isValid = TokenStore.IsValidToken(token);

        Assert.False(isValid);
    }

    [Fact]
    public void RemoveToken_InvalidatesToken()
    {
        var token = Guid.NewGuid().ToString();
        TokenStore.AddToken(token);

        TokenStore.RemoveToken(token);

        Assert.False(TokenStore.IsValidToken(token));
    }

    [Fact]
    public void RemoveToken_OnlyRemovesSpecifiedToken()
    {
        var token1 = Guid.NewGuid().ToString();
        var token2 = Guid.NewGuid().ToString();
        TokenStore.AddToken(token1);
        TokenStore.AddToken(token2);

        TokenStore.RemoveToken(token1);

        Assert.False(TokenStore.IsValidToken(token1));
        Assert.True(TokenStore.IsValidToken(token2));
        TokenStore.RemoveToken(token2);
    }
}