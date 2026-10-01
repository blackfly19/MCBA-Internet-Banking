using MCBA_AdminWebApi.Filters;
using MCBA_AdminWebApi.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;

namespace MCBA_Tests.AdminAPI;

public class ApiKeyAuthAttributeTests
{
    private readonly ApiKeyAuthAttribute _filter;

    public ApiKeyAuthAttributeTests()
    {
        _filter = new ApiKeyAuthAttribute();
    }

    [Fact]
    public void OnAuthorization_ReturnsUnauthorized_WhenApiKeyHeaderIsMissing()
    {
        var context = CreateAuthorizationContext();

        _filter.OnAuthorization(context);

        var result = Assert.IsType<UnauthorizedObjectResult>(context.Result);
        Assert.Equal("API Key is missing", result.Value);
    }

    [Fact]
    public void OnAuthorization_ReturnsUnauthorized_WhenApiKeyIsInvalid()
    {
        var context = CreateAuthorizationContext();
        context.HttpContext.Request.Headers["X-API-Key"] = "invalid-token";

        _filter.OnAuthorization(context);

        var result = Assert.IsType<UnauthorizedObjectResult>(context.Result);
        Assert.Equal("Invalid API Key", result.Value);
    }

    [Fact]
    public void OnAuthorization_AllowsRequest_WhenApiKeyIsValid()
    {
        var token = Guid.NewGuid().ToString();
        TokenStore.AddToken(token);
        var context = CreateAuthorizationContext();
        context.HttpContext.Request.Headers["X-API-Key"] = token;

        _filter.OnAuthorization(context);

        Assert.Null(context.Result);
        TokenStore.RemoveToken(token);
    }

    private AuthorizationFilterContext CreateAuthorizationContext()
    {
        var httpContext = new DefaultHttpContext();
        var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
        return new AuthorizationFilterContext(actionContext, new List<IFilterMetadata>());
    }
}