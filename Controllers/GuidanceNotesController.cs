using GuidanceOfficeAPI.Data;
using GuidanceOfficeAPI.Dtos;
using GuidanceOfficeAPI.Models;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using GuidanceOfficeAPI.Services;

namespace GuidanceOfficeAPI.Controllers
{
    [ApiController]
    [Route("api/guidance-notes")]
    public class GuidanceNotesController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly TimeZoneInfo _manilaTimeZone;
        private readonly IActivityLogger _activityLogger;

        public GuidanceNotesController(AppDbContext context, IActivityLogger activityLogger)
        {
            _context = context;
            _manilaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Manila");
            _activityLogger = activityLogger;
        }

        // Helper method to convert UTC to Manila time
        private DateTime ConvertToManilaTime(DateTime utcDateTime)
        {
            return TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, _manilaTimeZone);
        }

        // Helper method to convert Manila time to UTC for database storage
        private DateTime ConvertToUtc(DateTime manilaDateTime)
        {
            return TimeZoneInfo.ConvertTimeToUtc(manilaDateTime, _manilaTimeZone);
        }

        // Helper method to get current Manila time
        private DateTime GetCurrentManilaTime()
        {
            return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, _manilaTimeZone);
        }

        // GET: api/guidance-notes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<GuidanceNoteDto>>> GetGuidanceNotes()
        {
            try
            {
                var notesFromDb = await _context.GuidanceNotes
                    .Include(n => n.Student)
                    .Include(n => n.Counselor)
                    .OrderByDescending(n => n.InterviewDate)
                    .ToListAsync();

                var notes = notesFromDb.Select(n => new GuidanceNoteDto
                {
                    NoteId = n.NoteId,
                    StudentId = n.StudentId,
                    CounselorId = n.CounselorId,
                    InterviewDate = n.InterviewDate,
                    TimeStarted = n.TimeStarted?.ToString(@"hh\:mm"),
                    TimeEnded = n.TimeEnded?.ToString(@"hh\:mm"),
                    SchoolYear = n.SchoolYear,
                    TertiarySemester = n.TertiarySemester,
                    SeniorHighQuarter = n.SeniorHighQuarter,
                    GradeYearLevelSection = n.GradeYearLevelSection,
                    Program = n.Program,
                    // Nature of Counseling
                    IsAcademic = n.IsAcademic,
                    IsBehavioral = n.IsBehavioral,
                    IsPersonal = n.IsPersonal,
                    IsSocial = n.IsSocial,
                    IsCareer = n.IsCareer,
                    // Counseling Situation
                    IsIndividual = n.IsIndividual,
                    IsGroup = n.IsGroup,
                    IsClass = n.IsClass,
                    IsCounselorInitiated = n.IsCounselorInitiated,
                    IsWalkIn = n.IsWalkIn,
                    IsFollowUp = n.IsFollowUp,
                    ReferredBy = n.ReferredBy,
                    // Notes sections
                    PresentingProblem = n.PresentingProblem,
                    Assessment = n.Assessment,
                    Interventions = n.Interventions,
                    PlanOfAction = n.PlanOfAction,
                    // Recommendations
                    IsFollowThroughSession = n.IsFollowThroughSession,
                    FollowThroughDate = n.FollowThroughDate,
                    IsReferral = n.IsReferral,
                    ReferralAgencyName = n.ReferralAgencyName,
                    // Counselor name (new field)
                    CounselorName = n.CounselorName,
                    // Metadata
                    CreatedAt = ConvertToManilaTime(n.CreatedAt),
                    UpdatedAt = n.UpdatedAt.HasValue ? ConvertToManilaTime(n.UpdatedAt.Value) : null,
                    Student = n.Student != null ? new StudentDto
                    {
                        StudentId = n.Student.StudentId,
                        FullName = n.Student.FullName,
                        StudentNumber = n.Student.StudentNumber
                    } : null,
                    Counselor = n.Counselor != null ? new CounselorDto
                    {
                        CounselorId = n.Counselor.CounselorId,
                        Name = n.Counselor.Name,
                        Email = n.Counselor.Email
                    } : null
                }).ToList();

                return Ok(notes);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error fetching guidance notes", error = ex.Message });
            }
        }

        // GET: api/guidance-notes/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<GuidanceNoteDto>> GetGuidanceNote(int id)
        {
            try
            {
                var noteFromDb = await _context.GuidanceNotes
                    .Include(n => n.Student)
                    .Include(n => n.Counselor)
                    .Where(n => n.NoteId == id)
                    .FirstOrDefaultAsync();

                if (noteFromDb == null)
                {
                    return NotFound(new { message = "Guidance note not found" });
                }

                var note = new GuidanceNoteDto
                {
                    NoteId = noteFromDb.NoteId,
                    StudentId = noteFromDb.StudentId,
                    CounselorId = noteFromDb.CounselorId,
                    InterviewDate = noteFromDb.InterviewDate,
                    TimeStarted = noteFromDb.TimeStarted?.ToString(@"hh\:mm"),
                    TimeEnded = noteFromDb.TimeEnded?.ToString(@"hh\:mm"),
                    SchoolYear = noteFromDb.SchoolYear,
                    TertiarySemester = noteFromDb.TertiarySemester,
                    SeniorHighQuarter = noteFromDb.SeniorHighQuarter,
                    GradeYearLevelSection = noteFromDb.GradeYearLevelSection,
                    Program = noteFromDb.Program,
                    // Nature of Counseling
                    IsAcademic = noteFromDb.IsAcademic,
                    IsBehavioral = noteFromDb.IsBehavioral,
                    IsPersonal = noteFromDb.IsPersonal,
                    IsSocial = noteFromDb.IsSocial,
                    IsCareer = noteFromDb.IsCareer,
                    // Counseling Situation
                    IsIndividual = noteFromDb.IsIndividual,
                    IsGroup = noteFromDb.IsGroup,
                    IsClass = noteFromDb.IsClass,
                    IsCounselorInitiated = noteFromDb.IsCounselorInitiated,
                    IsWalkIn = noteFromDb.IsWalkIn,
                    IsFollowUp = noteFromDb.IsFollowUp,
                    ReferredBy = noteFromDb.ReferredBy,
                    // Notes sections
                    PresentingProblem = noteFromDb.PresentingProblem,
                    Assessment = noteFromDb.Assessment,
                    Interventions = noteFromDb.Interventions,
                    PlanOfAction = noteFromDb.PlanOfAction,
                    // Recommendations
                    IsFollowThroughSession = noteFromDb.IsFollowThroughSession,
                    FollowThroughDate = noteFromDb.FollowThroughDate,
                    IsReferral = noteFromDb.IsReferral,
                    ReferralAgencyName = noteFromDb.ReferralAgencyName,
                    // Counselor name (new field)
                    CounselorName = noteFromDb.CounselorName,
                    // Metadata
                    CreatedAt = ConvertToManilaTime(noteFromDb.CreatedAt),
                    UpdatedAt = noteFromDb.UpdatedAt.HasValue ? ConvertToManilaTime(noteFromDb.UpdatedAt.Value) : null,
                    Student = noteFromDb.Student != null ? new StudentDto
                    {
                        StudentId = noteFromDb.Student.StudentId,
                        FullName = noteFromDb.Student.FullName,
                        StudentNumber = noteFromDb.Student.StudentNumber
                    } : null,
                    Counselor = noteFromDb.Counselor != null ? new CounselorDto
                    {
                        CounselorId = noteFromDb.Counselor.CounselorId,
                        Name = noteFromDb.Counselor.Name,
                        Email = noteFromDb.Counselor.Email
                    } : null
                };

                return Ok(note);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error fetching guidance note", error = ex.Message });
            }
        }

        // POST: api/guidance-notes
        [HttpPost]
        public async Task<ActionResult<GuidanceNoteDto>> CreateGuidanceNote(CreateGuidanceNoteDto createDto)
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

                // Get counselor details for name
                var counselor = await _context.Counselors.FindAsync(counselorId);
                if (counselor == null)
                {
                    return BadRequest(new { message = "Counselor not found" });
                }

                var note = new GuidanceNote
                {
                    StudentId = createDto.StudentId,
                    CounselorId = counselorId,
                    InterviewDate = createDto.InterviewDate,
                    SchoolYear = createDto.SchoolYear,
                    TertiarySemester = createDto.TertiarySemester,
                    SeniorHighQuarter = createDto.SeniorHighQuarter,
                    GradeYearLevelSection = createDto.GradeYearLevelSection,
                    Program = createDto.Program,
                    // Nature of Counseling
                    IsAcademic = createDto.IsAcademic,
                    IsBehavioral = createDto.IsBehavioral,
                    IsPersonal = createDto.IsPersonal,
                    IsSocial = createDto.IsSocial,
                    IsCareer = createDto.IsCareer,
                    // Counseling Situation
                    IsIndividual = createDto.IsIndividual,
                    IsGroup = createDto.IsGroup,
                    IsClass = createDto.IsClass,
                    IsCounselorInitiated = createDto.IsCounselorInitiated,
                    IsWalkIn = createDto.IsWalkIn,
                    IsFollowUp = createDto.IsFollowUp,
                    ReferredBy = createDto.ReferredBy,
                    // Notes sections
                    PresentingProblem = createDto.PresentingProblem,
                    Assessment = createDto.Assessment,
                    Interventions = createDto.Interventions,
                    PlanOfAction = createDto.PlanOfAction,
                    // Recommendations
                    IsFollowThroughSession = createDto.IsFollowThroughSession,
                    FollowThroughDate = createDto.FollowThroughDate,
                    IsReferral = createDto.IsReferral,
                    ReferralAgencyName = createDto.ReferralAgencyName,
                    // Counselor name (auto-populated from counselor record or from DTO)
                    CounselorName = !string.IsNullOrEmpty(createDto.CounselorName) ? createDto.CounselorName : counselor.Name,
                    CreatedAt = DateTime.UtcNow
                };

                // Parse time fields
                if (!string.IsNullOrEmpty(createDto.TimeStarted))
                {
                    if (TimeSpan.TryParse(createDto.TimeStarted, out var parsedTimeStarted))
                    {
                        note.TimeStarted = parsedTimeStarted;
                    }
                    else
                    {
                        return BadRequest("Invalid time started format. Use HH:mm or HH:mm:ss.");
                    }
                }

                if (!string.IsNullOrEmpty(createDto.TimeEnded))
                {
                    if (TimeSpan.TryParse(createDto.TimeEnded, out var parsedTimeEnded))
                    {
                        note.TimeEnded = parsedTimeEnded;
                    }
                    else
                    {
                        return BadRequest("Invalid time ended format. Use HH:mm or HH:mm:ss.");
                    }
                }

                _context.GuidanceNotes.Add(note);
                await _context.SaveChangesAsync();

                await _activityLogger.LogAsync("note", note.NoteId, "created", "counselor", counselorId, new
                {
                    studentId = note.StudentId
                });

                // Fetch the created note with related data
                var createdNoteFromDb = await _context.GuidanceNotes
                    .Include(n => n.Student)
                    .Include(n => n.Counselor)
                    .Where(n => n.NoteId == note.NoteId)
                    .FirstOrDefaultAsync();

                var createdNote = new GuidanceNoteDto
                {
                    NoteId = createdNoteFromDb.NoteId,
                    StudentId = createdNoteFromDb.StudentId,
                    CounselorId = createdNoteFromDb.CounselorId,
                    InterviewDate = createdNoteFromDb.InterviewDate,
                    TimeStarted = createdNoteFromDb.TimeStarted?.ToString(@"hh\:mm"),
                    TimeEnded = createdNoteFromDb.TimeEnded?.ToString(@"hh\:mm"),
                    SchoolYear = createdNoteFromDb.SchoolYear,
                    TertiarySemester = createdNoteFromDb.TertiarySemester,
                    SeniorHighQuarter = createdNoteFromDb.SeniorHighQuarter,
                    GradeYearLevelSection = createdNoteFromDb.GradeYearLevelSection,
                    Program = createdNoteFromDb.Program,
                    // Nature of Counseling
                    IsAcademic = createdNoteFromDb.IsAcademic,
                    IsBehavioral = createdNoteFromDb.IsBehavioral,
                    IsPersonal = createdNoteFromDb.IsPersonal,
                    IsSocial = createdNoteFromDb.IsSocial,
                    IsCareer = createdNoteFromDb.IsCareer,
                    // Counseling Situation
                    IsIndividual = createdNoteFromDb.IsIndividual,
                    IsGroup = createdNoteFromDb.IsGroup,
                    IsClass = createdNoteFromDb.IsClass,
                    IsCounselorInitiated = createdNoteFromDb.IsCounselorInitiated,
                    IsWalkIn = createdNoteFromDb.IsWalkIn,
                    IsFollowUp = createdNoteFromDb.IsFollowUp,
                    ReferredBy = createdNoteFromDb.ReferredBy,
                    // Notes sections
                    PresentingProblem = createdNoteFromDb.PresentingProblem,
                    Assessment = createdNoteFromDb.Assessment,
                    Interventions = createdNoteFromDb.Interventions,
                    PlanOfAction = createdNoteFromDb.PlanOfAction,
                    // Recommendations
                    IsFollowThroughSession = createdNoteFromDb.IsFollowThroughSession,
                    FollowThroughDate = createdNoteFromDb.FollowThroughDate,
                    IsReferral = createdNoteFromDb.IsReferral,
                    ReferralAgencyName = createdNoteFromDb.ReferralAgencyName,
                    // Counselor name
                    CounselorName = createdNoteFromDb.CounselorName,
                    CreatedAt = ConvertToManilaTime(createdNoteFromDb.CreatedAt),
                    Student = createdNoteFromDb.Student != null ? new StudentDto
                    {
                        StudentId = createdNoteFromDb.Student.StudentId,
                        FullName = createdNoteFromDb.Student.FullName,
                        StudentNumber = createdNoteFromDb.Student.StudentNumber
                    } : null,
                    Counselor = createdNoteFromDb.Counselor != null ? new CounselorDto
                    {
                        CounselorId = createdNoteFromDb.Counselor.CounselorId,
                        Name = createdNoteFromDb.Counselor.Name,
                        Email = createdNoteFromDb.Counselor.Email
                    } : null
                };

                return CreatedAtAction(nameof(GetGuidanceNote),
                    new { id = note.NoteId }, createdNote);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error creating guidance note", error = ex.Message });
            }
        }

        // PUT: api/guidance-notes/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateGuidanceNote(int id, UpdateGuidanceNoteDto updateDto)
        {
            try
            {
                // Get counselor ID from JWT token
                var counselorIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(counselorIdClaim) || !int.TryParse(counselorIdClaim, out int counselorId))
                {
                    return Unauthorized(new { message = "Invalid counselor authentication" });
                }

                var note = await _context.GuidanceNotes.FindAsync(id);
                if (note == null)
                {
                    return NotFound(new { message = "Guidance note not found" });
                }

                // Verify the note belongs to the authenticated counselor
                if (note.CounselorId != counselorId)
                {
                    return Forbid("You can only update your own notes");
                }

                // Verify student exists if studentId is being changed
                if (note.StudentId != updateDto.StudentId)
                {
                    var studentExists = await _context.Students.AnyAsync(s => s.StudentId == updateDto.StudentId);
                    if (!studentExists)
                    {
                        return BadRequest(new { message = "Student not found" });
                    }
                }

                // Update note properties
                note.StudentId = updateDto.StudentId;
                note.InterviewDate = updateDto.InterviewDate;
                note.SchoolYear = updateDto.SchoolYear;
                note.TertiarySemester = updateDto.TertiarySemester;
                note.SeniorHighQuarter = updateDto.SeniorHighQuarter;
                note.GradeYearLevelSection = updateDto.GradeYearLevelSection;
                note.Program = updateDto.Program;
                // Nature of Counseling
                note.IsAcademic = updateDto.IsAcademic;
                note.IsBehavioral = updateDto.IsBehavioral;
                note.IsPersonal = updateDto.IsPersonal;
                note.IsSocial = updateDto.IsSocial;
                note.IsCareer = updateDto.IsCareer;
                // Counseling Situation
                note.IsIndividual = updateDto.IsIndividual;
                note.IsGroup = updateDto.IsGroup;
                note.IsClass = updateDto.IsClass;
                note.IsCounselorInitiated = updateDto.IsCounselorInitiated;
                note.IsWalkIn = updateDto.IsWalkIn;
                note.IsFollowUp = updateDto.IsFollowUp;
                note.ReferredBy = updateDto.ReferredBy;
                // Notes sections
                note.PresentingProblem = updateDto.PresentingProblem;
                note.Assessment = updateDto.Assessment;
                note.Interventions = updateDto.Interventions;
                note.PlanOfAction = updateDto.PlanOfAction;
                // Recommendations
                note.IsFollowThroughSession = updateDto.IsFollowThroughSession;
                note.FollowThroughDate = updateDto.FollowThroughDate;
                note.IsReferral = updateDto.IsReferral;
                note.ReferralAgencyName = updateDto.ReferralAgencyName;
                // Update counselor name if provided
                if (!string.IsNullOrEmpty(updateDto.CounselorName))
                {
                    note.CounselorName = updateDto.CounselorName;
                }
                note.UpdatedAt = DateTime.UtcNow;

                // Parse time fields
                if (!string.IsNullOrEmpty(updateDto.TimeStarted))
                {
                    if (TimeSpan.TryParse(updateDto.TimeStarted, out var parsedTimeStarted))
                    {
                        note.TimeStarted = parsedTimeStarted;
                    }
                    else
                    {
                        return BadRequest("Invalid time started format. Use HH:mm or HH:mm:ss.");
                    }
                }
                else
                {
                    note.TimeStarted = null;
                }

                if (!string.IsNullOrEmpty(updateDto.TimeEnded))
                {
                    if (TimeSpan.TryParse(updateDto.TimeEnded, out var parsedTimeEnded))
                    {
                        note.TimeEnded = parsedTimeEnded;
                    }
                    else
                    {
                        return BadRequest("Invalid time ended format. Use HH:mm or HH:mm:ss.");
                    }
                }
                else
                {
                    note.TimeEnded = null;
                }

                await _context.SaveChangesAsync();

                await _activityLogger.LogAsync("note", note.NoteId, "updated", "counselor", counselorId, null);

                return Ok(new
                {
                    message = "Guidance note updated successfully.",
                    updatedAt = ConvertToManilaTime(note.UpdatedAt.Value),
                    noteId = note.NoteId
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error updating guidance note", error = ex.Message });
            }
        }

        // DELETE: api/guidance-notes/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteGuidanceNote(int id)
        {
            try
            {
                // Get counselor ID from JWT token
                var counselorIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(counselorIdClaim) || !int.TryParse(counselorIdClaim, out int counselorId))
                {
                    return Unauthorized(new { message = "Invalid counselor authentication" });
                }

                var note = await _context.GuidanceNotes.FindAsync(id);
                if (note == null)
                {
                    return NotFound(new { message = "Guidance note not found" });
                }

                // Verify the note belongs to the authenticated counselor
                if (note.CounselorId != counselorId)
                {
                    return Forbid("You can only delete your own notes");
                }

                _context.GuidanceNotes.Remove(note);
                await _context.SaveChangesAsync();

                await _activityLogger.LogAsync("note", note.NoteId, "deleted", "counselor", counselorId, null);

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error deleting guidance note", error = ex.Message });
            }
        }

        // GET: api/guidance-notes/student/{studentId}
        [HttpGet("student/{studentId}")]
        public async Task<ActionResult<IEnumerable<GuidanceNoteDto>>> GetGuidanceNotesByStudent(int studentId)
        {
            try
            {
                var notesFromDb = await _context.GuidanceNotes
                    .Include(n => n.Student)
                    .Include(n => n.Counselor)
                    .Where(n => n.StudentId == studentId)
                    .OrderByDescending(n => n.InterviewDate)
                    .ToListAsync();

                var notes = notesFromDb.Select(n => new GuidanceNoteDto
                {
                    NoteId = n.NoteId,
                    StudentId = n.StudentId,
                    CounselorId = n.CounselorId,
                    InterviewDate = n.InterviewDate,
                    TimeStarted = n.TimeStarted?.ToString(@"hh\:mm"),
                    TimeEnded = n.TimeEnded?.ToString(@"hh\:mm"),
                    SchoolYear = n.SchoolYear,
                    TertiarySemester = n.TertiarySemester,
                    SeniorHighQuarter = n.SeniorHighQuarter,
                    GradeYearLevelSection = n.GradeYearLevelSection,
                    Program = n.Program,
                    // Nature of Counseling
                    IsAcademic = n.IsAcademic,
                    IsBehavioral = n.IsBehavioral,
                    IsPersonal = n.IsPersonal,
                    IsSocial = n.IsSocial,
                    IsCareer = n.IsCareer,
                    // Counseling Situation
                    IsIndividual = n.IsIndividual,
                    IsGroup = n.IsGroup,
                    IsClass = n.IsClass,
                    IsCounselorInitiated = n.IsCounselorInitiated,
                    IsWalkIn = n.IsWalkIn,
                    IsFollowUp = n.IsFollowUp,
                    ReferredBy = n.ReferredBy,
                    // Notes sections
                    PresentingProblem = n.PresentingProblem,
                    Assessment = n.Assessment,
                    Interventions = n.Interventions,
                    PlanOfAction = n.PlanOfAction,
                    // Recommendations
                    IsFollowThroughSession = n.IsFollowThroughSession,
                    FollowThroughDate = n.FollowThroughDate,
                    IsReferral = n.IsReferral,
                    ReferralAgencyName = n.ReferralAgencyName,
                    CreatedAt = ConvertToManilaTime(n.CreatedAt),
                    UpdatedAt = n.UpdatedAt.HasValue ? ConvertToManilaTime(n.UpdatedAt.Value) : null,
                    Student = n.Student != null ? new StudentDto
                    {
                        StudentId = n.Student.StudentId,
                        FullName = n.Student.FullName,
                        StudentNumber = n.Student.StudentNumber
                    } : null,
                    Counselor = n.Counselor != null ? new CounselorDto
                    {
                        CounselorId = n.Counselor.CounselorId,
                        Name = n.Counselor.Name,
                        Email = n.Counselor.Email
                    } : null
                }).ToList();

                return Ok(notes);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error fetching student's guidance notes", error = ex.Message });
            }
        }

        // GET: api/guidance-notes/student-details/{studentId}
        [HttpGet("student-details/{studentId}")]
        public async Task<ActionResult<object>> GetStudentDetailsForGuidanceNote(int studentId)
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
                        gradeYearLevelSection = $"{careerPlanDetails.GradeYear}{(string.IsNullOrEmpty(careerPlanDetails.Section) ? "" : $"-{careerPlanDetails.Section}")}",
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
                        gradeYearLevelSection = studentDetails.GradeYear,
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

        // GET: api/guidance-notes/current-counselor
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
