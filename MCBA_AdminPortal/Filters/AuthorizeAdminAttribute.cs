using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Caching.Memory;

namespace MCBA_AdminPortal.Filters;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class AuthorizeAdminAttribute : Attribute, IAuthorizationFilter
{
    private const string TokenCacheKey = "AdminToken";

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var cache = context.HttpContext.RequestServices.GetService<IMemoryCache>();
        
        if (cache?.TryGetValue(TokenCacheKey, out string token) != true || string.IsNullOrEmpty(token))
        {
            context.Result = new RedirectToActionResult("Index", "Login", null);
        }
    }
}