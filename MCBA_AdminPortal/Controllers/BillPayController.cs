using MCBA_AdminPortal.Filters;
using MCBA_AdminPortal.Helper;
using MCBA_AdminPortal.Models;
using Microsoft.AspNetCore.Mvc;

namespace MCBA_AdminPortal.Controllers;

[AuthorizeAdmin]
public class BillPayController : Controller
{
    private readonly IApiGeneric _api;

    public BillPayController(IApiGeneric api)
    {
        _api = api;
    }

    public IActionResult Index()
    {
        var billPays = _api.Get<List<BillPay>>("BillPay") ?? new List<BillPay>();
        return View(billPays);
    }

    [HttpPost]
    public IActionResult Block(int id)
    {
        var success = _api.Post($"BillPay/block/{id}");
        if (success)
            TempData["SuccessMessage"] = "Bill payment blocked successfully";
        else
            TempData["ErrorMessage"] = "Failed to block bill payment";

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public IActionResult Unblock(int id)
    {
        var success = _api.Post($"BillPay/unblock/{id}");
        if (success)
            TempData["SuccessMessage"] = "Bill payment unblocked successfully";
        else
            TempData["ErrorMessage"] = "Failed to unblock bill payment";

        return RedirectToAction(nameof(Index));
    }
}