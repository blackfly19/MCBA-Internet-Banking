using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using MCBA.Controllers;
using MCBA.Data;
using MCBA.ViewModels;
using MCBA_Tests.CustomerWebsite.Utility;
using Moq;

namespace MCBA_Tests.CustomerWebsite.Controllers;

public class WithdrawControllerTests
{
    private readonly MCBAContext _context;
    private readonly WithdrawController _controller;
    private readonly FakeSession _session;

    public WithdrawControllerTests()
    {
        _context = InMemoryContext.GetInMemoryContext();
        _controller = new WithdrawController(_context);

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
        var model = Assert.IsType<WithdrawViewModel>(viewResult.Model);

        Assert.NotNull(model.Accounts);
        Assert.Equal(2, model.Accounts.Count); // matches seeded accounts in InMemoryContext
    }

    [Fact]
    public void Index_Post_ValidModel_SetsSessionAndRedirectsToConfirm()
    {
        var vm = new WithdrawViewModel
        {
            AccountNumber = 4100,
            Amount = 100,
            Comment = "Test withdrawal"
        };

        var result = _controller.Index(vm);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Confirm", redirect.ActionName);
        Assert.Equal(4100, _session.GetInt32("WithdrawController_AccountNumber"));
        Assert.Equal("100", _session.GetString("WithdrawController_Amount"));
        Assert.Equal("Test withdrawal", _session.GetString("WithdrawController_Comment"));
    }

    [Fact]
    public void Index_Post_InvalidModel_ReturnsView()
    {
        _controller.ModelState.AddModelError("Amount", "Required");
        var vm = new WithdrawViewModel();

        var result = _controller.Index(vm);

        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<WithdrawViewModel>(viewResult.Model);
        Assert.NotNull(model.Accounts);
    }

    [Fact]
    public void Index_Post_SavingsAccount_InsufficientFunds_ReturnsViewWithError()
    {
        var savings = _context.Accounts.First(a => a.AccountType == 'S');
        savings.Balance = 50m;
        _context.SaveChanges();

        var vm = new WithdrawViewModel
        {
            AccountNumber = savings.AccountNumber,
            Amount = 100
        };

        var result = _controller.Index(vm);

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Contains(_controller.ModelState.Values, v => v.Errors.Count > 0);
    }

    [Fact]
    public void Index_Post_CheckingAccount_OverdraftExceeded_ReturnsViewWithError()
    {
        var checking = _context.Accounts.First(a => a.AccountType == 'C');
        checking.Balance = 0m;
        _context.SaveChanges();

        var vm = new WithdrawViewModel
        {
            AccountNumber = checking.AccountNumber,
            Amount = 1000m
        };

        var result = _controller.Index(vm);

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Contains(_controller.ModelState.Values, v => v.Errors.Count > 0);
    }

    [Fact]
    public void Confirm_WithSession_ReturnsView()
    {
        _session.SetInt32("WithdrawController_AccountNumber", 4100);
        _session.SetString("WithdrawController_Amount", "50");

        var result = _controller.Confirm();

        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<WithdrawViewModel>(viewResult.Model);
        Assert.Equal(4100, model.AccountNumber);
        Assert.Equal(50m, model.Amount);
    }

    [Fact]
    public void Confirm_NoSession_RedirectsToIndex()
    {
        var result = _controller.Confirm();

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
    }

    [Fact]
    public void Complete_ValidWithdrawal_UpdatesBalanceAndCreatesTransaction()
    {
        _session.SetInt32("WithdrawController_AccountNumber", 4100);
        _session.SetString("WithdrawController_Amount", "100");
        _session.SetString("WithdrawController_Comment", "ATM withdrawal");

        var result = _controller.Complete();

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
        Assert.Equal("Home", redirect.ControllerName);

        var account = _context.Accounts.Find(4100);
        Assert.Equal(900m, account.Balance); // 1000 - 100

        var transaction = _context.Transactions.FirstOrDefault(t => t.AccountNumber == 4100);
        Assert.NotNull(transaction);
        Assert.Equal('W', transaction.TransactionType);
        Assert.Equal(100m, transaction.Amount);
    }

    [Fact]
    public void Complete_WithdrawalWithServiceCharge_DecrementsFreeTransactions()
    {
        var checking = _context.Accounts.First(a => a.AccountType == 'C');
        checking.FreeTransactions = 1;
        _context.SaveChanges();

        _session.SetInt32("WithdrawController_AccountNumber", checking.AccountNumber);
        _session.SetString("WithdrawController_Amount", "100");
        _session.SetString("WithdrawController_Comment", "Test service charge");

        var result = _controller.Complete();

        var updated = _context.Accounts.Find(checking.AccountNumber);
        Assert.Equal(0, updated.FreeTransactions);
    }

    [Fact]
    public void Cancel_ClearsSessionAndRedirectsHome()
    {
        _session.SetInt32("WithdrawController_AccountNumber", 4100);
        _session.SetString("WithdrawController_Amount", "50");

        var result = _controller.Cancel();

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
        Assert.Equal("Home", redirect.ControllerName);
        Assert.Null(_session.GetInt32("WithdrawController_AccountNumber"));
        Assert.Null(_session.GetString("WithdrawController_Amount"));
    }
}