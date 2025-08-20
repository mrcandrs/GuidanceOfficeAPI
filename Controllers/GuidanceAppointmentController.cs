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

            // Ensure CreatedAt is set to current time
            appointment.CreatedAt = DateTime.Now;
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
            appointment.UpdatedAt = DateTime.Now; // Track when status was updated

            await _context.SaveChangesAsync();

            return Ok(new { message = $"Appointment status updated to {request.Status}" });
        }
    }

    // Helper class for status updates
    public class StatusUpdateRequest
    {
        public string Status { get; set; }
    }
}