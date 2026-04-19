using LogisticsSystemManagementApi.DTOs;
using LogisticsSystemManagementApi.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LogisticsSystemManagementApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class AccountsController : ControllerBase
    {
        private readonly IAccountsRepository _repository;

        public AccountsController(IAccountsRepository repository)
        {
            _repository = repository;
        }

        // get all users
        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _repository.GetUsers();
            return Ok(users);
        }

        // get a single user by id
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUser(int id)
        {
            var user = await _repository.GetUserById(id);
            if (user == null) return NotFound();
            return Ok(user);
        }

        // create a new user
        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] AccountsDto dto)
        {
            var id = await _repository.CreateUser(dto);
            return Ok(new { message = "User created successfully", userId = id });
        }

        // update an existing user
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] AccountsDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var updated = await _repository.UpdateUser(id, dto);
            if (!updated) return NotFound(new { message = "User not found" });
            return Ok(new { message = "User updated successfully" });
        }

        // delete a user and all their linked records
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var deleted = await _repository.DeleteUser(id);
            if (!deleted) return NotFound(new { message = "User not found" });
            return Ok(new { message = "User deleted successfully" });
        }
        // get all drivers with their availability status
        [HttpGet("drivers")]
        public async Task<IActionResult> GetDrivers()
        {
            var drivers = await _repository.GetDriversAsync();
            return Ok(drivers);
        }
    }
}


