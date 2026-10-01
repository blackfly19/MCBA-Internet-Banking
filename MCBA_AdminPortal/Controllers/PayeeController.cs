using MCBA_AdminPortal.Filters;
using MCBA_AdminPortal.Helper;
using MCBA_AdminPortal.Models;
using Microsoft.AspNetCore.Mvc;

namespace MCBA_AdminPortal.Controllers;

[AuthorizeAdmin]
public class PayeeController : Controller
{
    private readonly IApiGeneric _api;

    public PayeeController(IApiGeneric api)
    {
        _api = api;
    }

    public IActionResult Index(string postcode)
    {
        List<Payee> payees;

        if (string.IsNullOrEmpty(postcode))
        {
            payees = _api.Get<List<Payee>>("Payee") ?? new List<Payee>();
        }
        else
        {
            payees = _api.Get<List<Payee>>($"Payee/postcode/{postcode}") ?? new List<Payee>();
        }

        ViewBag.Postcode = postcode;
        return View(payees);
    }

    public IActionResult Edit(int id)
    {
        var payee = _api.Get<Payee>($"Payee/{id}");
        if (payee == null)
            return RedirectToAction(nameof(Index));

        return View(payee);
    }

    [HttpPost]
    public IActionResult Edit(Payee payee)
    {
        var success = _api.Put($"Payee/{payee.PayeeID}", payee);
        if (success)
        {
            TempData["SuccessMessage"] = "Payee updated successfully";
            return RedirectToAction(nameof(Index));
        }

        ModelState.AddModelError("", "Failed to update payee");
        return View(payee);
    }
}