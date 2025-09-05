using LunaEdgeTask.DTOS;
using LunaEdgeTask.Models;
using LunaEdgeTask.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace LunaEdgeTask.Controllers
{
    [ApiController]
    [Route("users")]
    public class UsersController : ControllerBase
    {
        private readonly UserService _userService;
        private readonly IConfiguration _config;
        private readonly ILogger<UsersController> _logger;

        public UsersController(UserService userService, IConfiguration config, ILogger<UsersController> logger)
        {
            _userService = userService;
            _config = config;
            _logger = logger;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            try
            {
                _logger.LogInformation("Attempting to register user with username {Username} and email {Email}", dto.Username, dto.Email);
                var result = await _userService.RegisterUserAsync(dto);
                if (!result.Success)
                {
                    _logger.LogWarning("User registration failed for username {Username}: {Message}", dto.Username, result.Message);
                    return BadRequest(result.Message);
                }
                _logger.LogInformation("User {Username} registered successfully", dto.Username);
                return Ok(new { message = result.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during user registration for username {Username}", dto.Username);
                throw;
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            try
            {
                _logger.LogInformation("User login attempt with username {Username}", dto.UsernameOrEmail);
                var user = await _userService.AuthenticateUserAsync(dto);
                if (user == null)
                {
                    _logger.LogWarning("Login failed for username {Username}: Invalid credentials", dto.UsernameOrEmail);
                    return Unauthorized("Invalid credentials");
                }
                var token = GenerateJwtToken(user);
                _logger.LogInformation("User {Username} logged in successfully", user.Username);
                return Ok(new { token });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for username {Username}", dto.UsernameOrEmail);
                throw;
            }
        }

        private string GenerateJwtToken(User user)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}