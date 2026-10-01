using System.Diagnostics;
using MCBA.Data;
using MCBA.Filters;
using Microsoft.AspNetCore.Mvc;
using MCBA.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace MCBA.Controllers;

[Authorization]
public class HomeController(MCBAContext context) : BaseController
{
    public IActionResult Index()
    {
        var customerId = GetCustomerID();
        
        var customer = context.Customers.
            Include(c => c.Accounts).
            FirstOrDefault(c => c.CustomerID == customerId);

        if (customer == null)
        {
            return RedirectToAction("Index", "Login");
        }
        
        return View(customer);
    }

    public IActionResult Privacy()
    {
        return View();
    }
}
