using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Student_Management_System.Models;
using Student_Management_System.Repositories;
using System.Security.Claims;

namespace Student_Management_System.Controllers
{
    [ApiController]
    [Route("api/attendance")]
    [Authorize]

    public class AttendanceController : ControllerBase
    {
        private readonly AttendanceRepository _repo;
        private readonly StudentRepository _students;
        private readonly TeacherRepository _teachers;   // ← added
        public AttendanceController(AttendanceRepository repo, StudentRepository students, TeacherRepository teachers)
        {
            _repo = repo;
            _students = students;
            _teachers = teachers;   // ← added
        }

        // GET api/attendance?status=Present  -> the "filter by status" dropdown
        // A Student only ever gets their own attendance history.
        [HttpGet]
        public IActionResult GetAll([FromQuery] string? status)
        {
            var role = User.FindFirstValue(ClaimTypes.Role);
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            if (role == "Student")
            {
                var student = _students.GetByUserId(userId);
                if (student is null) return Ok(new List<Attendance>());
                return Ok(_repo.GetByStudentId(student.StudentId));
            }

            return Ok(_repo.GetAll(status));
        }

        [HttpGet("{id}")]
        public IActionResult GetOne(int id)
        {
            var a = _repo.GetById(id);
            return a is null ? NotFound() : Ok(a);
        }

        // Admin AND Teacher can mark attendance (teachers mark their own class)
        [HttpPost]
        [Authorize(Roles = "Admin,Teacher")]
        public IActionResult Create(Attendance a)
        {
            var role = User.FindFirstValue(ClaimTypes.Role);
            if (role == "Teacher")
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var teacher = _teachers.GetByUserId(userId);
                if (teacher != null) a.MarkedByTeacherId = teacher.TeacherId;
            }
            try
            {
                int newId = _repo.Create(a);
                a.AttendanceId = newId;
                return CreatedAtAction(nameof(GetOne), new { id = newId }, a);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Teacher")]
        public IActionResult Update(int id, Attendance a) => _repo.Update(id, a) ? NoContent() : NotFound();

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public IActionResult Delete(int id) => _repo.Delete(id) ? NoContent() : NotFound();

    }
}
