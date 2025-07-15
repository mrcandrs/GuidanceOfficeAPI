using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;
using GuidanceOfficeAPI.Data;
using GuidanceOfficeAPI.Dtos;
using GuidanceOfficeAPI.Models;
using Microsoft.EntityFrameworkCore;

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
                // 🔒 Check for duplicate email or student number
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

                // Save student
                _context.Students.Add(dto.Student);
                await _context.SaveChangesAsync();

                int studentId = dto.Student.StudentId;

                // Set foreign keys
                dto.ConsentForm.StudentId = studentId;
                dto.InventoryForm.StudentId = studentId;
                dto.CareerPlanningForm.StudentId = studentId;

                dto.ConsentForm.SignedDate = DateTime.UtcNow;
                dto.InventoryForm.SubmissionDate = DateTime.UtcNow;
                dto.CareerPlanningForm.SubmittedAt = DateTime.UtcNow;

                _context.ConsentForms.Add(dto.ConsentForm);
                _context.InventoryForms.Add(dto.InventoryForm);
                await _context.SaveChangesAsync();

                _context.CareerPlanningForms.Add(dto.CareerPlanningForm);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Registration successful." });
            }
            catch (Exception ex)
            {
                Console.WriteLine("🔴 Registration error: " + ex.Message);
                Console.WriteLine("🔴 Stack trace: " + ex.StackTrace);
                return StatusCode(500, new { message = "Server error occurred.", error = ex.Message });
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

            var dto = new LoginDto
            {
                StudentId = student.StudentId,
                StudentNumber = student.StudentNumber,
                FullName = student.FullName,
                Username = student.Username,
                Email = student.Email,
                Program = student.Program,
                YearLevel = student.GradeYear,
                DateRegistered = ConvertToManilaTime(student.DateRegistered)
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
            var student = await _context.Students.FindAsync(dto.StudentId);
            if (student == null) return NotFound();

            student.Email = dto.Email;
            student.Username = dto.Username;
            student.Password = dto.Password;

            if (dto.ProfileImage != null)
            {
                using var ms = new MemoryStream();
                await dto.ProfileImage.CopyToAsync(ms);
                student.ProfileImage = ms.ToArray();
            }

            await _context.SaveChangesAsync();
            return Ok();
        }



    }

    public class LoginRequest
    {
        public string Login { get; set; }
        public string Password { get; set; }
    }
}
