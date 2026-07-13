using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Student_Management_System.Models;
using Student_Management_System.Repositories;
using BCrypt.Net;

namespace Student_Management_System.Controllers
{ 
    // Only Admins can see or manage this table — this is where an Admin
    // creates the login for a new Teacher or Student (exactly the flow you described).
    [ApiController]
    [Route("api/users")]
    [Authorize(Roles = "Admin")]

    public class UsersController : ControllerBase
    {
        private readonly UserRepository _repo;
        public UsersController(UserRepository repo) => _repo = repo;

        [HttpGet]
        public IActionResult GetAll([FromQuery] string? search) => Ok(_repo.GetAll(search));

        [HttpGet("{id}")]
        public IActionResult GetOne(int id)
        {
            var u = _repo.GetById(id);
            return u is null ? NotFound() : Ok(u);
        }

        // POST api/users  body: { fullName, email, password, role: "Teacher"|"Student"|"Admin" }
        [HttpPost]
        public IActionResult Create(CreateUserDto dto)
        {
            if (_repo.EmailExists(dto.Email))
                return BadRequest("Email already in use.");

            var user = new User
            {
                FullName = dto.FullName,
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                RoleId = _repo.GetRoleId(dto.Role),
                IsActive = true
            };

            int newId = _repo.Create(user);
            user.UserId = newId;
            user.RoleName = dto.Role;
            return CreatedAtAction(nameof(GetOne), new { id = newId }, user);
            // NOTE: after this call succeeds, also create the matching row in
            // Students or Teachers (with this UserId) so their profile exists too.
        }

        [HttpPut("{id}")]
        public IActionResult Update(int id, User user) => _repo.Update(id, user) ? NoContent() : NotFound();

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            try
            {
                return _repo.Delete(id) ? NoContent() : NotFound();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
