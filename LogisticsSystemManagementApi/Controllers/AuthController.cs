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
            // Checking if email already exists
            var existingUser = await _repository.GetUserByEmailAsync(dto.Email);
            if (existingUser != null)
            {
                return BadRequest(new { message = "Email already registered" });
            }

            // Creating new user record
            var user = new User
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                MobileNumber = dto.MobileNumber,
                RoleId = dto.RoleId
            };

            var userId = await _repository.RegisterUserAsync(user);

            //  UserCredentials record with hashed password
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
            await _repository.CreateUserCredentialsAsync(userId, passwordHash);

            //  Customer record if role is Customer (RoleId = 4)
            if (dto.RoleId == 4)
            {
                await _repository.CreateCustomerAsync(userId);
            }

            return Ok(new { message = "User registered successfully", userId = userId });

            
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            // Get user by email
            var user = await _repository.GetUserByEmailAsync(dto.Email);
            if (user == null)
            {
                return Unauthorized(new { message = "Invalid email or password" });
            }

            // Get password hash from UserCredentials
            var passwordHash = await _repository.GetPasswordHashAsync(user.UserId);
            if (string.IsNullOrEmpty(passwordHash) || !BCrypt.Net.BCrypt.Verify(dto.Password, passwordHash))
            {
                return Unauthorized(new { message = "Invalid email or password" });
            }

            // Get role name
            var role = await _repository.GetRoleByIdAsync(user.RoleId);
            if (role == null)
            {
                return StatusCode(500, "User role not found");
            }

            // Generate JWT token
            var token = GenerateJwtToken(user, role.RoleName);

            return Ok(new AuthResponseDto
            {
                Token = token,
                Email = user.Email,
                Role = role.RoleName,
                FirstName = user.FirstName,
                LastName = user.LastName
            });
        }

        private string GenerateJwtToken(User user, string roleName)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
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