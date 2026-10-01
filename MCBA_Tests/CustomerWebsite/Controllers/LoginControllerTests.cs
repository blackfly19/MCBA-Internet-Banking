using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using MCBA.Controllers;
using MCBA.Data;
using MCBA.Models;
using MCBA_Tests.CustomerWebsite.Utility;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace MCBA_Tests.CustomerWebsite.Controllers;

public class LoginControllerTests
{
    private readonly MCBAContext _context;
    private readonly LoginController _controller;
    private readonly FakeSession _session;

    public LoginControllerTests()
    {
        _context = InMemoryContext.GetInMemoryContext();
        _controller = new LoginController(_context);

        _session = new FakeSession();
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { Session = _session }
        };

        _controller.TempData = new TempDataDictionary(
            new DefaultHttpContext(),
            Mock.Of<ITempDataProvider>()
        );
    }

    [Fact]
    public void Index_Get_AlreadyLoggedIn_RedirectsToHome()
    {
        _session.SetInt32("CustomerID", 2100);

        var result = _controller.Index();

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
        Assert.Equal("Home", redirect.ControllerName);
    }

    [Fact]
    public void Index_Get_NotLoggedIn_ReturnsView()
    {
        var result = _controller.Index();

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Null(viewResult.Model);
    }

    [Fact]
    public void Index_Post_InvalidModel_ReturnsViewWithModel()
    {
        _controller.ModelState.AddModelError("LoginID", "Required");

        var model = new LoginViewModel { LoginID = "", Password = "" };
        var result = _controller.Index(model);

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(model, viewResult.Model);
    }

    [Fact]
    public void Index_Post_InvalidLogin_ReturnsViewWithError()
    {
        var model = new LoginViewModel { LoginID = "wrong", Password = "123" };

        var result = _controller.Index(model);

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(model, viewResult.Model);
        Assert.Contains(_controller.ModelState.Values, v => v.Errors.Count > 0);
    }

    [Fact]
    public void Index_Post_ValidLogin_SetsSessionAndRedirectsHome()
    {
        var login = _context.Logins.Include(l => l.Customer).First();
        var model = new LoginViewModel
        {
            LoginID = login.LoginID,
            Password = "abc123"
        };

        var result = _controller.Index(model);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
        Assert.Equal("Home", redirect.ControllerName);

        var customerId = _session.GetInt32("CustomerID");
        Assert.Equal(login.CustomerID, customerId);
    }

    [Fact]
    public void Logout_ClearsSessionAndRedirectsToIndex()
    {
        _session.SetInt32("CustomerID", 2100);

        var result = _controller.Logout();

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
        Assert.Null(_session.GetInt32("CustomerID"));
    }
}