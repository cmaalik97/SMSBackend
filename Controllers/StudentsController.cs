using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Student_Management_System.Models;
using Student_Management_System.Repositories;
using System.Security.Claims;

namespace Student_Management_System.Controllers
{
    [ApiController]
    [Route("api/students")]
    [Authorize]

    public class StudentsController : ControllerBase
    {
        private readonly StudentRepository _repo;
        public StudentsController(StudentRepository repo) => _repo = repo;

        // GET api/students                    -> all (Admin/Teacher)
        // GET api/students?search=ali          -> search box on the frontend
        // GET api/students?classId=3           -> "filter by class" dropdown
        // A logged-in Student only ever gets their own row, regardless of query params.
        [HttpGet]
        public IActionResult GetAll([FromQuery] string? search, [FromQuery] int? classId)
        {
            var role = User.FindFirstValue(ClaimTypes.Role);
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            if (role == "Student")
            {
                var mine = _repo.GetByUserId(userId);
                return Ok(mine is null ? new List<Student>() : new List<Student> { mine });
            }

            if (classId.HasValue)
                return Ok(_repo.GetByClass(classId.Value));

            return Ok(_repo.GetAll(search));
        }

        [HttpGet("{id}")]
        public IActionResult GetOne(int id)
        {
            var student = _repo.GetById(id);
            return student is null ? NotFound() : Ok(student);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IActionResult Create(Student student)
        {
            int newId = _repo.Create(student);
            student.StudentId = newId;
            return CreatedAtAction(nameof(GetOne), new { id = newId }, student);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public IActionResult Update(int id, Student student)
        {
            return _repo.Update(id, student) ? NoContent() : NotFound();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public IActionResult Delete(int id)
        {
            return _repo.Delete(id) ? NoContent() : NotFound();
        }
    }
}
