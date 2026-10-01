using MCBA_AdminPortal.Controllers;
using MCBA_AdminPortal.Helper;
using MCBA_AdminPortal.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Moq;

namespace MCBA_Tests.AdminPortal.Controllers;

public class PayeeControllerTests
{
    private readonly Mock<IApiGeneric> _mockApi;
    private readonly PayeeController _controller;

    public PayeeControllerTests()
    {
        _mockApi = new Mock<IApiGeneric>();
        _controller = new PayeeController(_mockApi.Object);
        
        _controller.TempData = new TempDataDictionary(
            new DefaultHttpContext(),
            Mock.Of<ITempDataProvider>()
        );
    }

    [Fact]
    public void Index_ReturnsAllPayees_WhenPostcodeIsNull()
    {
        var payees = new List<Payee>
        {
            new Payee { PayeeID = 1, Name = "Telstra", Postcode = "3000" },
            new Payee { PayeeID = 2, Name = "AGL", Postcode = "2000" }
        };
        _mockApi.Setup(a => a.Get<List<Payee>>("Payee")).Returns(payees);

        var result = _controller.Index(null);

        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<List<Payee>>(viewResult.Model);
        Assert.Equal(2, model.Count);
    }

    [Fact]
    public void Index_ReturnsFilteredPayees_WhenPostcodeProvided()
    {
        var payees = new List<Payee>
        {
            new Payee { PayeeID = 1, Name = "Telstra", Postcode = "3000" }
        };
        _mockApi.Setup(a => a.Get<List<Payee>>("Payee/postcode/3000")).Returns(payees);

        var result = _controller.Index("3000");

        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<List<Payee>>(viewResult.Model);
        Assert.Single(model);
        Assert.Equal("3000", viewResult.ViewData["Postcode"]);
    }

    [Fact]
    public void Index_ReturnsEmptyList_WhenApiReturnsNull()
    {
        _mockApi.Setup(a => a.Get<List<Payee>>("Payee")).Returns((List<Payee>)null);

        var result = _controller.Index(null);

        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<List<Payee>>(viewResult.Model);
        Assert.Empty(model);
    }

    [Fact]
    public void Edit_Get_ReturnsViewWithPayee_WhenPayeeExists()
    {
        var payee = new Payee { PayeeID = 1, Name = "Telstra", Postcode = "3000" };
        _mockApi.Setup(a => a.Get<Payee>("Payee/1")).Returns(payee);

        var result = _controller.Edit(1);

        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<Payee>(viewResult.Model);
        Assert.Equal(1, model.PayeeID);
    }

    [Fact]
    public void Edit_Get_RedirectsToIndex_WhenPayeeNotFound()
    {
        _mockApi.Setup(a => a.Get<Payee>("Payee/999")).Returns((Payee)null);

        var result = _controller.Edit(999);

        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirectResult.ActionName);
    }

    [Fact]
    public void Edit_Post_RedirectsToIndex_WhenUpdateSucceeds()
    {
        var payee = new Payee { PayeeID = 1, Name = "Telstra Updated", Postcode = "3000" };
        _mockApi.Setup(a => a.Put("Payee/1", payee)).Returns(true);

        var result = _controller.Edit(payee);

        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirectResult.ActionName);
        Assert.Equal("Payee updated successfully", _controller.TempData["SuccessMessage"]);
    }

    [Fact]
    public void Edit_Post_ReturnsViewWithError_WhenUpdateFails()
    {
        var payee = new Payee { PayeeID = 1, Name = "Telstra", Postcode = "3000" };
        _mockApi.Setup(a => a.Put("Payee/1", payee)).Returns(false);

        var result = _controller.Edit(payee);

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.False(_controller.ModelState.IsValid);
        Assert.Contains("Failed to update payee", _controller.ModelState[""].Errors.Select(e => e.ErrorMessage));
    }
}