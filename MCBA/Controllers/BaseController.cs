using MCBA.Data;
using Microsoft.AspNetCore.Mvc;

namespace MCBA.Controllers;

public class BaseController : Controller
{
    protected int? GetCustomerID()
    {
        return HttpContext.Session.GetInt32("CustomerID");
    }
    
    protected void ClearSession()
    {
        var customerId = GetCustomerID();
        HttpContext.Session.Clear();
        HttpContext.Session.SetInt32("CustomerID", customerId.Value);
    }
}