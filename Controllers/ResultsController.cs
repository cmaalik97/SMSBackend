using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Student_Management_System.Models;
using Student_Management_System.Repositories;
using System.Security.Claims;

namespace Student_Management_System.Controllers
{
    [ApiController]
    [Route("api/results")]
    [Authorize]

    public class ResultsController : ControllerBase
    {
        private readonly ResultRepository _repo;
        private readonly StudentRepository _students;
        public ResultsController(ResultRepository repo, StudentRepository students)
        {
            _repo = repo;
            _students = students;
        }

        // GET api/results?examType=Final -> "filter by exam type" dropdown
        // A Student only ever sees their own results.
        [HttpGet]
        public IActionResult GetAll([FromQuery] string? examType)
        {
            var role = User.FindFirstValue(ClaimTypes.Role);
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            if (role == "Student")
            {
                var student = _students.GetByUserId(userId);
                if (student is null) return Ok(new List<Result>());
                return Ok(_repo.GetByStudentId(student.StudentId));
            }

            return Ok(_repo.GetAll(examType));
        }

        [HttpGet("{id}")]
        public IActionResult GetOne(int id)
        {
            var r = _repo.GetById(id);
            return r is null ? NotFound() : Ok(r);
        }

        // Admin AND Teacher can enter results (teachers for their own subjects)
        [HttpPost]
        [Authorize(Roles = "Admin,Teacher")]
        public IActionResult Create(Result res)
        {
            try
            {
                int newId = _repo.Create(res);
                res.ResultId = newId;
                return CreatedAtAction(nameof(GetOne), new { id = newId }, res);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Teacher")]
        public IActionResult Update(int id, Result res) => _repo.Update(id, res) ? NoContent() : NotFound();

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public IActionResult Delete(int id) => _repo.Delete(id) ? NoContent() : NotFound();
    }
}
