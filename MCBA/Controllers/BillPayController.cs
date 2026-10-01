using Microsoft.AspNetCore.Mvc;
using MCBA.Data;
using MCBA.Filters;
using MCBA.Models;
using MCBA.ViewModels;

namespace MCBA.Controllers;

[Authorization]
public class BillPayController(MCBAContext context) : BaseController
{
    public IActionResult Index()
    {
        var customerId = GetCustomerID();
        var accounts = context.Accounts.Where(a => a.CustomerID == customerId).ToList();

        // Get all bills for this customer's accounts
        var accountNumbers = accounts.Select(a => a.AccountNumber).ToList();
        var scheduledBills = context.BillPays
            .Where(b => accountNumbers.Contains(b.AccountNumber))
            .ToList();

        foreach (var bill in scheduledBills)
        {
            bill.Account = context.Accounts.Find(bill.AccountNumber);
            bill.Payee = context.Payees.Find(bill.PayeeID);
        }

        var viewModel = new BillPayListViewModel
        {
            ScheduledBills = scheduledBills,
            Accounts = accounts
        };

        return View(viewModel);
    }

    public IActionResult Create()
    {
        var customerId = GetCustomerID();
        var accounts = context.Accounts.Where(a => a.CustomerID == customerId).ToList();
        var payees = context.Payees.ToList();

        var viewModel = new BillPayViewModel
        {
            Accounts = accounts,
            Payees = payees,
            ScheduleTimeUtc = DateTime.Now
        };

        return View(viewModel);
    }

    [HttpPost]
    public IActionResult Create(BillPayViewModel viewModel)
    {
        var customerId = GetCustomerID();

        if (!ModelState.IsValid)
        {
            viewModel.Accounts = context.Accounts.Where(a => a.CustomerID == customerId).ToList();
            viewModel.Payees = context.Payees.ToList();
            return View(viewModel);
        }

        var account = context.Accounts.Find(viewModel.AccountNumber);
        if (account == null || account.CustomerID != customerId)
            return RedirectToAction(nameof(Index));

        // Verify payee exists
        var payee = context.Payees.Find(viewModel.PayeeID);
        if (payee == null)
        {
            ModelState.AddModelError("", "Invalid payee selected");
            viewModel.Accounts = context.Accounts.Where(a => a.CustomerID == customerId).ToList();
            viewModel.Payees = context.Payees.ToList();
            return View(viewModel);
        }

        var billPay = new BillPay
        {
            AccountNumber = viewModel.AccountNumber,
            PayeeID = viewModel.PayeeID,
            Amount = viewModel.Amount,
            ScheduleTimeUtc = viewModel.ScheduleTimeUtc.ToUniversalTime(),
            Period = viewModel.Period
        };

        context.BillPays.Add(billPay);
        context.SaveChanges();

        TempData["SuccessMessage"] = "Bill payment scheduled successfully";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public IActionResult Cancel(int id)
    {
        var customerId = GetCustomerID();
        var billPay = context.BillPays.Find(id);

        if (billPay == null)
            return RedirectToAction(nameof(Index));

        // Verify the bill belongs to one of the customer's accounts
        var account = context.Accounts.Find(billPay.AccountNumber);
        if (account == null || account.CustomerID != customerId)
            return RedirectToAction(nameof(Index));

        context.BillPays.Remove(billPay);
        context.SaveChanges();

        TempData["SuccessMessage"] = "Bill payment cancelled successfully";
        return RedirectToAction(nameof(Index));
    }
    
    [HttpPost]
    public IActionResult Retry(int id)
    {
        var customerId = GetCustomerID();
        var billPay = context.BillPays.Find(id);

        if (billPay == null)
            return RedirectToAction(nameof(Index));

        var account = context.Accounts.Find(billPay.AccountNumber);
        if (account == null || account.CustomerID != customerId)
            return RedirectToAction(nameof(Index));

        // Check if bill is failed
        if (billPay.Status != "Failed")
        {
            TempData["ErrorMessage"] = "Only failed payments can be retried";
            return RedirectToAction(nameof(Index));
        }

        billPay.Status = "Pending";
        billPay.ScheduleTimeUtc = DateTime.UtcNow.AddMinutes(1);
    
        context.SaveChanges();

        TempData["SuccessMessage"] = "Payment retry scheduled. It will be processed shortly.";
        return RedirectToAction(nameof(Index));
    }
}