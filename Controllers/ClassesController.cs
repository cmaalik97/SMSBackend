using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Student_Management_System.Models;
using Student_Management_System.Repositories;

namespace Student_Management_System.Controllers
{
    [ApiController]
    [Route("api/classes")]
    [Authorize] // any logged-in role can VIEW classes (e.g. for dropdowns)

    public class ClassesController : ControllerBase
    {
        private readonly ClassRepository _repo;
        public ClassesController(ClassRepository repo) => _repo = repo;

        [HttpGet]
        public IActionResult GetAll([FromQuery] string? search) => Ok(_repo.GetAll(search));

        [HttpGet("{id}")]
        public IActionResult GetOne(int id)
        {
            var c = _repo.GetById(id);
            return c is null ? NotFound() : Ok(c);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IActionResult Create(ClassRoom c)
        {
            int newId = _repo.Create(c);
            c.ClassId = newId;
            return CreatedAtAction(nameof(GetOne), new { id = newId }, c);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public IActionResult Update(int id, ClassRoom c) => _repo.Update(id, c) ? NoContent() : NotFound();

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public IActionResult Delete(int id) => _repo.Delete(id) ? NoContent() : NotFound();
    }
}
