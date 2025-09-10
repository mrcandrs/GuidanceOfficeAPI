using GuidanceOfficeAPI.Data;
using GuidanceOfficeAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
        // Update the PostAppointment method to include slot validation
        [HttpPost]
        public async Task<IActionResult> PostAppointment(GuidanceAppointment appointment)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Check if time slot is available
            if (!await IsTimeSlotAvailable(appointment.Date, appointment.Time))
            {
                return BadRequest(new { message = "Selected time slot is not available or fully booked" });
            }

            // Set CreatedAt to Philippines time
            appointment.CreatedAt = GetPhilippinesTime();
            appointment.Status = "pending";

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
            try
            {
                var appointment = await _context.GuidanceAppointments.FindAsync(id);

                if (appointment == null)
                    return NotFound(new { message = "Appointment not found" });

                if (appointment.Status.ToLower() != "pending")
                    return BadRequest(new { message = "Only pending appointments can be approved" });

                // Debug logging
                Console.WriteLine($"Approving appointment: Date={appointment.Date}, Time={appointment.Time}");

                // Check if the slot exists and is active
                var targetDate = DateTime.Parse(appointment.Date);
                var slot = await _context.AvailableTimeSlots
                    .FirstOrDefaultAsync(s => s.Date.Date == targetDate.Date && s.Time == appointment.Time && s.IsActive);

                if (slot == null)
                {
                    return BadRequest(new
                    {
                        message = $"No active time slot found for {appointment.Date} at {appointment.Time}",
                        appointmentDate = appointment.Date,
                        appointmentTime = appointment.Time
                    });
                }

                // Check current appointment count for this slot
                var currentCount = await _context.GuidanceAppointments
                    .CountAsync(a => a.Date == appointment.Date && a.Time == appointment.Time &&
                                    (a.Status.ToLower() == "pending" || a.Status.ToLower() == "approved"));

                Console.WriteLine($"Current count: {currentCount}, Max appointments: {slot.MaxAppointments}");

                if (currentCount >= slot.MaxAppointments)
                {
                    return BadRequest(new
                    {
                        message = $"Time slot is fully booked. Current: {currentCount}, Max: {slot.MaxAppointments}",
                        currentCount = currentCount,
                        maxAppointments = slot.MaxAppointments
                    });
                }

                appointment.Status = "approved";
                appointment.UpdatedAt = GetPhilippinesTime();

                await _context.SaveChangesAsync();

                // Update the appointment count for this time slot
                await UpdateAppointmentCount(appointment.Date, appointment.Time);

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
            catch (Exception ex)
            {
                Console.WriteLine($"Error approving appointment: {ex.Message}");
                return BadRequest(new { message = $"Error approving appointment: {ex.Message}" });
            }
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

            // Update the appointment count for this time slot (since we're rejecting, count decreases)
            await UpdateAppointmentCount(appointment.Date, appointment.Time);

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

        // Update the IsTimeSlotAvailable method to be more accurate
        private async Task<bool> IsTimeSlotAvailable(string date, string time)
        {
            var targetDate = DateTime.Parse(date);
            var slot = await _context.AvailableTimeSlots
                .FirstOrDefaultAsync(s => s.Date.Date == targetDate.Date && s.Time == time && s.IsActive);

            if (slot == null)
                return false;

            // Count current appointments for this slot (pending + approved)
            var currentCount = await _context.GuidanceAppointments
                .CountAsync(a => a.Date == date && a.Time == time &&
                                (a.Status.ToLower() == "pending" || a.Status.ToLower() == "approved"));

            return currentCount < slot.MaxAppointments;
        }

        // Add this method to update appointment counts when approving/rejecting
        private async Task UpdateAppointmentCount(string date, string time)
        {
            var targetDate = DateTime.Parse(date);
            var slot = await _context.AvailableTimeSlots
                .FirstOrDefaultAsync(s => s.Date.Date == targetDate.Date && s.Time == time);

            if (slot != null)
            {
                // Count current approved and pending appointments for this slot
                var currentCount = await _context.GuidanceAppointments
                    .CountAsync(a => a.Date == date && a.Time == time &&
                                    (a.Status.ToLower() == "pending" || a.Status.ToLower() == "approved"));

                slot.CurrentAppointmentCount = currentCount;
                slot.UpdatedAt = GetPhilippinesTime();

                await _context.SaveChangesAsync();
            }
        }

        // Add this method to sync appointment counts (useful for maintenance)
        [HttpPost("sync-appointment-counts")]
        public async Task<IActionResult> SyncAppointmentCounts()
        {
            var slots = await _context.AvailableTimeSlots.ToListAsync();
            var updatedCount = 0;

            foreach (var slot in slots)
            {
                var currentCount = await _context.GuidanceAppointments
                    .CountAsync(a => a.Date == slot.Date.ToString("yyyy-MM-dd") && a.Time == slot.Time &&
                                    (a.Status.ToLower() == "pending" || a.Status.ToLower() == "approved"));

                if (slot.CurrentAppointmentCount != currentCount)
                {
                    slot.CurrentAppointmentCount = currentCount;
                    slot.UpdatedAt = GetPhilippinesTime();
                    updatedCount++;
                }
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = $"Synced {updatedCount} time slot appointment counts",
                updatedSlots = updatedCount
            });
        }
    }

    // Helper class for status updates
    public class StatusUpdateRequest
    {
        public string Status { get; set; }
    }
}