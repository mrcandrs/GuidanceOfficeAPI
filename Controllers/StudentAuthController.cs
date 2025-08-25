using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;
using GuidanceOfficeAPI.Data;
using GuidanceOfficeAPI.Dtos;
using GuidanceOfficeAPI.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace GuidanceOfficeAPI.Controllers
{
    [ApiController]
    [Route("api/student")]
    public class StudentAuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<StudentAuthController> _logger;

        public StudentAuthController(AppDbContext context, ILogger<StudentAuthController> logger)
        {
            _context = context;
            _logger = logger;
        }


        private static DateTime ConvertToManilaTime(DateTime utcDateTime)
        {
            var manilaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Manila");
            return TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, manilaTimeZone);
        }

        [HttpPost("submit-full-registration")]
        public async Task<IActionResult> SubmitFullRegistration([FromBody] FullRegistrationDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                                              .Select(e => e.ErrorMessage)
                                              .ToList();
                return BadRequest(new { message = "Validation failed", errors });
            }

            try
            {
                // Check for duplicates
                bool emailExists = _context.Students.Any(s => s.Email == dto.Student.Email);
                bool studentNumberExists = _context.Students.Any(s => s.StudentNumber == dto.Student.StudentNumber);
                bool userNameExists = _context.Students.Any(s => s.Username == dto.Student.Username);

                if (emailExists || studentNumberExists || userNameExists)
                {
                    return BadRequest(new
                    {
                        message = "Duplicate found.",
                        emailExists,
                        studentNumberExists,
                        userNameExists
                    });
                }

                // Start transaction for data integrity
                using var transaction = await _context.Database.BeginTransactionAsync();

                try
                {
                    // 1. Save Student first
                    _context.Students.Add(dto.Student);
                    await _context.SaveChangesAsync();
                    int studentId = dto.Student.StudentId;

                    // 2. Save ConsentForm
                    dto.ConsentForm.StudentId = studentId;
                    dto.ConsentForm.SignedDate = DateTime.UtcNow;
                    _context.ConsentForms.Add(dto.ConsentForm);
                    await _context.SaveChangesAsync();

                    // 3. Save InventoryForm
                    dto.InventoryForm.StudentId = studentId;
                    dto.InventoryForm.SubmissionDate = DateTime.UtcNow;
                    _context.InventoryForms.Add(dto.InventoryForm);
                    await _context.SaveChangesAsync();

                    // Get the generated InventoryForm ID
                    int inventoryFormId = dto.InventoryForm.InventoryId;

                    // 4. Save Siblings if they exist
                    /*if (dto.InventoryForm.Siblings != null && dto.InventoryForm.Siblings.Any())
                    {
                        foreach (var sibling in dto.InventoryForm.Siblings)
                        {
                            sibling.SiblingId = 0; // 👈 Force EF to treat it as a new record
                            sibling.InventoryFormId = inventoryFormId;
                            _context.Siblings.Add(sibling);
                        }
                        await _context.SaveChangesAsync();
                    }*/

                    // 5. Save Work Experience if they exist
                    /*if (dto.InventoryForm.WorkExperience != null && dto.InventoryForm.WorkExperience.Any())
                    {
                        foreach (var work in dto.InventoryForm.WorkExperience)
                        {
                            work.WorkId = 0;
                            work.InventoryFormId = inventoryFormId;
                            _context.WorkExperiences.Add(work);
                        }
                        await _context.SaveChangesAsync();
                    }*/

                    // 6. Save CareerPlanningForm
                    dto.CareerPlanningForm.StudentId = studentId;
                    dto.CareerPlanningForm.SubmittedAt = DateTime.UtcNow;
                    _context.CareerPlanningForms.Add(dto.CareerPlanningForm);
                    await _context.SaveChangesAsync();

                    // Commit transaction
                    await transaction.CommitAsync();

                    return Ok(new { message = "Registration successful." });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error during transaction");
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Registration error: {Message}", ex.Message);
                _logger.LogError(ex, "Stack trace: {StackTrace}", ex.StackTrace);

                return StatusCode(500, new
                {
                    message = "Server error occurred.",
                    error = ex.Message,
                    innerError = ex.InnerException?.Message
                });
            }
        }


        // POST: api/student/register
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] Student student)
        {
            if (string.IsNullOrWhiteSpace(student.StudentNumber) || string.IsNullOrWhiteSpace(student.Email) || string.IsNullOrWhiteSpace(student.Username) || string.IsNullOrWhiteSpace(student.Password))
            {
                return BadRequest(new { message = "Student Number, Email, Username, and Password are required." });
            }

            if (_context.Students.Any(s => s.Email == student.Email))
                return BadRequest(new { message = "Email already registered." });

            student.DateRegistered = DateTime.UtcNow;
            _context.Students.Add(student);
            await _context.SaveChangesAsync();

            var dto = new RegisterStudentDto
            {
                StudentId = student.StudentId,
                StudentNumber = student.StudentNumber,
                Username = student.Username,
                Email = student.Email,
                DateRegistered = ConvertToManilaTime(student.DateRegistered)
            };

            return Ok(dto);
        }

        // POST: api/student/login
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Login) || string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new { message = "Student Number and Password are required." });
            }

            var student = _context.Students.FirstOrDefault(s =>
                (s.StudentNumber == request.Login || s.Username == request.Login) &&
                s.Password == request.Password);

            if (student == null)
                return Unauthorized(new { message = "Invalid credentials." });

            //✅ Set LastLogin to Asia/Manila time
            var manilaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Manila");
            student.LastLogin = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, manilaTimeZone);

            _context.SaveChanges(); //✅ Save to DB

            var dto = new LoginDto
            {
                StudentId = student.StudentId,
                StudentNumber = student.StudentNumber,
                FullName = student.FullName,
                Username = student.Username,
                Email = student.Email,
                Program = student.Program,
                YearLevel = student.GradeYear,
                DateRegistered = ConvertToManilaTime(student.DateRegistered),
                LastLogin = student.LastLogin
            };

            return Ok(dto);
        }

        // GET: api/student/check-duplicate
        [HttpGet("check-duplicate")]
        public IActionResult CheckDuplicate(string studentNumber, string email)
        {
            bool studentNumberExists = _context.Students.Any(s => s.StudentNumber == studentNumber);
            bool emailExists = _context.Students.Any(s => s.Email == email);

            return Ok(new
            {
                studentNumberExists,
                emailExists
            });
        }

        // GET: api/student/check-duplicate-email
        [HttpGet("check-duplicate-email")]
        public IActionResult CheckDuplicateEmail(string email1, string email2, int studentId)
        {
            var existingEmails = _context.Students
                .Where(s => s.StudentId != studentId && (s.Email == email1 || s.Email == email2))
                .Select(s => s.Email)
                .ToList();

            return Ok(new
            {
                email1Exists = existingEmails.Contains(email1),
                email2Exists = existingEmails.Contains(email2)
            });
        }




        // GET: api/student/check-email-username
        [HttpGet("check-email-username")]
        public IActionResult CheckEmailOrUsername([FromQuery] string email, [FromQuery] string username, [FromQuery] int studentId)
        {
            bool emailExists = _context.Students.Any(s => s.Email == email && s.StudentId != studentId);
            bool usernameExists = _context.Students.Any(s => s.Username == username && s.StudentId != studentId);

            return Ok(new
            {
                emailExists,
                userNameExists = usernameExists
            });
        }


        [HttpPost("update-profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] StudentUpdateDto dto)
        {
            var student = await _context.Students.FindAsync(dto.StudentId);
            if (student == null) return NotFound();

            student.Email = dto.Email;
            student.Username = dto.Username;
            student.Password = dto.Password;

            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpPost("update-profile-with-image")]
        public async Task<IActionResult> UpdateProfileWithImage([FromForm] StudentUpdateWithImageDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState); // Helps debug what caused 400
            }

            var student = await _context.Students.FindAsync(dto.StudentId);
            if (student == null) return NotFound();

            student.Email = dto.Email;
            student.Username = dto.Username;

            // Update password only if not empty
            if (!string.IsNullOrWhiteSpace(dto.Password))
            {
                student.Password = dto.Password;
            }

            if (dto.ProfileImage != null && dto.ProfileImage.Length > 0)
            {
                using var ms = new MemoryStream();
                await dto.ProfileImage.CopyToAsync(ms);
                student.ProfileImage = ms.ToArray();
            }

            await _context.SaveChangesAsync();
            return Ok();
        }

        // GET: api/student/students-with-mood
        [HttpGet("students-with-mood")]
        public async Task<IActionResult> GetStudentsWithLastMood()
        {
            try
            {
                var studentsWithMood = await _context.Students
                    .Select(s => new
                    {
                        id = s.StudentId,
                        name = s.FullName,
                        studentno = s.StudentNumber,
                        program = s.Program,
                        section = s.GradeYear,
                        dateregistered = s.DateRegistered,
                        lastlogin = s.LastLogin,
                        status = "Active", // For now, static value
                        lastMood = _context.MoodTrackers
                            .Where(m => m.StudentId == s.StudentId)
                            .OrderByDescending(m => m.EntryDate)
                            .Select(m => m.MoodLevel)
                            .FirstOrDefault()
                    })
                    .ToListAsync();

                return Ok(studentsWithMood);
            }
            catch (Exception ex)
            {
                // Log the error
                Console.WriteLine($"Error fetching students: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        // DELETE: api/student/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteStudent(int id)
        {
            try
            {
                // Get counselor ID from JWT token
                var counselorIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(counselorIdClaim) || !int.TryParse(counselorIdClaim, out int counselorId))
                {
                    return Unauthorized(new { message = "Invalid counselor authentication" });
                }

                var form = await _context.Students.FindAsync(id);
                if (form == null)
                {
                    return NotFound(new { message = "Student not found" });
                }

                _context.Students.Remove(form);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error deleting student", error = ex.Message });
            }
        }

    }

    public class LoginRequest
    {
        public string Login { get; set; }
        public string Password { get; set; }
    }
}
