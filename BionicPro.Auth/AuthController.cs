using BionicPro.Auth.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BionicPro.Auth
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController(ILogger<AuthController> logger) : ControllerBase
    {
        [HttpGet("login")]
        [AllowAnonymous]
        public IActionResult Login([FromQuery] string returnUrl = "/")
        {
            return Challenge(new AuthenticationProperties { RedirectUri = returnUrl }, OpenIdConnectDefaults.AuthenticationScheme);
        }

        [HttpGet("logout")]
        [Authorize]
        public IActionResult Logout()
        {
            return SignOut(new AuthenticationProperties { RedirectUri = "/" },
                CookieAuthenticationDefaults.AuthenticationScheme,
                OpenIdConnectDefaults.AuthenticationScheme);
        }

        [HttpGet("token")]
        [Authorize]
        [ProducesResponseType<AuthResponse>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetSessionTokenAsync()
        {
            var accessToken = await HttpContext.GetTokenAsync("access_token");
            var refreshToken = await HttpContext.GetTokenAsync("refresh_token");

            if (string.IsNullOrEmpty(accessToken))
            {
                logger.LogWarning("Access token is missing for user {User}.", User.Identity?.Name);
                return Unauthorized("Токен отсутствует.");
            }

            var response = new AuthResponse{ AccessToken = accessToken, RefreshToken = refreshToken ?? string.Empty };
            return Ok(response);
        }
    }
}
