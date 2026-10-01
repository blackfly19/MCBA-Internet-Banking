using Microsoft.AspNetCore.Mvc;
using MCBA.Data;
using MCBA.Filters;
using MCBA.ViewModels;
using SimpleHashing.Net;

namespace MCBA.Controllers;

[Authorization]
public class ProfileController(MCBAContext context) : BaseController
{
    private static readonly ISimpleHash _simpleHash = new SimpleHash();

    private const string SessionKey_EditName = $"{nameof(ProfileController)}_EditName";
    private const string SessionKey_EditTFN = $"{nameof(ProfileController)}_EditTFN";
    private const string SessionKey_EditAddress = $"{nameof(ProfileController)}_EditAddress";
    private const string SessionKey_EditCity = $"{nameof(ProfileController)}_EditCity";
    private const string SessionKey_EditState = $"{nameof(ProfileController)}_EditState";
    private const string SessionKey_EditPostcode = $"{nameof(ProfileController)}_EditPostcode";
    private const string SessionKey_EditMobile = $"{nameof(ProfileController)}_EditMobile";

    public IActionResult Index()
    {
        var customerId = GetCustomerID();
        var customer = context.Customers.Find(customerId);

        if (customer == null)
            return RedirectToAction("Index", "Home");

        var viewModel = new ProfileViewModel
        {
            CustomerID = customer.CustomerID,
            Name = customer.Name,
            TFN = customer.TFN,
            Address = customer.Address,
            City = customer.City,
            State = customer.State,
            PostCode = customer.PostCode,
            Mobile = customer.Mobile
        };

        return View(viewModel);
    }

    public IActionResult Edit()
    {
        var customerId = GetCustomerID();
        var customer = context.Customers.Find(customerId);

        if (customer == null)
            return RedirectToAction("Index", "Home");

        var viewModel = new ProfileViewModel
        {
            CustomerID = customer.CustomerID,
            Name = customer.Name,
            TFN = customer.TFN,
            Address = customer.Address,
            City = customer.City,
            State = customer.State,
            PostCode = customer.PostCode,
            Mobile = customer.Mobile
        };

        return View(viewModel);
    }

    [HttpPost]
    public IActionResult Edit(ProfileViewModel viewModel)
    {
        if (!ModelState.IsValid)
        {
            return View(viewModel);
        }

        // Store in session for confirmation
        HttpContext.Session.SetString(SessionKey_EditName, viewModel.Name);
        HttpContext.Session.SetString(SessionKey_EditTFN, viewModel.TFN ?? "");
        HttpContext.Session.SetString(SessionKey_EditAddress, viewModel.Address ?? "");
        HttpContext.Session.SetString(SessionKey_EditCity, viewModel.City ?? "");
        HttpContext.Session.SetString(SessionKey_EditState, viewModel.State ?? "");
        HttpContext.Session.SetString(SessionKey_EditPostcode, viewModel.PostCode ?? "");
        HttpContext.Session.SetString(SessionKey_EditMobile, viewModel.Mobile ?? "");

        return RedirectToAction(nameof(ConfirmEdit));
    }

    public IActionResult ConfirmEdit()
    {
        int customerId = GetCustomerID().GetValueOrDefault();
        var viewModel = new ProfileViewModel
        {
            CustomerID = customerId,
            Name = HttpContext.Session.GetString(SessionKey_EditName),
            TFN = HttpContext.Session.GetString(SessionKey_EditTFN),
            Address = HttpContext.Session.GetString(SessionKey_EditAddress),
            City = HttpContext.Session.GetString(SessionKey_EditCity),
            State = HttpContext.Session.GetString(SessionKey_EditState),
            PostCode = HttpContext.Session.GetString(SessionKey_EditPostcode),
            Mobile = HttpContext.Session.GetString(SessionKey_EditMobile)
        };

        return View(viewModel);
    }

    [HttpPost]
    public IActionResult CompleteEdit()
    {
        var customerId = GetCustomerID();
        var customer = context.Customers.Find(customerId);

        if (customer == null)
            return RedirectToAction("Index", "Home");

        customer.Name = HttpContext.Session.GetString(SessionKey_EditName);
        customer.TFN = HttpContext.Session.GetString(SessionKey_EditTFN);
        customer.Address = HttpContext.Session.GetString(SessionKey_EditAddress);
        customer.City = HttpContext.Session.GetString(SessionKey_EditCity);
        customer.State = HttpContext.Session.GetString(SessionKey_EditState);
        customer.PostCode = HttpContext.Session.GetString(SessionKey_EditPostcode);
        customer.Mobile = HttpContext.Session.GetString(SessionKey_EditMobile);

        context.SaveChanges();
        ClearSession();

        TempData["SuccessMessage"] = "Profile updated successfully";
        return RedirectToAction(nameof(Index));
    }

    public IActionResult CancelEdit()
    {
        ClearSession();
        return RedirectToAction(nameof(Index));
    }

    public IActionResult ChangePassword()
    {
        return View(new ChangePasswordViewModel());
    }

    [HttpPost]
    public IActionResult ChangePassword(ChangePasswordViewModel viewModel)
    {
        
        if (!ModelState.IsValid)
            return View(viewModel);

        var customerId = GetCustomerID();
        var login = context.Logins.FirstOrDefault(l => l.CustomerID == customerId);

        if (login == null)
            return RedirectToAction("Index", "Home");

        if (!_simpleHash.Verify(viewModel.CurrentPassword, login.PasswordHash))
        {
            ModelState.AddModelError("CurrentPassword", "Current password is incorrect");
            return View(viewModel);
        }

        login.PasswordHash = _simpleHash.Compute(viewModel.NewPassword);
        context.SaveChanges();

        TempData["SuccessMessage"] = "Password changed successfully";
        return RedirectToAction(nameof(Index));
    }
}