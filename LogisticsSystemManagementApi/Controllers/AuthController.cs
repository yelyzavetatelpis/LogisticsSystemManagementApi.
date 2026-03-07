using LogisticsSystemManagementApi.DTOs;
using LogisticsSystemManagementApi.Models;
using LogisticsSystemManagementApi.Repositories;
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
            // Check if the email is already in use
            var existingUser = await _repository.GetUserByEmailAsync(dto.Email);
            if (existingUser != null)
                return BadRequest(new { message = "Email already registered" });

            // Create user record
            var user = new User
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                MobileNumber = dto.MobileNumber,
                RoleId = dto.RoleId
            };

            var userId = await _repository.RegisterUserAsync(user);

            // Store the hashed password in UserCredentials
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
            await _repository.CreateUserCredentialsAsync(userId, passwordHash);

            // Create a Customer record, roleid is 4
            if (dto.RoleId == 4)
                await _repository.CreateCustomerAsync(userId);

            return Ok(new { message = "User registered successfully", userId = userId });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            // Check if a user with this email exists
            var user = await _repository.GetUserByEmailAsync(dto.Email);
            if (user == null)
                return Unauthorized(new { message = "Invalid email or password" });

            // Verify the password with the hashed one 
            var passwordHash = await _repository.GetPasswordHashAsync(user.UserId);
            if (string.IsNullOrEmpty(passwordHash) || !BCrypt.Net.BCrypt.Verify(dto.Password, passwordHash))
                return Unauthorized(new { message = "Invalid email or password" });

            // Get the user's role name
            var role = await _repository.GetRoleByIdAsync(user.RoleId);
            if (role == null)
                return StatusCode(500, "User role not found");

            // Generate and return the JWT token
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

        //jwt token with user id, email and role
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