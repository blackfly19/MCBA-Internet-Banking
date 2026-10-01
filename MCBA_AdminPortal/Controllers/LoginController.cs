using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System.Text;
using System.Text.Json;

namespace MCBA_AdminPortal.Controllers;

public class LoginController : Controller
{
    private readonly HttpClient _httpClient;
    private readonly string _apiBaseUrl;
    private readonly IMemoryCache _cache;
    private const string TokenCacheKey = "AdminToken";

    public LoginController(HttpClient httpClient, IConfiguration configuration, IMemoryCache cache)
    {
        _httpClient = httpClient;
        _apiBaseUrl = configuration["ConnectionStrings:AdminApiUrl"];
        _cache = cache;
    }
    
    public IActionResult Index()
    {
        return View();
    }

    [HttpPost]
    public IActionResult Index(string username, string password)
    {
        var credentials = new { username, password };
        var json = JsonSerializer.Serialize(credentials);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = _httpClient.PostAsync($"{_apiBaseUrl}/authorization/login", content).Result;

        if (response.IsSuccessStatusCode)
        {
            var token = response.Content.ReadAsStringAsync().Result;
            token = token.Trim('"');

            if (!string.IsNullOrEmpty(token))
            {
                _cache.Set(TokenCacheKey, token, TimeSpan.FromHours(1));
                return RedirectToAction("Index", "Home");
            }
        }

        ModelState.AddModelError("", "Invalid username or password");
        return View();
    }

    public async Task<IActionResult> Logout()
    {
        if (_cache.TryGetValue(TokenCacheKey, out string? token) && token != null)
        {
            var json = JsonSerializer.Serialize(token);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                await _httpClient.PostAsync($"{_apiBaseUrl}/authorization/logout", content);
            }
            catch (HttpRequestException)
            {
            }
        }

        _cache.Remove(TokenCacheKey);
        
        return RedirectToAction(nameof(Index));
    }
}