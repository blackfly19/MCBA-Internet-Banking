using Microsoft.AspNetCore.Mvc;
using MCBA.Data;
using MCBA.Filters;
using X.PagedList.Extensions;

namespace MCBA.Controllers;

[Authorization]
public class StatementController(MCBAContext context) : BaseController
{
    public IActionResult Index(int? accountNumber)
    {
        var customerId = GetCustomerID();
        var accounts = context.Accounts.Where(a => a.CustomerID == customerId).ToList();

        ViewBag.Accounts = accounts;

        if (!accountNumber.HasValue)
        {
            return View();
        }

        var account = accounts.FirstOrDefault(a => a.AccountNumber == accountNumber.Value);
        if (account == null)
            return RedirectToAction(nameof(Index));

        ViewBag.Account = account;

        return RedirectToAction(nameof(ViewTransactions), new { accountNumber });
    }

    public IActionResult ViewTransactions(int accountNumber, int page = 1)
    {
        var customerId = GetCustomerID();
        var account = context.Accounts.FirstOrDefault(a => a.AccountNumber == accountNumber && a.CustomerID == customerId);
        
        if (account == null)
            return RedirectToAction(nameof(Index));

        ViewBag.Account = account;

        decimal availableBalance;
        if (account.AccountType == 'S')
        {
            availableBalance = account.Balance;
        }
        else 
        {
            availableBalance = account.Balance + 500;
        }
        ViewBag.AvailableBalance = availableBalance;

        // Page the transactions, maximum of 4 per page
        const int pageSize = 4;
        var pagedList = context.Transactions
            .Where(t => t.AccountNumber == accountNumber)
            .OrderByDescending(t => t.TransactionTimeUtc)
            .ToPagedList(page, pageSize);

        return View(pagedList);
    }
    
    public IActionResult Print(int accountNumber)
    {
        var customerId = GetCustomerID();
        var account = context.Accounts.FirstOrDefault(a => a.AccountNumber == accountNumber && a.CustomerID == customerId);
    
        if (account == null)
            return RedirectToAction(nameof(Index));

        decimal availableBalance;
        if (account.AccountType == 'S')
        {
            availableBalance = account.Balance;
        }
        else 
        {
            availableBalance = account.Balance + 500;
        }

        var transactions = context.Transactions
            .Where(t => t.AccountNumber == accountNumber)
            .OrderByDescending(t => t.TransactionTimeUtc)
            .ToList();

        ViewBag.Account = account;
        ViewBag.AvailableBalance = availableBalance;

        return View(transactions);
    }
}