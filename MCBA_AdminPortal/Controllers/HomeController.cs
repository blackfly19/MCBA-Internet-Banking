using Microsoft.AspNetCore.Mvc;
using MCBA_AdminPortal.Filters;

namespace MCBA_AdminPortal.Controllers;

[AuthorizeAdmin]
public class HomeController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}