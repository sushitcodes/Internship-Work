using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Project1_backend_.Data;
using Project1_backend_.Models;
using Project1_backend_.DTOs;

namespace Project1_backend_.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class SubmissionsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public SubmissionsController(ApplicationDbContext context)
        {
            _context = context;
        }
        // GET: api/submissions

        [HttpGet]
        public async Task<ActionResult<IEnumerable<SubmissionResponseDto>>> GetSubmissions()
        {
            var submissions = await _context.Submissions
                .Include(s => s.Education)
                .OrderByDescending(s => s.SubmittedAt)
                .ToListAsync();

            // Convert to DTOs
            var response = submissions.Select(s => new SubmissionResponseDto
            {
                Id = s.Id,
                Name = s.Name,
                Email = s.Email,
                Message = s.Message,
                SubmittedAt = s.SubmittedAt,
                Education = s.Education?.Select(e => new EducationResponseDto
                {
                    Id = e.Id,
                    Degree = e.Degree,
                    Year = e.Year,
                    School = e.School
                }).ToList() ?? new()
            });

            return Ok(response);
        }
        // GET: api/submissions

        [HttpGet("{id}")]
        public async Task<ActionResult<SubmissionResponseDto>> GetSubmission(int id)
        {
            var submission = await _context.Submissions
                .Include(s => s.Education)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (submission == null)
            {
                return NotFound(new { message = $"Submission with ID {id} not found" });
            }

            var response = new SubmissionResponseDto
            {
                Id = submission.Id,
                Name = submission.Name,
                Email = submission.Email,
                Message = submission.Message,
                SubmittedAt = submission.SubmittedAt,
                Education = submission.Education?.Select(e => new EducationResponseDto
                {
                    Id = e.Id,
                    Degree = e.Degree,
                    Year = e.Year,
                    School = e.School
                }).ToList() ?? new()
            };

            return Ok(response);
        }

        // POST: api/submissions
        [HttpPost]
        public async Task<ActionResult<SubmissionResponseDto>> PostSubmission(SubmissionDto submissionDto)
        {
            // Create new submission
            var submission = new Submission
            {
                Name = submissionDto.Name,
                Email = submissionDto.Email,
                Message = submissionDto.Message,
                SubmittedAt = DateTime.UtcNow,
                Education = submissionDto.Education?.Select(e => new Education
                {
                    Degree = e.Degree,
                    Year = e.Year,
                    School = e.School
                }).ToList() ?? new()
            };
            // Save to database
            _context.Submissions.Add(submission);
            await _context.SaveChangesAsync();

            var response = new SubmissionResponseDto
            {
                Id = submission.Id,
                Name = submission.Name,
                Email = submission.Email,
                Message = submission.Message,
                SubmittedAt = submission.SubmittedAt,
                Education = submission.Education?.Select(e => new EducationResponseDto
                {
                    Id = e.Id,
                    Degree = e.Degree,
                    Year = e.Year,
                    School = e.School
                }).ToList() ?? new()
            };
            return CreatedAtAction(nameof(GetSubmission), new { id = submission.Id }, response);
        }

        // DELETE: api/submissions

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSubmission(int id)
        {
            var submission = await _context.Submissions.FindAsync(id);
            if (submission == null)
            {
                return NotFound(new { message = $"Submission with ID {id} not found" });
            }

            _context.Submissions.Remove(submission);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Submission deleted successfully" });
        }
    }
}