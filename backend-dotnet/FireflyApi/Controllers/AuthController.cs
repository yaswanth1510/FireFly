using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FireflyApi.DTOs;
using FireflyApi.Models;
using FireflyApi.Services;

namespace FireflyApi.Controllers
{
    [ApiController]
    [Route("api/login")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IJwtService _jwtService;

        public AuthController(
            UserManager<User> userManager, 
            SignInManager<User> signInManager,
            IJwtService jwtService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _jwtService = jwtService;
        }

        [HttpPost("access-token")]
        public async Task<ActionResult<TokenDto>> LoginAccessToken([FromForm] LoginRequestDto request)
        {
            var user = await _userManager.FindByEmailAsync(request.Username);
            if (user == null)
            {
                return BadRequest(new { detail = "Incorrect email or password" });
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);
            if (!result.Succeeded)
            {
                return BadRequest(new { detail = "Incorrect email or password" });
            }

            if (user.Disabled)
            {
                return BadRequest(new { detail = "Inactive user" });
            }

            var token = _jwtService.GenerateAccessToken(user);
            return Ok(new TokenDto
            {
                AccessToken = token,
                TokenType = "bearer"
            });
        }

        [HttpPost("test-token")]
        [Authorize]
        public async Task<ActionResult<UserDto>> TestToken()
        {
            var userId = User.FindFirst("sub")?.Value;
            if (userId == null)
            {
                return Unauthorized();
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound(new { detail = "User not found" });
            }

            return Ok(new UserDto
            {
                Id = user.Id,
                Email = user.Email ?? string.Empty,
                FullName = user.FullName,
                Disabled = user.Disabled
            });
        }

        [HttpPost("signup")]
        public async Task<ActionResult<TokenDto>> SignUp([FromBody] SignUpRequestDto request)
        {
            // Check if user already exists
            var existingUser = await _userManager.FindByEmailAsync(request.Email);
            if (existingUser != null)
            {
                return BadRequest(new { detail = "The user with this Email already exists." });
            }

            // Check if full name is already taken
            var existingUserByName = await _userManager.Users
                .FirstOrDefaultAsync(u => u.FullName == request.FullName);
            if (existingUserByName != null)
            {
                return BadRequest(new { detail = "The user with this Name already exists." });
            }

            var user = new User
            {
                UserName = request.Email,
                Email = request.Email,
                FullName = request.FullName
            };

            var result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
            {
                return BadRequest(new { detail = string.Join(", ", result.Errors.Select(e => e.Description)) });
            }

            var token = _jwtService.GenerateAccessToken(user);
            return Ok(new TokenDto
            {
                AccessToken = token,
                TokenType = "bearer"
            });
        }
    }
}