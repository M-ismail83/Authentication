using Authentication.Services;
using Microsoft.AspNetCore.Mvc;

namespace Authentication.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public IActionResult Login()
        {
            // Logic will come after
            return Ok("Login endpoint hit");
        }

        [HttpPost("totp")]
        public IActionResult TOTP()
        {
            // Logic will come after as well
            return Ok("TOTP endpoint hit");
        }
    }
}