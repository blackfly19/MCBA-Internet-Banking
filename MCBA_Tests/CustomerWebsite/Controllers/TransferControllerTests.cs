using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MCBA.Controllers;
using MCBA.Data;
using MCBA.ViewModels;
using MCBA_Tests.CustomerWebsite.Utility;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;

namespace MCBA_Tests.CustomerWebsite.Controllers;

public class TransferControllerTests
{
    private readonly MCBAContext _context;
    private readonly TransferController _controller;
    private readonly FakeSession _session;

    public TransferControllerTests()
    {
        _context = InMemoryContext.GetInMemoryContext();

        _controller = new TransferController(_context);

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
    public void Index_Get_ReturnsViewWithAccounts()
    {
        var result = _controller.Index();

        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<TransferViewModel>(viewResult.Model);

        Assert.NotNull(model.Accounts);
        Assert.Equal(2, model.Accounts.Count);
    }

    [Fact]
    public void Index_Post_ValidTransfer_RedirectsToConfirm()
    {
        var vm = new TransferViewModel
        {
            SourceAccountNumber = 4100,
            DestinationAccountNumber = 4101,
            Amount = 100m,
            Comment = "Test transfer"
        };

        var result = _controller.Index(vm);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Confirm", redirect.ActionName);

        Assert.Equal(4100, _session.GetInt32("TransferController_SourceAccountNumber"));
        Assert.Equal(4101, _session.GetInt32("TransferController_DestinationAccountNumber"));
        Assert.Equal("100", _session.GetString("TransferController_Amount"));
    }

    [Fact]
    public void Complete_ValidTransfer_UpdatesBalances()
    {
        _session.SetInt32("TransferController_SourceAccountNumber", 4100);
        _session.SetInt32("TransferController_DestinationAccountNumber", 4101);
        _session.SetString("TransferController_Amount", "100");
        _session.SetString("TransferController_Comment", "Test");

        var result = _controller.Complete();
        var redirect = Assert.IsType<RedirectToActionResult>(result);

        var source = _context.Accounts.Find(4100);
        var destination = _context.Accounts.Find(4101);

        Assert.Equal(900m, source.Balance);
        Assert.Equal(600m, destination.Balance);
    }

    [Fact]
    public void Cancel_ClearsSessionAndRedirectsToHome()
    {
        _session.SetInt32("TransferController_SourceAccountNumber", 4100);
        _session.SetInt32("TransferController_DestinationAccountNumber", 4101);
        _session.SetString("TransferController_Amount", "100");

        var result = _controller.Cancel();

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
        Assert.Equal("Home", redirect.ControllerName);

        Assert.Null(_session.GetInt32("TransferController_SourceAccountNumber"));
        Assert.Null(_session.GetInt32("TransferController_DestinationAccountNumber"));
        Assert.Null(_session.GetString("TransferController_Amount"));
    }
    
    [Fact]
    public void Index_Post_InvalidModel_ReturnsView()
    {
        _controller.ModelState.AddModelError("Amount", "Required");

        var vm = new TransferViewModel
        {
            SourceAccountNumber = 4100,
            DestinationAccountNumber = 4101,
            Amount = 0
        };

        var result = _controller.Index(vm);

        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<TransferViewModel>(viewResult.Model);
        Assert.NotNull(model.Accounts);
    }

    [Fact]
    public void Index_Post_SameSourceAndDestination_ReturnsViewWithError()
    {
        var vm = new TransferViewModel
        {
            SourceAccountNumber = 4100,
            DestinationAccountNumber = 4100,
            Amount = 50
        };

        var result = _controller.Index(vm);

        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<TransferViewModel>(viewResult.Model);
        Assert.NotNull(model.Accounts);
    }

    [Fact]
    public void Index_Post_InsufficientFunds_ReturnsViewWithError()
    {
        var source = _context.Accounts.Find(4101);
        source.Balance = 10m;
        source.FreeTransactions = 0;
        _context.SaveChanges();

        var vm = new TransferViewModel
        {
            SourceAccountNumber = 4101,
            DestinationAccountNumber = 4100,
            Amount = 50
        };

        var result = _controller.Index(vm);

        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<TransferViewModel>(viewResult.Model);
        Assert.NotNull(model.Accounts);
    }

    [Fact]
    public void Index_Post_InvalidAccount_RedirectsHome()
    {
        var vm = new TransferViewModel
        {
            SourceAccountNumber = 9999, // non-existent
            DestinationAccountNumber = 4101,
            Amount = 50
        };

        var result = _controller.Index(vm);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
        Assert.Equal("Home", redirect.ControllerName);
    }

    [Fact]
    public void Complete_WithServiceCharge_CreatesServiceChargeTransaction()
    {
        var source = _context.Accounts.Find(4100);
        source.FreeTransactions = 0;
        _context.SaveChanges();

        _session.SetInt32("TransferController_SourceAccountNumber", 4100);
        _session.SetInt32("TransferController_DestinationAccountNumber", 4101);
        _session.SetString("TransferController_Amount", "100");
        _session.SetString("TransferController_Comment", "Service charge test");
        _session.SetString("TransferController_ServiceCharge", "0.05");

        var result = _controller.Complete();

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);

        var serviceChargeTx = _context.Transactions.FirstOrDefault(t => t.TransactionType == 'S' && t.AccountNumber == 4100);
        Assert.NotNull(serviceChargeTx);
        Assert.Equal(0.05m, serviceChargeTx.Amount);

        var sourceAccount = _context.Accounts.Find(4100);
        var destAccount = _context.Accounts.Find(4101);
        Assert.Equal(899.95m, sourceAccount.Balance); // 1000 - 100 - 0.05 handled in controller
        Assert.Equal(600m, destAccount.Balance);
    }

    [Fact]
    public void Confirm_MissingSession_RedirectsToIndex()
    {
        var result = _controller.Confirm();

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
    }

    [Fact]
    public void Confirm_WithValidSession_ReturnsView()
    {
        _session.SetInt32("TransferController_SourceAccountNumber", 4100);
        _session.SetInt32("TransferController_DestinationAccountNumber", 4101);
        _session.SetString("TransferController_Amount", "50");

        var result = _controller.Confirm();

        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<TransferViewModel>(viewResult.Model);
        Assert.Equal(4100, model.SourceAccountNumber);
        Assert.Equal(4101, model.DestinationAccountNumber);
        Assert.Equal(50m, model.Amount);
    }
}