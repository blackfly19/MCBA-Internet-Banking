using MCBA.Filters;
using MCBA_Tests.CustomerWebsite.Utility;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;

namespace MCBA_Tests.CustomerWebsite;

public class AuthorizationAttributeTests
{
    private readonly AuthorizationAttribute _filter;
    private readonly FakeSession _session;

    public AuthorizationAttributeTests()
    {
        _filter = new AuthorizationAttribute();
        _session = new FakeSession();
    }

    [Fact]
    public void OnAuthorization_RedirectsToLogin_WhenSessionIsNull()
    {
        var context = CreateAuthorizationContext(_session);

        _filter.OnAuthorization(context);

        var redirectResult = Assert.IsType<RedirectToActionResult>(context.Result);
        Assert.Equal("Index", redirectResult.ActionName);
        Assert.Equal("Login", redirectResult.ControllerName);
    }

    [Fact]
    public void OnAuthorization_AllowsRequest_WhenSessionHasCustomerID()
    {
        _session.SetInt32("CustomerID", 2100);
        var context = CreateAuthorizationContext(_session);

        _filter.OnAuthorization(context);

        Assert.Null(context.Result);
    }

    [Theory]
    [InlineData(2100)]
    [InlineData(2200)]
    [InlineData(2300)]
    public void OnAuthorization_AllowsRequest_ForDifferentCustomerIDs(int customerID)
    {
        _session.SetInt32("CustomerID", customerID);
        var context = CreateAuthorizationContext(_session);

        _filter.OnAuthorization(context);

        Assert.Null(context.Result);
    }

    private AuthorizationFilterContext CreateAuthorizationContext(ISession session)
    {
        var httpContext = new DefaultHttpContext { Session = session };
        var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
        return new AuthorizationFilterContext(actionContext, new List<IFilterMetadata>());
    }
}