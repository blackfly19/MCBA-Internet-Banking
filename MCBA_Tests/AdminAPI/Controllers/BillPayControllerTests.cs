using MCBA_AdminWebApi.Controllers;
using MCBA_AdminWebApi.Models;
using MCBA_AdminWebApi.Models.Repository;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace MCBA_Tests.AdminAPI.Controllers;

public class BillPayControllerTests
{
    private readonly Mock<IBillPayRepository> _mockRepository;
    private readonly BillPayController _controller;

    public BillPayControllerTests()
    {
        _mockRepository = new Mock<IBillPayRepository>();
        _controller = new BillPayController(_mockRepository.Object);
    }

    [Fact]
    public void GetBillPay_ReturnsNotFound_WhenDoesNotExist()
    {
        _mockRepository.Setup(r => r.GetBillPayById(999)).Returns((BillPay)null);

        var result = _controller.GetBillPay(999);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public void BlockBillPay_ReturnsNotFound_WhenDoesNotExist()
    {
        _mockRepository.Setup(r => r.BlockBillPay(999)).Throws<KeyNotFoundException>();

        var result = _controller.BlockBillPay(999);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public void UnblockBillPay_ReturnsNotFound_WhenDoesNotExist()
    {
        _mockRepository.Setup(r => r.UnblockBillPay(999)).Throws<KeyNotFoundException>();

        var result = _controller.UnblockBillPay(999);

        Assert.IsType<NotFoundResult>(result);
    }
}