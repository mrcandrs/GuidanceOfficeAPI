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
        [HttpPost]
        public async Task<IActionResult> PostAppointment(GuidanceAppointment appointment)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                    return BadRequest(new { message = "Validation failed", errors = errors });
                }

                // Validate required fields
                if (string.IsNullOrWhiteSpace(appointment.StudentName))
                    return BadRequest(new { message = "Student name is required" });

                if (string.IsNullOrWhiteSpace(appointment.ProgramSection))
                    return BadRequest(new { message = "Program section is required" });

                if (string.IsNullOrWhiteSpace(appointment.Reason))
                    return BadRequest(new { message = "Reason is required" });

                if (string.IsNullOrWhiteSpace(appointment.Date))
                    return BadRequest(new { message = "Date is required" });

                if (string.IsNullOrWhiteSpace(appointment.Time))
                    return BadRequest(new { message = "Time is required" });

                // Validate date format
                if (!DateTime.TryParse(appointment.Date, out DateTime parsedDate))
                {
                    return BadRequest(new { message = "Invalid date format. Use YYYY-MM-DD" });
                }

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
            catch (Exception ex)
            {
                // Log the exception for debugging
                Console.WriteLine($"Error creating appointment: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");

                return StatusCode(500, new
                {
                    message = "An error occurred while creating the appointment",
                    error = ex.Message
                });
            }
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

                // Check if the slot exists and is active
                var targetDate = DateTime.Parse(appointment.Date);
                var slot = await _context.AvailableTimeSlots
                    .FirstOrDefaultAsync(s => s.Date.Date == targetDate.Date && s.Time == appointment.Time && s.IsActive);

                if (slot == null)
                {
                    return BadRequest(new
                    {
                        message = $"No active time slot found for {appointment.Date} at {appointment.Time}"
                    });
                }

                // Count ONLY approved appointments (excluding the current pending one we're about to approve)
                var approvedCount = await _context.GuidanceAppointments
                    .CountAsync(a => a.Date == appointment.Date && a.Time == appointment.Time &&
                                    a.Status.ToLower() == "approved");

                if (approvedCount >= slot.MaxAppointments)
                {
                    return BadRequest(new
                    {
                        message = $"Time slot is fully booked. Approved: {approvedCount}, Max: {slot.MaxAppointments}",
                        approvedCount = approvedCount,
                        maxAppointments = slot.MaxAppointments
                    });
                }

                // Approve the current appointment
                appointment.Status = "approved";
                appointment.UpdatedAt = GetPhilippinesTime();

                // Auto-reject remaining pending appointments if slot becomes full
                var newApprovedCount = approvedCount + 1;
                if (newApprovedCount >= slot.MaxAppointments)
                {
                    // Get all remaining pending appointments for this slot
                    var remainingPendingAppointments = await _context.GuidanceAppointments
                        .Where(a => a.Date == appointment.Date &&
                                   a.Time == appointment.Time &&
                                   a.Status.ToLower() == "pending" &&
                                   a.AppointmentId != appointment.AppointmentId)
                        .ToListAsync();

                    // Auto-reject all remaining pending appointments with custom reason
                    foreach (var pendingAppointment in remainingPendingAppointments)
                    {
                        pendingAppointment.Status = "rejected";
                        pendingAppointment.RejectionReason = $"Slot became full after another appointment was approved for {appointment.Date} at {appointment.Time}";
                        pendingAppointment.UpdatedAt = GetPhilippinesTime();
                    }

                    Console.WriteLine($"Auto-rejected {remainingPendingAppointments.Count} pending appointments for slot {appointment.Date} {appointment.Time}");
                }

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
        public async Task<IActionResult> RejectAppointment(int id, [FromBody] RejectAppointmentRequest request)
        {
            var appointment = await _context.GuidanceAppointments.FindAsync(id);

            if (appointment == null)
                return NotFound(new { message = "Appointment not found" });

            if (appointment.Status.ToLower() != "pending")
                return BadRequest(new { message = "Only pending appointments can be rejected" });

            // Validate rejection reason
            if (string.IsNullOrWhiteSpace(request.RejectionReason))
            {
                return BadRequest(new { message = "Rejection reason is required" });
            }

            appointment.Status = "rejected";
            appointment.RejectionReason = request.RejectionReason.Trim();
            appointment.UpdatedAt = GetPhilippinesTime();

            await _context.SaveChangesAsync();

            // Update the appointment count for this time slot
            await UpdateAppointmentCount(appointment.Date, appointment.Time);

            return Ok(new
            {
                message = "Appointment rejected successfully",
                appointment = new
                {
                    appointmentId = appointment.AppointmentId,
                    studentName = appointment.StudentName,
                    status = appointment.Status,
                    rejectionReason = appointment.RejectionReason,
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
            try
            {
                if (string.IsNullOrWhiteSpace(date) || string.IsNullOrWhiteSpace(time))
                {
                    Console.WriteLine("Date or time is null or empty");
                    return false;
                }

                if (!DateTime.TryParse(date, out DateTime targetDate))
                {
                    Console.WriteLine($"Invalid date format: {date}");
                    return false;
                }

                var slot = await _context.AvailableTimeSlots
                    .FirstOrDefaultAsync(s => s.Date.Date == targetDate.Date && s.Time == time && s.IsActive);

                if (slot == null)
                {
                    Console.WriteLine($"No active slot found for {date} at {time}");
                    return false;
                }

                // Count ONLY approved appointments (not pending ones)
                var approvedCount = await _context.GuidanceAppointments
                    .CountAsync(a => a.Date == date && a.Time == time &&
                                    a.Status.ToLower() == "approved");

                Console.WriteLine($"Slot {date} {time}: Approved count = {approvedCount}, Max = {slot.MaxAppointments}");

                return approvedCount < slot.MaxAppointments;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error checking time slot availability: {ex.Message}");
                return false;
            }
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

        // GET: api/guidanceappointment/approved-by-slot/{slotId}
        [HttpGet("approved-by-slot/{slotId:int}")]
        public async Task<IActionResult> GetApprovedAppointmentsBySlot(int slotId)
        {
            var slot = await _context.AvailableTimeSlots.FirstOrDefaultAsync(s => s.SlotId == slotId);
            if (slot == null)
                return NotFound(new { message = "Time slot not found" });

            var dateKey = slot.Date.ToString("yyyy-MM-dd");

            var approved = await _context.GuidanceAppointments
                .Where(a => a.Date == dateKey && a.Time == slot.Time && a.Status.ToLower() == "approved")
                .OrderBy(a => a.CreatedAt)
                .Select(a => new
                {
                    appointmentId = a.AppointmentId,
                    studentId = a.StudentId,
                    studentName = a.StudentName,
                    programSection = a.ProgramSection,
                    reason = a.Reason,
                    date = a.Date,
                    time = a.Time,
                    status = a.Status,
                    createdAt = a.CreatedAt,
                    updatedAt = a.UpdatedAt
                })
                .ToListAsync();

            return Ok(approved);
        }
    }

    // Helper class for status updates
    public class StatusUpdateRequest
    {
        public string Status { get; set; }
    }

    // Add this class to your GuidanceAppointmentController file
    public class RejectAppointmentRequest
    {
        public string RejectionReason { get; set; }
    }
}