using H1V1.Models.Entities;
using H1V1.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace H1V1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AssessmentController : ControllerBase
    {
        private readonly IAcademicRepository _repo;

        public AssessmentController(IAcademicRepository repo)
        {
            _repo = repo;
        }

        // GET: api/assessment/pending/1
        [HttpGet("pending/{studentId}")]
        public async Task<IActionResult> GetPendingAssessments(int studentId)
        {
            var assessments = await _repo.GetPendingAssessmentsAsync(studentId);
            return Ok(assessments);
        }

        // POST: api/assessment
        [HttpPost]
        public async Task<IActionResult> AddAssessment([FromBody] Assessment assessment)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var created = await _repo.AddAssessmentAsync(assessment);
            return Ok(created);
        }

        // POST: api/assessment/complete/5
        [HttpPost("complete/{id}")]
        public async Task<IActionResult> MarkComplete(int id)
        {
            var success = await _repo.MarkAssessmentCompleteAsync(id);
            if (!success) return NotFound(new { message = "Assessment not found" });

            return Ok(new { success = true });
        }
    }
}