using Authentication.Services;
using Microsoft.AspNetCore.Mvc;
using Authentication.DTOs;

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

        [HttpPost("register")]
        public async Task<ActionResult<AuthResponseDTO>> Register(RegisterDTO registerDto)
        {
            var register = await _authService.Register(registerDto);

            if (register.IsSuccess == false)
            {
                return Unauthorized(new { message = register.Message });
            }

            return Ok(register);
        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthResponseDTO>> Login(LoginRequestDTO loginDto)
        {
            var login = await _authService.Login(loginDto);
            if (login.IsSuccess == false)
            {
                return Unauthorized(new { message = login.Message });
            }
            return Ok(login);
        }

        [HttpPost("totp")]
        public async Task<ActionResult<AuthResponseDTO>> TOTP(VerifyTotpDTO totpDto)
        {
            var totp = await _authService.verifyTOTP(totpDto);
            if (totp.IsSuccess == false)
            {
                return Unauthorized(new { message = totp.Message });
            }
            return Ok(totp);
        }
    }
}