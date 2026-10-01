using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using MCBA.Controllers;
using MCBA.Data;
using MCBA.Models;
using MCBA_Tests.CustomerWebsite.Utility;
using Moq;

namespace MCBA_Tests.CustomerWebsite.Controllers;

public class HomeControllerTests
{
    private readonly MCBAContext _context;
    private readonly HomeController _controller;
    private readonly FakeSession _session;

    public HomeControllerTests()
    {
        _context = InMemoryContext.GetInMemoryContext();
        _controller = new HomeController(_context);

        _session = new FakeSession();
        _session.SetInt32("CustomerID", 2100);

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
    public void Index_ValidCustomer_ReturnsViewWithCustomer()
    {
        var result = _controller.Index();

        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<Customer>(viewResult.Model);
        Assert.Equal(2100, model.CustomerID);
        Assert.NotEmpty(model.Accounts);
    }

    [Fact]
    public void Index_InvalidCustomer_RedirectsToLogin()
    {
        _session.SetInt32("CustomerID", 9999); // non-existent customer

        var result = _controller.Index();

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
        Assert.Equal("Login", redirect.ControllerName);
    }

    [Fact]
    public void Privacy_ReturnsView()
    {
        var result = _controller.Privacy();

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Null(viewResult.Model);
    }
}