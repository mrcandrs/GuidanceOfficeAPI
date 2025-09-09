using GuidanceOfficeAPI.Data;
using GuidanceOfficeAPI.Models;
using Microsoft.AspNetCore.Mvc;

namespace GuidanceOfficeAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GuidanceAppointmentController : ControllerBase
    {
        private readonly AppDbContext _context;

        public GuidanceAppointmentController(AppDbContext context)
        {
            _context = context;
        }

        // Helper method to get Philippines time
        private DateTime GetPhilippinesTime()
        {
            var philippinesTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Manila");
            return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, philippinesTimeZone);
        }

        // GET: api/guidanceappointment/all-appointments
        [HttpGet("all-appointments")]
        public IActionResult GetAllAppointments()
        {
            var appointments = _context.GuidanceAppointments
                .OrderByDescending(a => a.CreatedAt) // Order by submission date
                .ToList();
            return Ok(appointments);
        }

        // GET: api/guidanceappointment/pending-appointments
        [HttpGet("pending-appointments")]
        public IActionResult GetPendingAppointments()
        {
            var appointments = _context.GuidanceAppointments
                .Where(a => a.Status.ToLower() == "pending")
                .OrderByDescending(a => a.CreatedAt) // Order by submission date
                .ToList();
            return Ok(appointments);
        }

        // GET: api/guidanceappointment/{id}
        [HttpGet("{id}")]
        public IActionResult GetAppointmentById(int id)
        {
            var appointment = _context.GuidanceAppointments
                .FirstOrDefault(a => a.AppointmentId == id);

            if (appointment == null)
                return NotFound(new { message = "Appointment not found" });

            return Ok(appointment);
        }

        // POST: api/guidanceappointment
        [HttpPost]
        public async Task<IActionResult> PostAppointment(GuidanceAppointment appointment)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Set CreatedAt to Philippines time
            appointment.CreatedAt = GetPhilippinesTime();
            appointment.Status = "pending"; // Ensure status is pending

            _context.GuidanceAppointments.Add(appointment);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Appointment submitted successfully." });
        }

        // PUT: api/guidanceappointment/{id}/status
        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateAppointmentStatus(int id, [FromBody] StatusUpdateRequest request)
        {
            var appointment = await _context.GuidanceAppointments.FindAsync(id);

            if (appointment == null)
                return NotFound(new { message = "Appointment not found" });

            appointment.Status = request.Status;
            appointment.UpdatedAt = GetPhilippinesTime(); // Track when status was updated in Philippines time

            await _context.SaveChangesAsync();

            return Ok(new { message = $"Appointment status updated to {request.Status}" });
        }

        // PUT: api/guidanceappointment/{id}/approve
        [HttpPut("{id}/approve")]
        public async Task<IActionResult> ApproveAppointment(int id)
        {
            var appointment = await _context.GuidanceAppointments.FindAsync(id);

            if (appointment == null)
                return NotFound(new { message = "Appointment not found" });

            if (appointment.Status.ToLower() != "pending")
                return BadRequest(new { message = "Only pending appointments can be approved" });

            appointment.Status = "approved";
            appointment.UpdatedAt = GetPhilippinesTime();

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Appointment approved successfully",
                appointment = new
                {
                    appointmentId = appointment.AppointmentId,
                    studentName = appointment.StudentName,
                    status = appointment.Status,
                    updatedAt = appointment.UpdatedAt
                }
            });
        }

        // PUT: api/guidanceappointment/{id}/reject
        [HttpPut("{id}/reject")]
        public async Task<IActionResult> RejectAppointment(int id)
        {
            var appointment = await _context.GuidanceAppointments.FindAsync(id);

            if (appointment == null)
                return NotFound(new { message = "Appointment not found" });

            if (appointment.Status.ToLower() != "pending")
                return BadRequest(new { message = "Only pending appointments can be rejected" });

            appointment.Status = "rejected";
            appointment.UpdatedAt = GetPhilippinesTime();

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Appointment rejected successfully",
                appointment = new
                {
                    appointmentId = appointment.AppointmentId,
                    studentName = appointment.StudentName,
                    status = appointment.Status,
                    updatedAt = appointment.UpdatedAt
                }
            });
        }

        // GET: api/guidanceappointment/student/{studentId}
        [HttpGet("student/{studentId}")]
        public IActionResult GetStudentAppointments(int studentId)
        {
            var appointments = _context.GuidanceAppointments
                .Where(a => a.StudentId == studentId)
                .OrderByDescending(a => a.CreatedAt)
                .ToList();

            return Ok(appointments);
        }
    }

    // Helper class for status updates
    public class StatusUpdateRequest
    {
        public string Status { get; set; }
    }
}