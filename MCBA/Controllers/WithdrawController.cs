using Microsoft.AspNetCore.Mvc;
using MCBA.Data;
using MCBA.Filters;
using MCBA.Models;
using MCBA.ViewModels;

namespace MCBA.Controllers;

[Authorization]
public class WithdrawController(MCBAContext context) : BaseController
{
    private const string SessionKey_AccountNumber = $"{nameof(WithdrawController)}_AccountNumber";
    private const string SessionKey_Amount = $"{nameof(WithdrawController)}_Amount";
    private const string SessionKey_Comment = $"{nameof(WithdrawController)}_Comment";
    private const string SessionKey_ServiceCharge = $"{nameof(WithdrawController)}_ServiceCharge";

    public IActionResult Index()
    {
        return View(GetWithdrawViewModel(GetCustomerID()));
    }

    [HttpPost]
    public IActionResult Index(WithdrawViewModel viewModel)
    {
        var customerId = GetCustomerID();

        if (!ModelState.IsValid)
            return View(GetWithdrawViewModel(customerId));

        var account = context.Accounts.Find(viewModel.AccountNumber);
        if (account == null || account.CustomerID != customerId)
            return RedirectToAction("Index", "Home");

        var serviceFee = account.FreeTransactions > 0 ? 0 : 0.01m;
        var totalAmount = viewModel.Amount + serviceFee;

        if (account.AccountType == 'S' && totalAmount > account.Balance)
        {
            ModelState.AddModelError("", "Insufficient funds. Savings account cannot go below $0.");
            return View(GetWithdrawViewModel(customerId));
        }

        if (account.AccountType == 'C' && totalAmount > account.Balance + 500)
        {
            ModelState.AddModelError("", "Insufficient funds. Checking account cannot go below -$500.");
            return View(GetWithdrawViewModel(customerId));
        }

        HttpContext.Session.SetInt32(SessionKey_AccountNumber, viewModel.AccountNumber);
        HttpContext.Session.SetString(SessionKey_Amount, viewModel.Amount.ToString());
        HttpContext.Session.SetString(SessionKey_Comment, viewModel.Comment ?? "");
        HttpContext.Session.SetString(SessionKey_ServiceCharge, serviceFee.ToString());

        return RedirectToAction(nameof(Confirm));
    }

    public IActionResult Confirm()
    {
        var accountNumber = HttpContext.Session.GetInt32(SessionKey_AccountNumber);
        if (accountNumber == null || accountNumber == 0)
            return RedirectToAction(nameof(Index));

        var customerId = GetCustomerID();
        var viewModel = GetWithdrawViewModel(customerId);

        return View(viewModel);
    }

    [HttpPost]
    public IActionResult Complete()
    {
        var customerId = GetCustomerID();
        var viewModel = GetWithdrawViewModel(customerId);
        
        var account = context.Accounts.Find(viewModel.AccountNumber);
        if (account == null || account.CustomerID != customerId)
            return RedirectToAction("Index", "Home");

        var transaction = new Transaction
        {
            TransactionType = 'W',
            AccountNumber = viewModel.AccountNumber,
            Amount = viewModel.Amount,
            Comment = viewModel.Comment,
            TransactionTimeUtc = DateTime.UtcNow
        };

        account.Balance -= viewModel.TotalAmount;

        context.Transactions.Add(transaction);

        if (viewModel.ServiceCharge > 0)
        {
            var serviceChargeTransaction = new Transaction
            {
                TransactionType = 'S',
                AccountNumber = viewModel.AccountNumber,
                Amount = viewModel.ServiceCharge,
                TransactionTimeUtc = DateTime.UtcNow
            };
            context.Transactions.Add(serviceChargeTransaction);
        }
        else
        {
            account.FreeTransactions--;
        }

        context.SaveChanges();
        ClearSession();

        TempData["SuccessMessage"] = $"Successfully withdrew {viewModel.Amount:C} from account {viewModel.AccountNumber}";

        return RedirectToAction("Index", "Home");
    }

    public IActionResult Cancel()
    {
        ClearSession();
        return RedirectToAction("Index", "Home");
    }

    private WithdrawViewModel GetWithdrawViewModel(int? customerId)
    {
        var accountNumber = HttpContext.Session.GetInt32(SessionKey_AccountNumber) ?? 0;
        var amountStr = HttpContext.Session.GetString(SessionKey_Amount);
        var amount = string.IsNullOrEmpty(amountStr) ? 0 : decimal.Parse(amountStr);
        var comment = HttpContext.Session.GetString(SessionKey_Comment);
        var serviceChargeStr = HttpContext.Session.GetString(SessionKey_ServiceCharge);
        var serviceCharge = string.IsNullOrEmpty(serviceChargeStr) ? 0 : decimal.Parse(serviceChargeStr);

        var accountType = accountNumber > 0 
            ? (context.Accounts.Find(accountNumber)?.AccountType == 'C' ? "Checking" : "Savings")
            : null;

        return new WithdrawViewModel
        {
            AccountNumber = accountNumber,
            Amount = amount,
            Comment = comment,
            AccountType = accountType,
            ServiceCharge = serviceCharge,
            TotalAmount = amount + serviceCharge,
            Accounts = customerId.HasValue 
                ? context.Accounts.Where(a => a.CustomerID == customerId.Value).ToList() : []
        };
    }
}