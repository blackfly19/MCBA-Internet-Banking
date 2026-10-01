using MCBA_AdminWebApi.Models;
using MCBA_AdminWebApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace MCBA_AdminWebApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthorizationController(IConfiguration configuration) : ControllerBase
{
    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest credentials)
    {
        string username = credentials.username;
        string password = credentials.password;

        var adminUsername = configuration["AdminCredentials:Username"];
        var adminPassword = configuration["AdminCredentials:Password"];

        if (!string.IsNullOrEmpty(adminUsername) && !string.IsNullOrEmpty(adminPassword) &&
            username == adminUsername && password == adminPassword)
        {
            var token = Guid.NewGuid().ToString();
            TokenStore.AddToken(token);
            return Ok(token);
        }

        return Unauthorized("Invalid username or password");
    }
    
    [HttpPost("logout")]
    public IActionResult Logout([FromBody] string token)
    {
        TokenStore.RemoveToken(token);
        return Ok("Logged out successfully");
    }
}