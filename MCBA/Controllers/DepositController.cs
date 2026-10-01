using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using MCBA.Data;
using MCBA.Filters;
using MCBA.Models;
using MCBA.ViewModels;

namespace MCBA.Controllers;

[Authorization]
public class DepositController(MCBAContext context) : BaseController
{
    private const string SessionKey_AccountNumber = $"{nameof(DepositController)}_AccountNumber";
    private const string SessionKey_Amount = $"{nameof(DepositController)}_Amount";
    private const string SessionKey_Comment = $"{nameof(DepositController)}_Comment";

    public IActionResult Index()
    {
        return View(GetDepositViewModel(GetCustomerID()));
    }

    [HttpPost]
    public IActionResult Index(DepositViewModel viewModel)
    {
        var customerId = GetCustomerID();

        if (!ModelState.IsValid)        
            return View(GetDepositViewModel(customerId));

        var account = context.Accounts.Find(viewModel.AccountNumber);
        if (account == null || account.CustomerID != customerId)
        {
            return RedirectToAction("Index", "Home");
        }

        HttpContext.Session.SetInt32(SessionKey_AccountNumber, viewModel.AccountNumber);
        HttpContext.Session.SetString(SessionKey_Amount, viewModel.Amount.ToString());
        HttpContext.Session.SetString(SessionKey_Comment, viewModel.Comment ?? "");

        return RedirectToAction(nameof(Confirm));
    }

    public IActionResult Confirm()
    {
        var accountNumber = HttpContext.Session.GetInt32(SessionKey_AccountNumber);
        if (accountNumber is null or 0)
        {
            return RedirectToAction("Index");
        }
        
        return View(GetDepositViewModel(GetCustomerID()));
    }

    [HttpPost]
    public IActionResult Complete()
    {
        var customerId = GetCustomerID();
        var viewModel = GetDepositViewModel(customerId);
        
        var account = context.Accounts.Find(viewModel.AccountNumber);
        if (account == null || account.CustomerID != customerId)
        {
            return RedirectToAction("Index", "Home");
        }

        var transaction = new Transaction
        {
            TransactionType = 'D',
            AccountNumber = viewModel.AccountNumber,
            Amount = viewModel.Amount,
            Comment = viewModel.Comment,
            TransactionTimeUtc = DateTime.UtcNow
        };

        account.Balance += viewModel.Amount;

        context.Transactions.Add(transaction);
        context.SaveChanges();
        
        ClearSession();

        TempData["SuccessMessage"] = $"Successfully deposited {viewModel.Amount:C} to account {viewModel.AccountNumber}";

        return RedirectToAction("Index", "Home");
    }

    public IActionResult Cancel()
    {
        ClearSession();
        return RedirectToAction("Index", "Home");
    }

    private DepositViewModel GetDepositViewModel(int? customerId)
    {
        var accountNumber = HttpContext.Session.GetInt32(SessionKey_AccountNumber) ?? 0;
        var amountStr = HttpContext.Session.GetString(SessionKey_Amount);
        var amount = string.IsNullOrEmpty(amountStr) ? 0 : decimal.Parse(amountStr);
        var comment = HttpContext.Session.GetString(SessionKey_Comment);
        var accountType = accountNumber > 0 
            ? context.Accounts.Find(accountNumber)?.AccountType.ToString()
            : null;

        return new DepositViewModel
        {
            AccountNumber = accountNumber,
            Amount = amount,
            Comment = comment,
            AccountType = accountType,
            Accounts = customerId.HasValue ? context.Accounts.
                Where(a => a.CustomerID == customerId).ToList() : []
        };
    }
}