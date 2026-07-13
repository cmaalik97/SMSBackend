using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Student_Management_System.Models;
using Student_Management_System.Repositories;
using Student_Management_System.Services;
using BCrypt.Net;

namespace Student_Management_System.Controllers
{
    [ApiController]
    [Route("api/auth")]

    public class AuthController : ControllerBase
    {
        private readonly UserRepository _users;
        private readonly TokenService _tokens;

        public AuthController(UserRepository users, TokenService tokens)
        {
            _users = users;
            _tokens = tokens;
        }

        // POST api/auth/login -> the public Sign In page, for ALL roles
        [HttpPost("login")]
        public IActionResult Login(LoginDto dto)
        {
            var user = _users.GetByEmail(dto.Email);
            if (user is null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                return Unauthorized("Invalid email or password.");

            string token = _tokens.CreateToken(user);
            return Ok(new { token, fullName = user.FullName, role = user.RoleName, userId = user.UserId });
        }

        // POST api/auth/register-admin -> the public Sign Up page.
        // Only creates Admin accounts, exactly like you described:
        // Teacher/Student logins are created later via UsersController by an Admin.
        [HttpPost("register-admin")]
        public IActionResult RegisterAdmin(RegisterAdminDto dto)
        {
            if (_users.EmailExists(dto.Email))
                return BadRequest("An account with this email already exists.");

            var user = new User
            {
                FullName = dto.FullName,
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password), // never store plain text passwords
                RoleId = _users.GetRoleId("Admin"),
                IsActive = true
            };

            int newId = _users.Create(user);
            user.UserId = newId;
            user.RoleName = "Admin";

            string token = _tokens.CreateToken(user);
            return Ok(new { token, fullName = user.FullName, role = "Admin", userId = newId });
        }
    }
}
