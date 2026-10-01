using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using MCBA.Controllers;
using MCBA.Data;
using MCBA.ViewModels;
using MCBA_Tests.CustomerWebsite.Utility;
using Moq;

namespace MCBA_Tests.CustomerWebsite.Controllers;

public class DepositControllerTests
{
    private readonly MCBAContext _context;
    private readonly DepositController _controller;
    private readonly FakeSession _session;

    public DepositControllerTests()
    {
        _context = InMemoryContext.GetInMemoryContext();
        _controller = new DepositController(_context);

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
        var model = Assert.IsType<DepositViewModel>(viewResult.Model);

        Assert.NotNull(model.Accounts);
        Assert.Equal(2, model.Accounts.Count);
    }

    [Fact]
    public void Index_Post_ValidDeposit_RedirectsToConfirmAndSavesSession()
    {
        var viewModel = new DepositViewModel
        {
            AccountNumber = 4100,
            Amount = 100m,
            Comment = "Deposit Test"
        };

        var result = _controller.Index(viewModel);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Confirm", redirect.ActionName);

        Assert.Equal(4100, _session.GetInt32("DepositController_AccountNumber"));
        Assert.Equal("100", _session.GetString("DepositController_Amount"));
        Assert.Equal("Deposit Test", _session.GetString("DepositController_Comment"));
    }

    [Fact]
    public void Index_Post_InvalidModel_ReturnsView()
    {
        _controller.ModelState.AddModelError("Amount", "Required");

        var vm = new DepositViewModel
        {
            AccountNumber = 4100,
            Amount = 0m
        };

        var result = _controller.Index(vm);

        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<DepositViewModel>(viewResult.Model);

        Assert.NotNull(model.Accounts);
        Assert.Equal(2, model.Accounts.Count);
    }

    [Fact]
    public void Confirm_WithValidSession_ReturnsView()
    {
        _session.SetInt32("DepositController_AccountNumber", 4100);
        _session.SetString("DepositController_Amount", "100");

        var result = _controller.Confirm();

        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<DepositViewModel>(viewResult.Model);
        Assert.Equal(4100, model.AccountNumber);
        Assert.Equal(100m, model.Amount);
    }

    [Fact]
    public void Confirm_MissingSession_RedirectsToIndex()
    {
        var result = _controller.Confirm();

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
    }

    [Fact]
    public void Complete_ValidDeposit_UpdatesBalanceAndCreatesTransaction()
    {
        _session.SetInt32("DepositController_AccountNumber", 4100);
        _session.SetString("DepositController_Amount", "200");
        _session.SetString("DepositController_Comment", "Deposit OK");

        var result = _controller.Complete();

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
        Assert.Equal("Home", redirect.ControllerName);

        var account = _context.Accounts.Find(4100);
        Assert.Equal(1200m, account.Balance);

        var transaction = _context.Transactions.FirstOrDefault(t => t.AccountNumber == 4100);
        Assert.NotNull(transaction);
        Assert.Equal('D', transaction.TransactionType);
        Assert.Equal(200m, transaction.Amount);
    }

    [Fact]
    public void Cancel_ClearsSessionAndRedirectsHome()
    {
        _session.SetInt32("DepositController_AccountNumber", 4100);
        _session.SetString("DepositController_Amount", "100");
        _session.SetString("DepositController_Comment", "Test");

        var result = _controller.Cancel();

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
        Assert.Equal("Home", redirect.ControllerName);

        Assert.Null(_session.GetInt32("DepositController_AccountNumber"));
        Assert.Null(_session.GetString("DepositController_Amount"));
        Assert.Null(_session.GetString("DepositController_Comment"));
    }
}