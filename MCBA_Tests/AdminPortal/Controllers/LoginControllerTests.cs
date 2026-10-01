using MCBA_AdminPortal.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Moq;

namespace MCBA_Tests.AdminPortal.Controllers;

public class LoginControllerTests
{
    private readonly Mock<HttpClient> _mockHttpClient;
    private readonly Mock<IConfiguration> _mockConfig;
    private readonly Mock<IMemoryCache> _mockCache;
    private readonly LoginController _controller;

    public LoginControllerTests()
    {
        _mockHttpClient = new Mock<HttpClient>();
        _mockConfig = new Mock<IConfiguration>();
        _mockCache = new Mock<IMemoryCache>();
        
        _mockConfig.Setup(c => c["ConnectionStrings:AdminApiUrl"]).Returns("http://localhost");
        
        _controller = new LoginController(_mockHttpClient.Object, _mockConfig.Object, _mockCache.Object);
        
        _controller.TempData = new TempDataDictionary(
            new DefaultHttpContext(),
            Mock.Of<ITempDataProvider>()
        );
    }

    [Fact]
    public void Index_Get_ReturnsView()
    {
        var result = _controller.Index();

        Assert.IsType<ViewResult>(result);
    }

    [Fact]
    public async Task Logout_RemovesTokenFromCache_AndRedirectsToIndex()
    {
        object cachedValue = "test-token";
        _mockCache.Setup(c => c.TryGetValue("AdminToken", out cachedValue)).Returns(true);

        var result = await _controller.Logout();

        _mockCache.Verify(c => c.Remove("AdminToken"), Times.Once);
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirectResult.ActionName);
    }

    [Fact]
    public async Task Logout_RedirectsToIndex_WhenNoTokenInCache()
    {
        object cachedValue = null;
        _mockCache.Setup(c => c.TryGetValue("AdminToken", out cachedValue)).Returns(false);

        var result = await _controller.Logout();

        _mockCache.Verify(c => c.Remove("AdminToken"), Times.Once);
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirectResult.ActionName);
    }
}