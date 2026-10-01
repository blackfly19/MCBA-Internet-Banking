using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MCBA.Controllers;
using MCBA.Data;
using MCBA.Models;
using MCBA_Tests.CustomerWebsite.Utility;

namespace MCBA_Tests.CustomerWebsite.Controllers;

public class StatementControllerTests
{
    private readonly MCBAContext _context;
    private readonly StatementController _controller;
    private readonly FakeSession _session;

    public StatementControllerTests()
    {
        _context = InMemoryContext.GetInMemoryContext();
        _controller = new StatementController(_context);

        _session = new FakeSession();
        _session.SetInt32("CustomerID", 2100);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { Session = _session }
        };
    }

    [Fact]
    public void Index_NoAccountNumber_ReturnsViewWithAccounts()
    {
        var result = _controller.Index(null);

        var viewResult = Assert.IsType<ViewResult>(result);
        var accounts = viewResult.ViewData["Accounts"] as System.Collections.Generic.List<Account>;
        Assert.NotNull(accounts);
        Assert.Equal(2, accounts.Count); // seeded accounts for customer 2100
    }

    [Fact]
    public void Index_WithValidAccountNumber_RedirectsToViewTransactions()
    {
        var accountNumber = 4100; // existing account

        var result = _controller.Index(accountNumber);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("ViewTransactions", redirect.ActionName);
        Assert.Equal(accountNumber, redirect.RouteValues["accountNumber"]);
    }

    [Fact]
    public void Index_WithInvalidAccountNumber_RedirectsBackToIndex()
    {
        var result = _controller.Index(9999);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
    }

    [Fact]
    public void ViewTransactions_ValidAccount_ReturnsViewWithPagedTransactions()
    {
        var accountNumber = 4100;

        // Add some transactions for this account
        for (int i = 1; i <= 6; i++)
        {
            _context.Transactions.Add(new Transaction
            {
                AccountNumber = accountNumber,
                TransactionType = 'D',
                Amount = i * 10,
                TransactionTimeUtc = System.DateTime.UtcNow.AddMinutes(-i)
            });
        }
        _context.SaveChanges();

        var result = _controller.ViewTransactions(accountNumber, page: 1);

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(_context.Accounts.Find(accountNumber), viewResult.ViewData["Account"]);

        var availableBalance = (decimal)viewResult.ViewData["AvailableBalance"];
        var account = _context.Accounts.Find(accountNumber);
        if (account.AccountType == 'S')
            Assert.Equal(account.Balance, availableBalance);
        else
            Assert.Equal(account.Balance + 500, availableBalance);

        // Check that only 4 transactions are returned for page 1
        dynamic model = viewResult.Model;
        Assert.Equal(4, model.Count);
    }

    [Fact]
    public void ViewTransactions_InvalidAccount_RedirectsToIndex()
    {
        var result = _controller.ViewTransactions(9999);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
    }
}