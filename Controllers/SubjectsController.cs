using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Student_Management_System.Repositories;
using System.Security.Claims;

namespace Student_Management_System.Controllers
{
    [ApiController]
    [Route("api/subjects")]
    [Authorize]

    public class SubjectsController : ControllerBase
    {
        private readonly SubjectRepository _repo;
        private readonly TeacherRepository _teachers;
        public SubjectsController(SubjectRepository repo, TeacherRepository teachers)
        {
            _repo = repo;
            _teachers = teachers;
        }

        // A Teacher only sees the subjects assigned to them ("My Subjects").
        [HttpGet]
        public IActionResult GetAll([FromQuery] string? search)
        {
            var role = User.FindFirstValue(ClaimTypes.Role);
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            if (role == "Teacher")
            {
                var teacher = _teachers.GetByUserId(userId);
                if (teacher is null) return Ok(new List<Student_Management_System.Models.Subject>());
                return Ok(_repo.GetByTeacherId(teacher.TeacherId));
            }

            return Ok(_repo.GetAll(search));
        }

        [HttpGet("{id}")]
        public IActionResult GetOne(int id)
        {
            var s = _repo.GetById(id);
            return s is null ? NotFound() : Ok(s);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IActionResult Create(Student_Management_System.Models.Subject s)
        {
            int newId = _repo.Create(s);
            s.SubjectId = newId;
            return CreatedAtAction(nameof(GetOne), new { id = newId }, s);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public IActionResult Update(int id, Student_Management_System.Models.Subject s) => _repo.Update(id, s) ? NoContent() : NotFound();

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public IActionResult Delete(int id) => _repo.Delete(id) ? NoContent() : NotFound();
    }
}
