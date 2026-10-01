using MCBA.Data;
using MCBA.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SimpleHashing.Net;

namespace MCBA.Controllers;

public class LoginController(MCBAContext context) : BaseController
{
    private readonly ISimpleHash _simpleHash = new SimpleHash();

    public IActionResult Index()
    {
        if (GetCustomerID() != null)
        {
            return RedirectToAction("Index", "Home");
        }
        return View();
    }

    [HttpPost]
    public IActionResult Index(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var login = context.Logins.Include(l => l.Customer).
            FirstOrDefault(l => l.LoginID == model.LoginID);

        if (login == null || !_simpleHash.Verify(model.Password, login.PasswordHash))
        {
            ModelState.AddModelError(string.Empty, "Invalid Login ID or password");
            return View(model);
        }
        
        HttpContext.Session.SetInt32("CustomerID", login.CustomerID);
        
        return RedirectToAction("Index", "Home");
    }
    
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction(nameof(Index));
    }
}