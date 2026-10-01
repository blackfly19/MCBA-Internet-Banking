using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using MCBA.Controllers;
using MCBA.Data;
using MCBA.Models;
using MCBA.ViewModels;
using MCBA_Tests.CustomerWebsite.Utility;
using Moq;

namespace MCBA_Tests.CustomerWebsite.Controllers;

public class BillPayControllerTests
{
    private readonly MCBAContext _context;
    private readonly BillPayController _controller;
    private readonly FakeSession _session;

    public BillPayControllerTests()
    {
        _context = InMemoryContext.GetInMemoryContext();
        _controller = new BillPayController(_context);

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
    public void Index_ReturnsViewWithScheduledBills()
    {
        var result = _controller.Index();

        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<BillPayListViewModel>(viewResult.Model);

        Assert.NotNull(model.Accounts);
        Assert.NotNull(model.ScheduledBills);
    }

    [Fact]
    public void Create_Get_ReturnsViewWithAccountsAndPayees()
    {
        var result = _controller.Create();

        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<BillPayViewModel>(viewResult.Model);

        Assert.NotEmpty(model.Accounts);
        Assert.NotEmpty(model.Payees);
    }

    [Fact]
    public void Create_Post_InvalidModel_ReturnsViewWithLists()
    {
        _controller.ModelState.AddModelError("Amount", "Required");

        var vm = new BillPayViewModel();

        var result = _controller.Create(vm);

        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<BillPayViewModel>(viewResult.Model);

        Assert.NotEmpty(model.Accounts);
        Assert.NotEmpty(model.Payees);
    }

    [Fact]
    public void Create_Post_InvalidAccount_RedirectsToIndex()
    {
        var vm = new BillPayViewModel
        {
            AccountNumber = 9999,
            PayeeID = 1,
            Amount = 100,
            Period = 'M',
            ScheduleTimeUtc = DateTime.Now
        };

        var result = _controller.Create(vm);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
    }

    [Fact]
    public void Create_Post_InvalidPayee_ReturnsViewWithError()
    {
        var account = _context.Accounts.First();
        var vm = new BillPayViewModel
        {
            AccountNumber = account.AccountNumber,
            PayeeID = 9999,
            Amount = 100,
            Period = 'M',
            ScheduleTimeUtc = DateTime.Now
        };

        var result = _controller.Create(vm);

        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<BillPayViewModel>(viewResult.Model);
        Assert.Contains(_controller.ModelState.Values, v => v.Errors.Count > 0);
        Assert.NotEmpty(model.Accounts);
        Assert.NotEmpty(model.Payees);
    }

    [Fact]
    public void Create_Post_ValidBillPay_AddsAndRedirects()
    {
        var account = _context.Accounts.First();
        var payee = _context.Payees.First();

        var vm = new BillPayViewModel
        {
            AccountNumber = account.AccountNumber,
            PayeeID = payee.PayeeID,
            Amount = 200,
            Period = 'M',
            ScheduleTimeUtc = DateTime.Now
        };

        var result = _controller.Create(vm);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);

        var bill = _context.BillPays.FirstOrDefault(b => b.AccountNumber == account.AccountNumber && b.PayeeID == payee.PayeeID);
        Assert.NotNull(bill);
    }

    [Fact]
    public void Cancel_BillDoesNotExist_RedirectsToIndex()
    {
        var result = _controller.Cancel(9999);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
    }

    [Fact]
    public void Cancel_BillBelongsToOtherCustomer_RedirectsToIndex()
    {
        var otherAccount = _context.Accounts.First();
        var bill = new BillPay
        {
            AccountNumber = otherAccount.AccountNumber,
            PayeeID = 1,
            Amount = 50,
            ScheduleTimeUtc = DateTime.Now,
            Period = 'M'
        };
        _context.BillPays.Add(bill);
        _context.SaveChanges();

        // Change session to different customer
        _session.SetInt32("CustomerID", 9999);

        var result = _controller.Cancel(bill.BillPayID);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
    }

    [Fact]
    public void Cancel_ValidBill_RemovesAndRedirects()
    {
        var account = _context.Accounts.First();
        var bill = new BillPay
        {
            AccountNumber = account.AccountNumber,
            PayeeID = 1,
            Amount = 50,
            ScheduleTimeUtc = DateTime.Now,
            Period = 'M'
        };
        _context.BillPays.Add(bill);
        _context.SaveChanges();

        var result = _controller.Cancel(bill.BillPayID);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);

        var deletedBill = _context.BillPays.Find(bill.BillPayID);
        Assert.Null(deletedBill);
    }
}