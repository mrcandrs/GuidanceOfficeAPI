using System.Security.Claims;
using GuidanceOfficeAPI.Data;
using GuidanceOfficeAPI.Dtos;
using GuidanceOfficeAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GuidanceOfficeAPI.Controllers
{
    [ApiController]
    [Route("api/consultation-conference")]
    public class ConsultationConferenceController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly TimeZoneInfo _manilaTimeZone;

        public ConsultationConferenceController(AppDbContext context)
        {
            _context = context;
            _manilaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Manila");
        }

        //Helper method to convert UTC to Manila time
        private DateTime ConvertToManilaTime(DateTime utcDateTime)
        {
            return TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, _manilaTimeZone);
        }

        //Helper method to convert Manila time to UTC for database storage
        private DateTime ConvertToUtc(DateTime manilaDateTime)
        {
            return TimeZoneInfo.ConvertTimeToUtc(manilaDateTime, _manilaTimeZone);
        }

        //Helper method to get current Manila time
        private DateTime GetCurrentManilaTime()
        {
            return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, _manilaTimeZone);
        }

        // GET: api/consultation-conference
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ConsultationConferenceFormDto>>> GetConsultationConferenceForms()
        {
            try
            {
                var formsFromDb = await _context.ConsultationForms
                    .Include(f => f.Student)
                    .Include(f => f.Counselor)
                    .OrderByDescending(f => f.Date)
                    .ToListAsync();

                var forms = formsFromDb.Select(f => new ConsultationConferenceFormDto
                {
                    ConsultationId = f.ConsultationId,
                    StudentId = f.StudentId,
                    CounselorId = f.CounselorId,
                    Date = f.Date,
                    Time = f.Time.HasValue ? f.Time.Value.ToString(@"hh\:mm") : null,
                    GradeYearLevel = f.GradeYearLevel,
                    Section = f.Section,
                    Concerns = f.Concerns,
                    Remarks = f.Remarks,
                    CounselorName = f.CounselorName,
                    ParentGuardian = f.ParentGuardian,
                    SchoolPersonnel = f.SchoolPersonnel,
                    ParentContactNumber = f.ParentContactNumber,
                    CreatedAt = ConvertToManilaTime(f.CreatedAt),
                    UpdatedAt = f.UpdatedAt.HasValue ? ConvertToManilaTime(f.UpdatedAt.Value) : null, // Add this line
                    Student = f.Student != null ? new StudentDto
                    {
                        StudentId = f.Student.StudentId,
                        FullName = f.Student.FullName,
                        StudentNumber = f.Student.StudentNumber
                    } : null,
                    Counselor = f.Counselor != null ? new CounselorDto
                    {
                        CounselorId = f.Counselor.CounselorId,
                        Name = f.Counselor.Name,
                        Email = f.Counselor.Email
                    } : null
                }).ToList();

                return Ok(forms);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error fetching consultation/conference forms", error = ex.Message });
            }
        }

        // GET: api/consultation-conference/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<ConsultationConferenceFormDto>> GetConsultationConferenceForm(int id)
        {
            try
            {
                var formFromDb = await _context.ConsultationForms
                    .Include(f => f.Student)
                    .Include(f => f.Counselor)
                    .Where(f => f.ConsultationId == id)
                    .FirstOrDefaultAsync();

                if (formFromDb == null)
                {
                    return NotFound(new { message = "Consultation/Conference form not found" });
                }

                var form = new ConsultationConferenceFormDto
                {
                    ConsultationId = formFromDb.ConsultationId,
                    StudentId = formFromDb.StudentId,
                    CounselorId = formFromDb.CounselorId,
                    Date = formFromDb.Date,
                    Time = formFromDb.Time.HasValue ? formFromDb.Time.Value.ToString(@"hh\:mm") : null,
                    GradeYearLevel = formFromDb.GradeYearLevel,
                    Section = formFromDb.Section,
                    Concerns = formFromDb.Concerns,
                    Remarks = formFromDb.Remarks,
                    CounselorName = formFromDb.CounselorName,
                    ParentGuardian = formFromDb.ParentGuardian,
                    SchoolPersonnel = formFromDb.SchoolPersonnel,
                    ParentContactNumber = formFromDb.ParentContactNumber,
                    CreatedAt = ConvertToManilaTime(formFromDb.CreatedAt),
                    UpdatedAt = formFromDb.UpdatedAt.HasValue ? ConvertToManilaTime(formFromDb.UpdatedAt.Value) : null, // Add this line
                    Student = formFromDb.Student != null ? new StudentDto
                    {
                        StudentId = formFromDb.Student.StudentId,
                        FullName = formFromDb.Student.FullName,
                        StudentNumber = formFromDb.Student.StudentNumber
                    } : null,
                    Counselor = formFromDb.Counselor != null ? new CounselorDto
                    {
                        CounselorId = formFromDb.Counselor.CounselorId,
                        Name = formFromDb.Counselor.Name,
                        Email = formFromDb.Counselor.Email
                    } : null
                };

                return Ok(form);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error fetching consultation/conference form", error = ex.Message });
            }
        }

        // POST: api/consultation-conference
        [HttpPost]
        public async Task<ActionResult<ConsultationConferenceFormDto>> CreateConsultationConferenceForm(CreateConsultationConferenceFormDto createDto)
        {
            try
            {
                // Get counselor ID from JWT token
                var counselorIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(counselorIdClaim) || !int.TryParse(counselorIdClaim, out int counselorId))
                {
                    return Unauthorized(new { message = "Invalid counselor authentication" });
                }

                // Verify student exists
                var studentExists = await _context.Students.AnyAsync(s => s.StudentId == createDto.StudentId);
                if (!studentExists)
                {
                    return BadRequest(new { message = "Student not found" });
                }

                var form = new ConsultationForm
                {
                    StudentId = createDto.StudentId,
                    CounselorId = counselorId,
                    Date = createDto.Date,
                    GradeYearLevel = createDto.GradeYearLevel,
                    Section = createDto.Section,
                    Concerns = createDto.Concerns,
                    Remarks = createDto.Remarks,
                    CounselorName = createDto.CounselorName,
                    ParentGuardian = createDto.ParentGuardian,
                    SchoolPersonnel = createDto.SchoolPersonnel,
                    ParentContactNumber = createDto.ParentContactNumber,
                    CreatedAt = DateTime.UtcNow
                };

                // Parse the string time into TimeSpan (if valid)
                if (!string.IsNullOrEmpty(createDto.Time))
                {
                    if (TimeSpan.TryParse(createDto.Time, out var parsedTime))
                    {
                        form.Time = parsedTime;
                    }
                    else
                    {
                        return BadRequest("Invalid time format. Use HH:mm or HH:mm:ss.");
                    }
                }

                _context.ConsultationForms.Add(form);
                await _context.SaveChangesAsync();

                // Fetch the created form with related data
                var createdFormFromDb = await _context.ConsultationForms
                    .Include(f => f.Student)
                    .Include(f => f.Counselor)
                    .Where(f => f.ConsultationId == form.ConsultationId)
                    .FirstOrDefaultAsync();

                var createdForm = new ConsultationConferenceFormDto
                {
                    ConsultationId = createdFormFromDb.ConsultationId,
                    StudentId = createdFormFromDb.StudentId,
                    CounselorId = createdFormFromDb.CounselorId,
                    Date = createdFormFromDb.Date,
                    Time = createdFormFromDb.Time.HasValue ? createdFormFromDb.Time.Value.ToString(@"hh\:mm") : null,
                    GradeYearLevel = createdFormFromDb.GradeYearLevel,
                    Section = createdFormFromDb.Section,
                    Concerns = createdFormFromDb.Concerns,
                    Remarks = createdFormFromDb.Remarks,
                    CounselorName = createdFormFromDb.CounselorName,
                    ParentGuardian = createdFormFromDb.ParentGuardian,
                    SchoolPersonnel = createdFormFromDb.SchoolPersonnel,
                    ParentContactNumber = createdFormFromDb.ParentContactNumber,
                    CreatedAt = ConvertToManilaTime(createdFormFromDb.CreatedAt),
                    Student = createdFormFromDb.Student != null ? new StudentDto
                    {
                        StudentId = createdFormFromDb.Student.StudentId,
                        FullName = createdFormFromDb.Student.FullName,
                        StudentNumber = createdFormFromDb.Student.StudentNumber
                    } : null,
                    Counselor = createdFormFromDb.Counselor != null ? new CounselorDto
                    {
                        CounselorId = createdFormFromDb.Counselor.CounselorId,
                        Name = createdFormFromDb.Counselor.Name,
                        Email = createdFormFromDb.Counselor.Email
                    } : null
                };

                return CreatedAtAction(nameof(GetConsultationConferenceForm),
                    new { id = form.ConsultationId }, createdForm);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error creating consultation/conference form", error = ex.Message });
            }
        }

        // PUT: api/consultation-conference/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateConsultationConferenceForm(int id, UpdateConsultationConferenceFormDto updateDto)
        {
            try
            {
                // Get counselor ID from JWT token
                var counselorIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(counselorIdClaim) || !int.TryParse(counselorIdClaim, out int counselorId))
                {
                    return Unauthorized(new { message = "Invalid counselor authentication" });
                }

                var form = await _context.ConsultationForms.FindAsync(id);
                if (form == null)
                {
                    return NotFound(new { message = "Consultation/Conference form not found" });
                }

                // Verify the form belongs to the authenticated counselor
                if (form.CounselorId != counselorId)
                {
                    return Forbid("You can only update your own forms");
                }

                // Verify student exists if studentId is being changed
                if (form.StudentId != updateDto.StudentId)
                {
                    var studentExists = await _context.Students.AnyAsync(s => s.StudentId == updateDto.StudentId);
                    if (!studentExists)
                    {
                        return BadRequest(new { message = "Student not found" });
                    }
                }

                // Update form properties
                form.StudentId = updateDto.StudentId;
                form.Date = updateDto.Date;
                form.GradeYearLevel = updateDto.GradeYearLevel;
                form.Section = updateDto.Section;
                form.Concerns = updateDto.Concerns;
                form.Remarks = updateDto.Remarks;
                form.CounselorName = updateDto.CounselorName;
                form.ParentGuardian = updateDto.ParentGuardian;
                form.SchoolPersonnel = updateDto.SchoolPersonnel;
                form.ParentContactNumber = updateDto.ParentContactNumber;
                form.UpdatedAt = DateTime.UtcNow; // Set the UpdatedAt timestamp

                // Parse the string time into TimeSpan (if valid)
                if (!string.IsNullOrEmpty(updateDto.Time))
                {
                    if (TimeSpan.TryParse(updateDto.Time, out var parsedTime))
                    {
                        form.Time = parsedTime;
                    }
                    else
                    {
                        return BadRequest("Invalid time format. Use HH:mm or HH:mm:ss.");
                    }
                }

                await _context.SaveChangesAsync();

                // Return success response with message
                return Ok(new
                {
                    message = "Consultation/Conference form updated successfully.",
                    updatedAt = ConvertToManilaTime(form.UpdatedAt.Value),
                    consultationId = form.ConsultationId
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error updating consultation/conference form", error = ex.Message });
            }
        }

        // DELETE: api/consultation-conference/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteConsultationConferenceForm(int id)
        {
            try
            {
                // Get counselor ID from JWT token
                var counselorIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(counselorIdClaim) || !int.TryParse(counselorIdClaim, out int counselorId))
                {
                    return Unauthorized(new { message = "Invalid counselor authentication" });
                }

                var form = await _context.ConsultationForms.FindAsync(id);
                if (form == null)
                {
                    return NotFound(new { message = "Consultation/Conference form not found" });
                }

                // Verify the form belongs to the authenticated counselor
                if (form.CounselorId != counselorId)
                {
                    return Forbid("You can only delete your own forms");
                }

                _context.ConsultationForms.Remove(form);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error deleting consultation/conference form", error = ex.Message });
            }
        }

        // GET: api/consultation-conference/student/{studentId}
        [HttpGet("student/{studentId}")]
        public async Task<ActionResult<IEnumerable<ConsultationConferenceFormDto>>> GetConsultationConferenceFormsByStudent(int studentId)
        {
            try
            {
                var formsFromDb = await _context.ConsultationForms
                    .Include(f => f.Student)
                    .Include(f => f.Counselor)
                    .Where(f => f.StudentId == studentId)
                    .OrderByDescending(f => f.Date)
                    .ToListAsync();

                var forms = formsFromDb.Select(f => new ConsultationConferenceFormDto
                {
                    ConsultationId = f.ConsultationId,
                    StudentId = f.StudentId,
                    CounselorId = f.CounselorId,
                    Date = f.Date,
                    Time = f.Time.HasValue ? f.Time.Value.ToString(@"hh\:mm") : null,
                    GradeYearLevel = f.GradeYearLevel,
                    Section = f.Section,
                    Concerns = f.Concerns,
                    Remarks = f.Remarks,
                    CounselorName = f.CounselorName,
                    ParentGuardian = f.ParentGuardian,
                    SchoolPersonnel = f.SchoolPersonnel,
                    ParentContactNumber = f.ParentContactNumber,
                    CreatedAt = ConvertToManilaTime(f.CreatedAt),
                    UpdatedAt = f.UpdatedAt.HasValue ? ConvertToManilaTime(f.UpdatedAt.Value) : null, // Add this line
                    Student = f.Student != null ? new StudentDto
                    {
                        StudentId = f.Student.StudentId,
                        FullName = f.Student.FullName,
                        StudentNumber = f.Student.StudentNumber
                    } : null,
                    Counselor = f.Counselor != null ? new CounselorDto
                    {
                        CounselorId = f.Counselor.CounselorId,
                        Name = f.Counselor.Name,
                        Email = f.Counselor.Email
                    } : null
                }).ToList();

                return Ok(forms);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error fetching student's consultation/conference forms", error = ex.Message });
            }
        }

        // GET: api/consultation-conference/student-details/{studentId}
        [HttpGet("student-details/{studentId}")]
        public async Task<ActionResult<object>> GetStudentDetailsFromCareerPlan(int studentId)
        {
            try
            {
                // First, try to get details from the most recent Career Planning Form
                var careerPlanDetails = await _context.CareerPlanningForms
                    .Where(c => c.StudentId == studentId)
                    .OrderByDescending(c => c.SubmittedAt)
                    .Select(c => new {
                        GradeYear = c.GradeYear,
                        Section = c.Section,
                        Program = c.Program,
                        FullName = c.FullName
                    })
                    .FirstOrDefaultAsync();

                if (careerPlanDetails != null)
                {
                    return Ok(new
                    {
                        gradeYearLevel = careerPlanDetails.GradeYear,
                        section = careerPlanDetails.Section,
                        program = careerPlanDetails.Program,
                        fullName = careerPlanDetails.FullName,
                        source = "CareerPlanningForm"
                    });
                }

                // Fallback to Student table if no Career Planning Form exists
                var studentDetails = await _context.Students
                    .Where(s => s.StudentId == studentId)
                    .Select(s => new {
                        GradeYear = s.GradeYear,
                        Program = s.Program,
                        FullName = s.FullName
                    })
                    .FirstOrDefaultAsync();

                if (studentDetails != null)
                {
                    return Ok(new
                    {
                        gradeYearLevel = studentDetails.GradeYear,
                        section = "", // Student table doesn't have section
                        program = studentDetails.Program,
                        fullName = studentDetails.FullName,
                        source = "StudentTable"
                    });
                }

                return NotFound(new { message = "Student details not found" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error fetching student details", error = ex.Message });
            }
        }

        // GET: api/consultation-conference/current-counselor
        [HttpGet("current-counselor")]
        public async Task<ActionResult<object>> GetCurrentCounselorDetails()
        {
            try
            {
                // Get counselor ID from JWT token
                var counselorIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(counselorIdClaim) || !int.TryParse(counselorIdClaim, out int counselorId))
                {
                    return Unauthorized(new { message = "Invalid counselor authentication" });
                }

                var counselor = await _context.Counselors
                    .Where(c => c.CounselorId == counselorId)
                    .Select(c => new {
                        CounselorId = c.CounselorId,
                        Name = c.Name,
                        Email = c.Email
                    })
                    .FirstOrDefaultAsync();

                if (counselor != null)
                {
                    return Ok(new
                    {
                        counselorId = counselor.CounselorId,
                        name = counselor.Name,
                        email = counselor.Email
                    });
                }

                return NotFound(new { message = "Counselor not found" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error fetching counselor details", error = ex.Message });
            }
        }
    }
}
