using GuidanceOfficeAPI.Data;
using GuidanceOfficeAPI.Dtos;
using GuidanceOfficeAPI.Models;
using Microsoft.AspNetCore.Mvc;

namespace GuidanceOfficeAPI.Controllers
{
    [Route("api/referral")]
    [ApiController]
    public class ReferralController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ReferralController(AppDbContext context)
        {
            _context = context;
        }

        //POST: api/referral/submit-referral
        [HttpPost("submit-referral")]
        public IActionResult SubmitReferral([FromBody] ReferralForm form)
        {
            if (form == null)
            {
                return BadRequest(new { message = "Referral form is null" });
            }


            // Optional: Check if StudentId is valid
            var studentExists = _context.Students.Any(s => s.StudentId == form.StudentId);
            if (!studentExists)
            {
                return BadRequest(new { message = "Student ID does not exist." });
            }

            // ✅ Validate model binding
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return BadRequest(new { message = "Model validation failed", errors });
            }

            form.SubmissionDate = DateTime.Now;

            try
            {
                _context.ReferralForms.Add(form);
                _context.SaveChanges();
                return Ok(new { message = "Referral form submitted successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while saving the form.", error = ex.Message });
            }
        }

        //GET: api/referral/student/{studentId}/latest
        [HttpGet("student/{studentId}/latest")]
        public IActionResult GetLatestReferralForStudent(int studentId)
        {
            var latestReferral = _context.ReferralForms
                .Where(r => r.StudentId == studentId)
                .OrderByDescending(r => r.SubmissionDate)
                .FirstOrDefault();

            if (latestReferral == null)
                return NotFound(new { message = "No referral form found for this student." });

            return Ok(latestReferral);
        }

        //PUT
        [HttpPut("{referralId}/feedback")]
        public IActionResult UpdateFeedback(int referralId, [FromBody] ReferralFeedbackDto dto)
        {
            var form = _context.ReferralForms.FirstOrDefault(r => r.ReferralId == referralId);
            if (form == null) return NotFound(new { message = "Referral not found." });

            form.CounselorFeedbackStudentName = dto.CounselorFeedbackStudentName ?? form.CounselorFeedbackStudentName;
            form.CounselorFeedbackDateReferred = dto.CounselorFeedbackDateReferred ?? form.CounselorFeedbackDateReferred;
            form.CounselorSessionDate = dto.CounselorSessionDate ?? form.CounselorSessionDate;
            form.CounselorActionsTaken = dto.CounselorActionsTaken ?? form.CounselorActionsTaken;
            form.CounselorName = dto.CounselorName ?? form.CounselorName;

            _context.SaveChanges();
            return Ok(new { message = "Feedback saved." });
        }

        //Optional convenience endpoint if you prefer “latest for student”:
        [HttpGet("latest-per-student")]
        public IActionResult GetLatestPerStudent()
        {
            var forms = _context.ReferralForms
              .GroupBy(r => r.StudentId)
              .Select(g => g.OrderByDescending(r => r.SubmissionDate).First())
              .OrderByDescending(r => r.SubmissionDate)
              .ToList();
            return Ok(forms);
        }
    }
}
