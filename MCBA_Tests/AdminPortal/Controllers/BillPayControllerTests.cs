using MCBA_AdminPortal.Controllers;
using MCBA_AdminPortal.Helper;
using MCBA_AdminPortal.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;

namespace MCBA_Tests.AdminPortal.Controllers;

public class BillPayControllerTests
{
    private readonly Mock<IApiGeneric> _mockApi;
    private readonly BillPayController _controller;

    public BillPayControllerTests()
    {
        _mockApi = new Mock<IApiGeneric>();
        _controller = new BillPayController(_mockApi.Object);
        
        _controller.TempData = new TempDataDictionary(
            new DefaultHttpContext(),
            Mock.Of<ITempDataProvider>()
        );
    }

    [Fact]
    public void Index_ReturnsViewWithBillPays()
    {
        var billPays = new List<BillPay>
        {
            new BillPay { BillPayID = 1, AccountNumber = 4100, Amount = 100m },
            new BillPay { BillPayID = 2, AccountNumber = 4101, Amount = 200m }
        };
        _mockApi.Setup(a => a.Get<List<BillPay>>("BillPay")).Returns(billPays);

        var result = _controller.Index();

        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<List<BillPay>>(viewResult.Model);
        Assert.Equal(2, model.Count);
    }

    [Fact]
    public void Index_ReturnsEmptyList_WhenApiReturnsNull()
    {
        _mockApi.Setup(a => a.Get<List<BillPay>>("BillPay")).Returns((List<BillPay>)null);

        var result = _controller.Index();

        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<List<BillPay>>(viewResult.Model);
        Assert.Empty(model);
    }

    [Fact]
    public void Block_SetsSuccessMessage_WhenApiSucceeds()
    {
        _mockApi.Setup(a => a.Post("BillPay/block/1")).Returns(true);

        var result = _controller.Block(1);

        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirectResult.ActionName);
        Assert.Equal("Bill payment blocked successfully", _controller.TempData["SuccessMessage"]);
    }

    [Fact]
    public void Block_SetsErrorMessage_WhenApiFails()
    {
        _mockApi.Setup(a => a.Post("BillPay/block/1")).Returns(false);

        var result = _controller.Block(1);

        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirectResult.ActionName);
        Assert.Equal("Failed to block bill payment", _controller.TempData["ErrorMessage"]);
    }

    [Fact]
    public void Unblock_SetsSuccessMessage_WhenApiSucceeds()
    {
        _mockApi.Setup(a => a.Post("BillPay/unblock/1")).Returns(true);

        var result = _controller.Unblock(1);

        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirectResult.ActionName);
        Assert.Equal("Bill payment unblocked successfully", _controller.TempData["SuccessMessage"]);
    }

    [Fact]
    public void Unblock_SetsErrorMessage_WhenApiFails()
    {
        _mockApi.Setup(a => a.Post("BillPay/unblock/1")).Returns(false);

        var result = _controller.Unblock(1);

        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirectResult.ActionName);
        Assert.Equal("Failed to unblock bill payment", _controller.TempData["ErrorMessage"]);
    }
}