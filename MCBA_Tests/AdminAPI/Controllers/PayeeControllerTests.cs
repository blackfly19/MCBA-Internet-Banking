using MCBA_AdminWebApi.Controllers;
using MCBA_AdminWebApi.Models;
using MCBA_AdminWebApi.Models.Repository;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace MCBA_Tests.AdminAPI.Controllers;

public class PayeeControllerTests
{
    private readonly Mock<IPayeeRepository> _mockRepository;
    private readonly PayeeController _controller;

    public PayeeControllerTests()
    {
        _mockRepository = new Mock<IPayeeRepository>();
        _controller = new PayeeController(_mockRepository.Object);
    }

    [Fact]
    public void GetAllPayees_ReturnsListOfPayees()
    {
        var payees = new List<Payee>
        {
            new Payee { PayeeID = 1, Name = "Telstra", Address = "123 Main St", City = "Melbourne", State = "VIC", Postcode = "3000", Phone = "(03) 1234 5678" },
            new Payee { PayeeID = 2, Name = "AGL", Address = "456 King St", City = "Sydney", State = "NSW", Postcode = "2000", Phone = "(02) 8765 4321" }
        };
        _mockRepository.Setup(r => r.GetAllPayees()).Returns(payees);

        var result = _controller.GetAllPayees();

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedPayees = Assert.IsType<List<Payee>>(okResult.Value);
        Assert.Equal(2, returnedPayees.Count);
    }

    [Fact]
    public void GetAllPayeesByPostcode_ReturnsFilteredPayees()
    {
        var payees = new List<Payee>
        {
            new Payee { PayeeID = 1, Name = "Telstra", Address = "123 Main St", City = "Melbourne", State = "VIC", Postcode = "3000", Phone = "(03) 1234 5678" }
        };
        _mockRepository.Setup(r => r.GetPayeesByPostcode("3000")).Returns(payees);

        var result = _controller.GetAllPayeesByPostcode("3000");

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedPayees = Assert.IsType<List<Payee>>(okResult.Value);
        Assert.Single(returnedPayees);
        Assert.Equal("3000", returnedPayees[0].Postcode);
    }

    [Fact]
    public void GetPayeeById_ReturnsPayee_WhenExists()
    {
        var payee = new Payee { PayeeID = 1, Name = "Telstra", Address = "123 Main St", City = "Melbourne", State = "VIC", Postcode = "3000", Phone = "(03) 1234 5678" };
        _mockRepository.Setup(r => r.GetPayeeById(1)).Returns(payee);

        var result = _controller.GetPayeeById(1);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedPayee = Assert.IsType<Payee>(okResult.Value);
        Assert.Equal(1, returnedPayee.PayeeID);
    }

    [Fact]
    public void GetPayeeById_ReturnsNotFound_WhenDoesNotExist()
    {
        _mockRepository.Setup(r => r.GetPayeeById(999)).Returns((Payee)null);

        var result = _controller.GetPayeeById(999);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public void UpdatePayee_ReturnsOk_WhenSuccessful()
    {
        var payee = new Payee { PayeeID = 1, Name = "Telstra Updated", Address = "123 Main St", City = "Melbourne", State = "VIC", Postcode = "3000", Phone = "(03) 1234 5678" };
        _mockRepository.Setup(r => r.UpdatePayee(payee)).Returns(payee);

        var result = _controller.UpdatePayee(1, payee);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var updatedPayee = Assert.IsType<Payee>(okResult.Value);
        Assert.Equal("Telstra Updated", updatedPayee.Name);
    }

    [Fact]
    public void UpdatePayee_ReturnsBadRequest_WhenIdMismatch()
    {
        var payee = new Payee { PayeeID = 1, Name = "Telstra", Address = "123 Main St", City = "Melbourne", State = "VIC", Postcode = "3000", Phone = "(03) 1234 5678" };

        var result = _controller.UpdatePayee(2, payee);

        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("ID mismatch", badRequestResult.Value);
    }

    [Fact]
    public void UpdatePayee_ReturnsServerError_WhenExceptionThrown()
    {
        var payee = new Payee { PayeeID = 1, Name = "Telstra", Address = "123 Main St", City = "Melbourne", State = "VIC", Postcode = "3000", Phone = "(03) 1234 5678" };
        _mockRepository.Setup(r => r.UpdatePayee(payee)).Throws(new Exception("Database error"));

        var result = _controller.UpdatePayee(1, payee);

        var statusCodeResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, statusCodeResult.StatusCode);
    }
}