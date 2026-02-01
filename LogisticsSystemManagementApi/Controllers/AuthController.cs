using LogisticsSystemManagementApi.DTOs;
using LogisticsSystemManagementApi.Models;
using LogisticsSystemManagementApi.Repositories;
using BCrypt.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace LogisticsSystemManagementApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthRepository _repository;
        private readonly IConfiguration _configuration;

        public AuthController(IAuthRepository repository, IConfiguration configuration)
        {
            _repository = repository;
            _configuration = configuration;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            var existingUser = await _repository.GetUserByUsernameAsync(dto.Username);

            var user = new User
            {
                Username = dto.Username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                FullName = dto.FullName,
                Email = dto.Email,
                RoleId = dto.RoleId,
                IsActive = true
            };

            var id = await _repository.RegisterUserAsync(user);
            return Ok(new { Message = "User registered successfully", UserId = id });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            var user = await _repository.GetUserByUsernameAsync(dto.Username);
            if (user == null || string.IsNullOrEmpty(user.PasswordHash) || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            {
                return Unauthorized("Invalid username or password");
            }

            var role = await _repository.GetRoleByIdAsync(user.RoleId);
            if (role == null)
            {
                return StatusCode(500, "User role not found");
            }

            var token = GenerateJwtToken(user, role.Name);

            return Ok(new AuthResponseDto
            {
                Token = token,
                Username = user.Username,
                Role = role.Name,
                FullName = user.FullName ?? user.Username
            });
        }

        private string GenerateJwtToken(User user, string roleName)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, roleName)
            };

            var keyStr = _configuration["Jwt:Key"];
            var issuer = _configuration["Jwt:Issuer"];
            var audience = _configuration["Jwt:Audience"];

            if (string.IsNullOrEmpty(keyStr)) throw new ArgumentNullException("Jwt:Key is missing in configuration");
            if (string.IsNullOrEmpty(issuer)) throw new ArgumentNullException("Jwt:Issuer is missing in configuration");
            if (string.IsNullOrEmpty(audience)) throw new ArgumentNullException("Jwt:Audience is missing in configuration");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyStr));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}