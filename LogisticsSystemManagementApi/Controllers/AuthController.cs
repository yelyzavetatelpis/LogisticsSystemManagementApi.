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
            // dont allow duplicate emails
            var existingUser = await _repository.GetUserByEmailAsync(dto.Email);
            if (existingUser != null)
                return BadRequest("Email already exists");

            // drivers need a unique license number
            if (dto.RoleId == 3)
            {
                var licenseExists = await _repository.IsLicenseExists(dto.LicenseNumber);
                if (licenseExists)
                    return BadRequest("License number already exists");
            }

            // building the user object from the registration form
            var user = new User
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                MobileNumber = dto.MobileNumber,
                RoleId = dto.RoleId,
                LicenseNumber = dto.LicenseNumber
            };

            var userId = await _repository.RegisterUserAsync(user);

            // hash the password before storing it
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
            await _repository.CreateUserCredentialsAsync(userId, passwordHash);

            // create the role-specific record depending on who is registering
            if (dto.RoleId == 4)
                await _repository.CreateCustomerAsync(userId);

            if (dto.RoleId == 3)
                await _repository.CreateDriverAsync(userId, dto.LicenseNumber, 1);

            if (dto.RoleId == 2)
                await _repository.CreateDispatcherAsync(userId);

            return Ok(new { message = "User registered successfully", userId });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            // check if the email exists
            var user = await _repository.GetUserByEmailAsync(dto.Email);
            if (user == null)
                return Unauthorized(new { message = "Invalid email or password" });

            // verify the password against the stored hash
            var passwordHash = await _repository.GetPasswordHashAsync(user.UserId);
            if (string.IsNullOrEmpty(passwordHash) || !BCrypt.Net.BCrypt.Verify(dto.Password, passwordHash))
                return Unauthorized(new { message = "Invalid email or password" });

            // get the role name to include in the token
            var role = await _repository.GetRoleByIdAsync(user.RoleId);
            if (role == null)
                return StatusCode(500, "User role not found");

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

        // jwt token with the user id email and role
        private string GenerateJwtToken(User user, string roleName)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, roleName)
            };

            var jwtKey = _configuration["Jwt:Key"];
            var issuer = _configuration["Jwt:Issuer"];
            var audience = _configuration["Jwt:Audience"];

            if (string.IsNullOrEmpty(jwtKey))
                throw new ArgumentNullException("Jwt:Key is missing");
            if (string.IsNullOrEmpty(issuer))
                throw new ArgumentNullException("Jwt:Issuer is missing");
            if (string.IsNullOrEmpty(audience))
                throw new ArgumentNullException("Jwt:Audience is missing");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
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


