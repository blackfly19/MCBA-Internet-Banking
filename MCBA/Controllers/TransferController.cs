using Microsoft.AspNetCore.Mvc;
using MCBA.Data;
using MCBA.Filters;
using MCBA.Models;
using MCBA.ViewModels;
using Microsoft.AspNetCore.Authorization;

namespace MCBA.Controllers;

[Authorization]
public class TransferController(MCBAContext context) : BaseController
{
    private const string SessionKey_SourceAccountNumber = $"{nameof(TransferController)}_SourceAccountNumber";
    private const string SessionKey_DestinationAccountNumber = $"{nameof(TransferController)}_DestinationAccountNumber";
    private const string SessionKey_Amount = $"{nameof(TransferController)}_Amount";
    private const string SessionKey_Comment = $"{nameof(TransferController)}_Comment";
    private const string SessionKey_ServiceCharge = $"{nameof(TransferController)}_ServiceCharge";

    public IActionResult Index()
    {
        return View(GetTransferViewModel(GetCustomerID()));
    }

    [HttpPost]
    public IActionResult Index(TransferViewModel viewModel)
    {
        var customerId = GetCustomerID();

        if (!ModelState.IsValid)
            return View(GetTransferViewModel(customerId));

        if (viewModel.SourceAccountNumber == viewModel.DestinationAccountNumber)
        {
            ModelState.AddModelError("", "Source and destination accounts must be different");
            return View(GetTransferViewModel(customerId));
        }

        var sourceAccount = context.Accounts.Find(viewModel.SourceAccountNumber);
        if (sourceAccount == null || sourceAccount.CustomerID != customerId)
            return RedirectToAction("Index", "Home");

        var destinationAccount = context.Accounts.Find(viewModel.DestinationAccountNumber);
        if (destinationAccount == null)
        {
            ModelState.AddModelError("", "Destination account does not exist");
            return View(GetTransferViewModel(customerId));
        }

        var serviceFee = sourceAccount.FreeTransactions > 0 ? 0 : 0.05m;
        var totalAmount = viewModel.Amount + serviceFee;

        if (sourceAccount.AccountType == 'S' && totalAmount > sourceAccount.Balance)
        {
            ModelState.AddModelError("", "Insufficient funds. Savings account cannot go below $0.");
            return View(GetTransferViewModel(customerId));
        }

        if (sourceAccount.AccountType == 'C' && totalAmount > sourceAccount.Balance + 500)
        {
            ModelState.AddModelError("", "Insufficient funds. Checking account cannot go below -$500.");
            return View(GetTransferViewModel(customerId));
        }

        HttpContext.Session.SetInt32(SessionKey_SourceAccountNumber, viewModel.SourceAccountNumber);
        HttpContext.Session.SetInt32(SessionKey_DestinationAccountNumber, viewModel.DestinationAccountNumber);
        HttpContext.Session.SetString(SessionKey_Amount, viewModel.Amount.ToString());
        HttpContext.Session.SetString(SessionKey_Comment, viewModel.Comment ?? "");
        HttpContext.Session.SetString(SessionKey_ServiceCharge, serviceFee.ToString());

        return RedirectToAction(nameof(Confirm));
    }

    public IActionResult Confirm()
    {
        var sourceAccountNumber = HttpContext.Session.GetInt32(SessionKey_SourceAccountNumber);
        if (sourceAccountNumber == null || sourceAccountNumber == 0)
            return RedirectToAction(nameof(Index));

        var customerId = GetCustomerID();
        var viewModel = GetTransferViewModel(customerId);

        return View(viewModel);
    }

    [HttpPost]
    public IActionResult Complete()
    {
        var customerId = GetCustomerID();
        var viewModel = GetTransferViewModel(customerId);
        
        var sourceAccount = context.Accounts.Find(viewModel.SourceAccountNumber);
        if (sourceAccount == null || sourceAccount.CustomerID != customerId)
            return RedirectToAction("Index", "Home");

        var destinationAccount = context.Accounts.Find(viewModel.DestinationAccountNumber);
        if (destinationAccount == null)
            return RedirectToAction("Index", "Home");

        var outgoingTransaction = new Transaction
        {
            TransactionType = 'T',
            AccountNumber = viewModel.SourceAccountNumber,
            DestinationAccountNumber = viewModel.DestinationAccountNumber,
            Amount = viewModel.Amount,
            Comment = viewModel.Comment,
            TransactionTimeUtc = DateTime.UtcNow
        };

        var incomingTransaction = new Transaction
        {
            TransactionType = 'T',
            AccountNumber = viewModel.DestinationAccountNumber,
            DestinationAccountNumber = null,
            Amount = viewModel.Amount,
            Comment = viewModel.Comment,
            TransactionTimeUtc = DateTime.UtcNow
        };

        // Update balances
        sourceAccount.Balance -= viewModel.TotalAmount;
        destinationAccount.Balance += viewModel.Amount;

        context.Transactions.Add(outgoingTransaction);
        context.Transactions.Add(incomingTransaction);

        // Handle service charge
        if (viewModel.ServiceCharge > 0)
        {
            var serviceChargeTransaction = new Transaction
            {
                TransactionType = 'S',
                AccountNumber = viewModel.SourceAccountNumber,
                Amount = viewModel.ServiceCharge,
                TransactionTimeUtc = DateTime.UtcNow
            };
            context.Transactions.Add(serviceChargeTransaction);
        }
        else
        {
            sourceAccount.FreeTransactions--;
        }

        context.SaveChanges();
        ClearSession();

        TempData["SuccessMessage"] = $"Successfully transferred {viewModel.Amount:C} from account {viewModel.SourceAccountNumber} to {viewModel.DestinationAccountNumber}";

        return RedirectToAction("Index", "Home");
    }

    public IActionResult Cancel()
    {
        ClearSession();
        return RedirectToAction("Index", "Home");
    }

    private TransferViewModel GetTransferViewModel(int? customerId)
    {
        var sourceAccountNumber = HttpContext.Session.GetInt32(SessionKey_SourceAccountNumber) ?? 0;
        var destinationAccountNumber = HttpContext.Session.GetInt32(SessionKey_DestinationAccountNumber) ?? 0;
        var amountStr = HttpContext.Session.GetString(SessionKey_Amount);
        var amount = string.IsNullOrEmpty(amountStr) ? 0 : decimal.Parse(amountStr);
        var comment = HttpContext.Session.GetString(SessionKey_Comment);
        var serviceChargeStr = HttpContext.Session.GetString(SessionKey_ServiceCharge);
        var serviceCharge = string.IsNullOrEmpty(serviceChargeStr) ? 0 : decimal.Parse(serviceChargeStr);

        var sourceAccountType = "";
        var destinationAccountType = "";

        if (sourceAccountNumber > 0)
        {
            var sourceAccount = context.Accounts.Find(sourceAccountNumber);
            sourceAccountType = sourceAccount?.AccountType == 'C' ? "Checking" : "Savings";
        }

        if (destinationAccountNumber > 0)
        {
            var destinationAccount = context.Accounts.Find(destinationAccountNumber);
            destinationAccountType = destinationAccount?.AccountType == 'C' ? "Checking" : "Savings";
        }

        return new TransferViewModel
        {
            SourceAccountNumber = sourceAccountNumber,
            DestinationAccountNumber = destinationAccountNumber,
            Amount = amount,
            Comment = comment,
            SourceAccountType = sourceAccountType,
            DestinationAccountType = destinationAccountType,
            ServiceCharge = serviceCharge,
            TotalAmount = amount + serviceCharge,
            Accounts = customerId.HasValue 
                ? context.Accounts.Where(a => a.CustomerID == customerId.Value).ToList()
                : []
        };
    }
}