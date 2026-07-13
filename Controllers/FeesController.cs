using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Student_Management_System.Models;
using Student_Management_System.Repositories;
using System.Security.Claims;

namespace Student_Management_System.Controllers
{
    [ApiController]
    [Route("api/fees")]
    [Authorize]

    public class FeesController : ControllerBase
    {
        private readonly FeeRepository _repo;
        private readonly StudentRepository _students;
        public FeesController(FeeRepository repo, StudentRepository students)
        {
            _repo = repo;
            _students = students;
        }

        // GET api/fees?status=Unpaid -> "filter by status" dropdown
        // A Student only ever sees their own fee balance.
        [HttpGet]
        public IActionResult GetAll([FromQuery] string? status)
        {
            var role = User.FindFirstValue(ClaimTypes.Role);
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            if (role == "Student")
            {
                var student = _students.GetByUserId(userId);
                if (student is null) return Ok(new List<Fee>());
                return Ok(_repo.GetByStudentId(student.StudentId));
            }

            return Ok(_repo.GetAll(status));
        }

        [HttpGet("{id}")]
        public IActionResult GetOne(int id)
        {
            var f = _repo.GetById(id);
            return f is null ? NotFound() : Ok(f);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IActionResult Create(Fee f)
        {
            try
            {
                int newId = _repo.Create(f);
                f.FeeId = newId;
                return CreatedAtAction(nameof(GetOne), new { id = newId }, f);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public IActionResult Update(int id, Fee f) => _repo.Update(id, f) ? NoContent() : NotFound();

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public IActionResult Delete(int id) => _repo.Delete(id) ? NoContent() : NotFound();
    }
}
