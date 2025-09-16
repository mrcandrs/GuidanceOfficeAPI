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

            form.CounselorFeedbackStudentName = string.IsNullOrWhiteSpace(dto.CounselorFeedbackStudentName)
            ? form.FullName
            : dto.CounselorFeedbackStudentName;
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
            var studentIds = _context.ReferralForms
                .Select(r => r.StudentId)
                .Distinct()
                .ToList();

            var latest = new List<ReferralForm>();

            foreach (var studentId in studentIds)
            {
                var latestForStudent = _context.ReferralForms
                    .Where(r => r.StudentId == studentId)
                    .OrderByDescending(r => r.SubmissionDate)
                    .FirstOrDefault();

                if (latestForStudent != null)
                {
                    latest.Add(latestForStudent);
                }
            }

            // Get student and career planning data
            var studentData = _context.Students
                .Where(s => studentIds.Contains(s.StudentId))
                .Join(_context.CareerPlanningForms,
                      s => s.StudentId,
                      c => c.StudentId,
                      (s, c) => new {
                          StudentId = s.StudentId,
                          FullName = s.FullName,
                          StudentNumber = s.StudentNumber,
                          Program = s.Program,
                          Section = c.Section
                      })
                .ToDictionary(x => x.StudentId);

            var result = latest
                .OrderByDescending(r => r.SubmissionDate)
                .Select(r =>
                {
                    studentData.TryGetValue(r.StudentId, out var studentInfo);
                    return new ReferralLatestDto
                    {
                        ReferralId = r.ReferralId,
                        StudentId = r.StudentId,
                        SubmissionDate = r.SubmissionDate,

                        StudentFullName = studentInfo?.FullName ?? r.FullName,
                        StudentNumber = studentInfo?.StudentNumber ?? r.StudentNumber,
                        Program = studentInfo?.Program ?? r.Program,
                        Section = studentInfo?.Section ?? string.Empty,

                        FullName = r.FullName,
                        PersonWhoReferred = r.PersonWhoReferred,
                        DateReferred = r.DateReferred,
                        CounselorFeedbackStudentName = r.CounselorFeedbackStudentName,
                        CounselorFeedbackDateReferred = r.CounselorFeedbackDateReferred,
                        CounselorSessionDate = r.CounselorSessionDate,
                        CounselorActionsTaken = r.CounselorActionsTaken,
                        CounselorName = r.CounselorName
                    };
                })
                .ToList();

            return Ok(result);
        }
    }
}
