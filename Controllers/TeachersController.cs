using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Student_Management_System.Models;
using Student_Management_System.Repositories;
using System.Security.Claims;

namespace Student_Management_System.Controllers
{
    [ApiController]
    [Route("api/teachers")]
    [Authorize]

    public class TeachersController : ControllerBase
    {
        private readonly TeacherRepository _repo;
        public TeachersController(TeacherRepository repo) => _repo = repo;

        // A logged-in Teacher only gets their own row. Admin gets everyone (with optional search).
        [HttpGet]
        public IActionResult GetAll([FromQuery] string? search)
        {
            var role = User.FindFirstValue(ClaimTypes.Role);
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            if (role == "Teacher")
            {
                var mine = _repo.GetByUserId(userId);
                return Ok(mine is null ? new List<Teacher>() : new List<Teacher> { mine });
            }

            return Ok(_repo.GetAll(search));
        }

        [HttpGet("{id}")]
        public IActionResult GetOne(int id)
        {
            var teacher = _repo.GetById(id);
            return teacher is null ? NotFound() : Ok(teacher);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IActionResult Create(Teacher teacher)
        {
            int newId = _repo.Create(teacher);
            teacher.TeacherId = newId;
            return CreatedAtAction(nameof(GetOne), new { id = newId }, teacher);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public IActionResult Update(int id, Teacher teacher)
        {
            return _repo.Update(id, teacher) ? NoContent() : NotFound();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public IActionResult Delete(int id)
        {
            return _repo.Delete(id) ? NoContent() : NotFound();
        }
    }
}
