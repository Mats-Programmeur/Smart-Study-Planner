using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartStudyPlanner.Api.Contracts;
using SmartStudyPlanner.Api.Extensions;
using SmartStudyPlanner.Api.Services;

namespace SmartStudyPlanner.Api.Controllers
{
    [ApiController]
    [Route("auth")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;

        public AuthController(AuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register(RegisterRequest request)
        {
            var result = await _authService.RegisterAsync(request);

            if (!result.Success || result.Response is null)
            {
                return BadRequest(result.ErrorMessage);
            }

            return Ok(result.Response);
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            var result = await _authService.LoginAsync(request);

            if (!result.Success || result.Response is null)
            {
                return Unauthorized(result.ErrorMessage);
            }

            return Ok(result.Response);
        }

        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> Me()
        {
            var userId = User.GetUserId();
            var user = await _authService.GetByIdAsync(userId);

            if (user is null)
            {
                return NotFound("Gebruiker niet gevonden.");
            }

            return Ok(new
            {
                user.Id,
                user.Naam,
                user.Email,
                user.Rol,
                user.IsActief
            });
        }
    }
}
