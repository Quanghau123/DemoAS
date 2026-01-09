using Microsoft.AspNetCore.Mvc;

using DemoEF.Application.Interfaces;

namespace DemoEF.WebApi.Controllers
{
    [ApiController]
    [Route("api/v1/auth/oauth")]
    public class OAuthController : ControllerBase
    {
        private readonly IOAuthService _oauthService;
        private readonly IConfiguration _configuration;
        public OAuthController(IOAuthService oauthService, IConfiguration configuration)
        {
            _oauthService = oauthService;
            _configuration = configuration;
        }

        /// <summary>
        /// Redirect người dùng sang Google để đăng nhập OAuth2.
        /// </summary>
        /// <remarks>
        /// Với Swagger UI sẽ cố gắng fetch kết quả JSON để hiển thị response.
        /// Nhưng endpoint này không trả JSON, mà Redirect (HTTP 302) sang Google.
        /// Test bằng Browser sẽ đúng hơn.
        /// </remarks>
        [HttpGet("google/login")]
        public IActionResult GoogleLogin()
        {
            var clientId = _configuration["Google:ClientId"];
            var redirectUri = _configuration["Google:RedirectUri"];
            if (string.IsNullOrEmpty(redirectUri))
            {
                return BadRequest("Google redirect URI is not configured.");
            }

            var url =
                "https://accounts.google.com/o/oauth2/v2/auth" +
                "?client_id=" + clientId +
                "&redirect_uri=" + Uri.EscapeDataString(redirectUri) +
                "&response_type=code" +
                "&scope=openid%20email%20profile" +
                "&access_type=offline" +
                "&prompt=consent";

            return Redirect(url);
        }

        [HttpGet("google/callback")]
        public async Task<IActionResult> GoogleCallback(string code)
        {
            var result = await _oauthService.LoginWithGoogleAsync(code);
            return Ok(result);
        }

        [HttpGet("facebook/login")]
        public IActionResult FacebookLogin()
        {
            var clientId = _configuration["Facebook:ClientId"];
            var redirectUri = _configuration["Facebook:RedirectUri"];
            if (string.IsNullOrEmpty(redirectUri))
            {
                return BadRequest("Facebook redirect URI is not configured.");
            }
            // var state = Guid.NewGuid().ToString();

            var url = $"https://www.facebook.com/v18.0/dialog/oauth" +
                    $"?client_id={clientId}" +
                    $"&redirect_uri={Uri.EscapeDataString(redirectUri)}" +
                    // $"&state={state}" +
                    // $"&scope=email,public_profile";
                    $"&response_type=code" +
                    $"&scope=public_profile";

            return Redirect(url);
        }

        [HttpGet("facebook/callback")]
        public async Task<IActionResult> FacebookCallback(string code)
        {
            var result = await _oauthService.LoginWithFacebookAsync(code);
            return Ok(result);
        }
    }
}
