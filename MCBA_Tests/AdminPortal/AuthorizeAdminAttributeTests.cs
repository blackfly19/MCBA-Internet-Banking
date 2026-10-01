using MCBA_AdminPortal.Filters;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Caching.Memory;
using Moq;

namespace MCBA_Tests.AdminPortal;

public class AuthorizeAdminAttributeTests
{
    [Fact]
    public void OnAuthorization_RedirectsToLogin_WhenTokenNotInCache()
    {
        var mockCache = new Mock<IMemoryCache>();
        object cachedValue = null;
        mockCache.Setup(c => c.TryGetValue("AdminToken", out cachedValue)).Returns(false);

        var filter = new AuthorizeAdminAttribute();
        var context = CreateAuthorizationContext(mockCache.Object);

        filter.OnAuthorization(context);

        var redirectResult = Assert.IsType<RedirectToActionResult>(context.Result);
        Assert.Equal("Index", redirectResult.ActionName);
        Assert.Equal("Login", redirectResult.ControllerName);
    }

    [Fact]
    public void OnAuthorization_RedirectsToLogin_WhenTokenIsEmpty()
    {
        var mockCache = new Mock<IMemoryCache>();
        object cachedValue = "";
        mockCache.Setup(c => c.TryGetValue("AdminToken", out cachedValue)).Returns(true);

        var filter = new AuthorizeAdminAttribute();
        var context = CreateAuthorizationContext(mockCache.Object);

        filter.OnAuthorization(context);

        var redirectResult = Assert.IsType<RedirectToActionResult>(context.Result);
        Assert.Equal("Index", redirectResult.ActionName);
        Assert.Equal("Login", redirectResult.ControllerName);
    }

    [Fact]
    public void OnAuthorization_AllowsRequest_WhenTokenExists()
    {
        var mockCache = new Mock<IMemoryCache>();
        object cachedValue = "valid-token";
        mockCache.Setup(c => c.TryGetValue("AdminToken", out cachedValue)).Returns(true);

        var filter = new AuthorizeAdminAttribute();
        var context = CreateAuthorizationContext(mockCache.Object);

        filter.OnAuthorization(context);

        Assert.Null(context.Result);
    }

    private AuthorizationFilterContext CreateAuthorizationContext(IMemoryCache cache)
    {
        var serviceProvider = new Mock<IServiceProvider>();
        serviceProvider.Setup(s => s.GetService(typeof(IMemoryCache))).Returns(cache);

        var httpContext = new DefaultHttpContext { RequestServices = serviceProvider.Object };
        var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
        return new AuthorizationFilterContext(actionContext, new List<IFilterMetadata>());
    }
}