using System.Numerics;
using GuidanceOfficeAPI.Data;
using GuidanceOfficeAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GuidanceOfficeAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AvailableTimeSlotController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AvailableTimeSlotController(AppDbContext context)
        {
            _context = context;
        }

        // Helper method to get Philippines time
        private DateTime GetPhilippinesTime()
        {
            var philippinesTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Manila");
            return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, philippinesTimeZone);
        }

        // GET: api/availabletimeslot
        [HttpGet]
        public async Task<IActionResult> GetAvailableTimeSlots()
        {
            await ExpirePastTodaySlots();
            var today = GetPhilippinesTime().Date;
            var slots = await _context.AvailableTimeSlots
                .Where(s => s.Date >= today && s.IsActive)
                .OrderBy(s => s.Date)
                .ThenBy(s => s.Time)
                .ToListAsync();

            return Ok(slots);
        }

        // GET: api/availabletimeslot/date/{date}
        [HttpGet("date/{date}")]
        public async Task<IActionResult> GetAvailableTimeSlotsByDate(string date)
        {
            await ExpirePastTodaySlots();
            if (!DateTime.TryParse(date, out DateTime targetDate))
            {
                return BadRequest(new { message = "Invalid date format" });
            }

            var slots = await _context.AvailableTimeSlots
                .Where(s => s.Date.Date == targetDate.Date && s.IsActive)
                .OrderBy(s => s.Time)
                .ToListAsync();

            return Ok(slots);
        }

        // POST: api/availabletimeslot
        [HttpPost]
        public async Task<IActionResult> CreateAvailableTimeSlot([FromBody] AvailableTimeSlotRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Validate that the date is not in the past
            var today = GetPhilippinesTime().Date;
            if (request.Date.Date < today)
            {
                return BadRequest(new { message = "Cannot create time slots for past dates" });
            }

            // If it's today, validate that the time hasn't passed
            if (request.Date.Date == today)
            {
                var currentTime = GetPhilippinesTime();
                var currentTimeString = currentTime.ToString("h:mm tt");

                // Parse the requested time to compare
                if (DateTime.TryParseExact(request.Time, "h:mm tt", null, System.Globalization.DateTimeStyles.None, out DateTime requestedTime))
                {
                    var requestedDateTime = today.Add(requestedTime.TimeOfDay);

                    if (requestedDateTime <= currentTime)
                    {
                        return BadRequest(new { message = $"Cannot create time slots for past times. Current time is {currentTimeString}" });
                    }
                }
            }

            // Check if slot already exists
            var existingSlot = await _context.AvailableTimeSlots
                .FirstOrDefaultAsync(s => s.Date.Date == request.Date.Date && s.Time == request.Time);

            if (existingSlot != null)
            {
                return BadRequest(new { message = "Time slot already exists for this date and time" });
            }

            var slot = new AvailableTimeSlot
            {
                Date = request.Date.Date,
                Time = request.Time,
                MaxAppointments = request.MaxAppointments,
                CreatedAt = GetPhilippinesTime()
            };

            _context.AvailableTimeSlots.Add(slot);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Time slot created successfully", slot });
        }

        // PUT: api/availabletimeslot/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAvailableTimeSlot(int id, [FromBody] AvailableTimeSlotRequest request)
        {
            var slot = await _context.AvailableTimeSlots.FindAsync(id);
            if (slot == null)
                return NotFound(new { message = "Time slot not found" });

            slot.Date = request.Date.Date;
            slot.Time = request.Time;
            slot.MaxAppointments = request.MaxAppointments;
            slot.UpdatedAt = GetPhilippinesTime();

            await _context.SaveChangesAsync();

            return Ok(new { message = "Time slot updated successfully", slot });
        }

        // DELETE: api/availabletimeslot/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAvailableTimeSlot(int id)
        {
            var slot = await _context.AvailableTimeSlots.FindAsync(id);
            if (slot == null)
                return NotFound(new { message = "Time slot not found" });

            _context.AvailableTimeSlots.Remove(slot);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Time slot deleted successfully" });
        }

        // PUT: api/availabletimeslot/{id}/toggle
        [HttpPut("{id}/toggle")]
        public async Task<IActionResult> ToggleTimeSlot(int id)
        {
            var slot = await _context.AvailableTimeSlots.FindAsync(id);
            if (slot == null)
                return NotFound(new { message = "Time slot not found" });

            slot.IsActive = !slot.IsActive;
            slot.UpdatedAt = GetPhilippinesTime();

            await _context.SaveChangesAsync();

            if (!slot.IsActive)
            {
                await CompleteApprovedAppointmentsForSlot(slot);
            }

            return Ok(new { message = $"Time slot {(slot.IsActive ? "activated" : "deactivated")} successfully", slot });
        }

        //Make auto-expiry also complete appointments:
        private async Task<int> ExpirePastTodaySlots()
        {
            var now = GetPhilippinesTime();
            var today = now.Date;
            var changed = 0;

            var todaysActive = await _context.AvailableTimeSlots
                .Where(s => s.IsActive && s.Date == today)
                .ToListAsync();

            foreach (var slot in todaysActive)
            {
                if (TryParseSlotTime(slot.Time, out var t))
                {
                    var slotDateTime = today.Add(t.TimeOfDay);
                    if (slotDateTime <= now)
                    {
                        slot.IsActive = false;
                        slot.UpdatedAt = now;
                        changed++;

                        // complete any remaining approved appointments for this slot
                        await CompleteApprovedAppointmentsForSlot(slot);
                    }
                }
            }

            if (changed > 0) await _context.SaveChangesAsync();
            return changed;
        }

        // POST: api/availabletimeslot/bulk
        [HttpPost("bulk")]
        public async Task<IActionResult> CreateBulkTimeSlots([FromBody] BulkTimeSlotRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var today = GetPhilippinesTime().Date;
            var currentTime = GetPhilippinesTime();

            // Validate that the date is not in the past
            if (request.Date.Date < today)
            {
                return BadRequest(new { message = "Cannot create time slots for past dates" });
            }

            var slots = new List<AvailableTimeSlot>();
            var createdSlots = new List<AvailableTimeSlot>();
            var skippedSlots = new List<string>();

            foreach (var time in request.Times)
            {
                // If it's today, check if the time has passed
                if (request.Date.Date == today)
                {
                    if (DateTime.TryParseExact(time, "h:mm tt", null, System.Globalization.DateTimeStyles.None, out DateTime requestedTime))
                    {
                        var requestedDateTime = today.Add(requestedTime.TimeOfDay);

                        if (requestedDateTime <= currentTime)
                        {
                            skippedSlots.Add($"{time} (time has passed)");
                            continue;
                        }
                    }
                }

                // Check if slot already exists
                var existingSlot = await _context.AvailableTimeSlots
                    .FirstOrDefaultAsync(s => s.Date.Date == request.Date.Date && s.Time == time);

                if (existingSlot == null)
                {
                    var slot = new AvailableTimeSlot
                    {
                        Date = request.Date.Date,
                        Time = time,
                        MaxAppointments = request.MaxAppointments,
                        CreatedAt = GetPhilippinesTime()
                    };
                    slots.Add(slot);
                }
                else
                {
                    skippedSlots.Add($"{time} (already exists)");
                }
            }

            if (slots.Any())
            {
                _context.AvailableTimeSlots.AddRange(slots);
                await _context.SaveChangesAsync();
                createdSlots = slots;
            }

            return Ok(new
            {
                message = $"{createdSlots.Count} time slots created successfully",
                createdSlots,
                skipped = skippedSlots,
                skippedCount = skippedSlots.Count
            });
        }

        // Add this method to AvailableTimeSlotController
        [HttpGet("with-counts")]
        public async Task<IActionResult> GetAvailableTimeSlotsWithCounts()
        {
            var today = GetPhilippinesTime().Date;
            var slots = await _context.AvailableTimeSlots
                .Where(s => s.Date >= today && s.IsActive)
                .OrderBy(s => s.Date)
                .ThenBy(s => s.Time)
                .ToListAsync();

            // Update counts for each slot
            foreach (var slot in slots)
            {
                var currentCount = await _context.GuidanceAppointments
                    .CountAsync(a => a.Date == slot.Date.ToString("yyyy-MM-dd") && a.Time == slot.Time &&
                                    (a.Status.ToLower() == "pending" || a.Status.ToLower() == "approved"));

                slot.CurrentAppointmentCount = currentCount;
            }

            await _context.SaveChangesAsync();

            return Ok(slots);
        }

        // Add this method to AvailableTimeSlotController
        [HttpGet("available-for-students")]
        public async Task<IActionResult> GetAvailableSlotsForStudents()
        {
            await ExpirePastTodaySlots();
            var today = GetPhilippinesTime().Date;
            var availableSlots = new List<object>();

            // Get all active time slots
            var slots = await _context.AvailableTimeSlots
                .Where(s => s.Date >= today && s.IsActive)
                .OrderBy(s => s.Date)
                .ThenBy(s => s.Time)
                .ToListAsync();

            foreach (var slot in slots)
            {
                // Count only approved appointments for this slot
                var approvedCount = await _context.GuidanceAppointments
                    .CountAsync(a => a.Date == slot.Date.ToString("yyyy-MM-dd") &&
                                    a.Time == slot.Time &&
                                    a.Status.ToLower() == "approved");

                // Only include slots that have available capacity
                if (approvedCount < slot.MaxAppointments)
                {
                    availableSlots.Add(new
                    {
                        date = slot.Date.ToString("yyyy-MM-dd"),
                        time = slot.Time,
                        availableSpots = slot.MaxAppointments - approvedCount,
                        maxAppointments = slot.MaxAppointments
                    });
                }
            }

            return Ok(availableSlots);
        }

        // Also add this method to get available slots for a specific date
        [HttpGet("available-for-students/date/{date}")]
        public async Task<IActionResult> GetAvailableSlotsForStudentsByDate(string date)
        {
            await ExpirePastTodaySlots();
            if (!DateTime.TryParse(date, out DateTime targetDate))
            {
                return BadRequest(new { message = "Invalid date format" });
            }

            var availableSlots = new List<object>();

            // Get active time slots for the specific date
            var slots = await _context.AvailableTimeSlots
                .Where(s => s.Date.Date == targetDate.Date && s.IsActive)
                .OrderBy(s => s.Time)
                .ToListAsync();

            foreach (var slot in slots)
            {
                // Count ONLY approved appointments for this slot
                var approvedCount = await _context.GuidanceAppointments
                    .CountAsync(a => a.Date == date &&
                                    a.Time == slot.Time &&
                                    a.Status.ToLower() == "approved");

                // Only include slots that have available capacity
                if (approvedCount < slot.MaxAppointments)
                {
                    availableSlots.Add(new
                    {
                        date = slot.Date.ToString("yyyy-MM-dd"),
                        time = slot.Time,
                        availableSpots = slot.MaxAppointments - approvedCount,
                        maxAppointments = slot.MaxAppointments
                    });
                }
            }

            return Ok(availableSlots);
        }

        // Add this method to AvailableTimeSlotController
        [HttpDelete("{id}/safe-delete")]
        public async Task<IActionResult> SafeDeleteTimeSlot(int id)
        {
            var slot = await _context.AvailableTimeSlots.FindAsync(id);
            if (slot == null)
                return NotFound(new { message = "Time slot not found" });

            // Check if there are any appointments for this slot
            var appointmentCount = await _context.GuidanceAppointments
                .CountAsync(a => a.Date == slot.Date.ToString("yyyy-MM-dd") &&
                                a.Time == slot.Time);

            if (appointmentCount > 0)
            {
                return BadRequest(new
                {
                    message = $"Cannot delete time slot. There are {appointmentCount} existing appointments for this slot.",
                    appointmentCount = appointmentCount,
                    hasAppointments = true
                });
            }

            _context.AvailableTimeSlots.Remove(slot);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Time slot deleted successfully" });
        }

        // Add this method to get slot details with appointment info
        [HttpGet("{id}/details")]
        public async Task<IActionResult> GetTimeSlotDetails(int id)
        {
            var slot = await _context.AvailableTimeSlots.FindAsync(id);
            if (slot == null)
                return NotFound(new { message = "Time slot not found" });

            var appointments = await _context.GuidanceAppointments
                .Where(a => a.Date == slot.Date.ToString("yyyy-MM-dd") && a.Time == slot.Time)
                .Select(a => new
                {
                    appointmentId = a.AppointmentId,
                    studentName = a.StudentName,
                    status = a.Status,
                    createdAt = a.CreatedAt
                })
                .ToListAsync();

            return Ok(new
            {
                slot = slot,
                appointments = appointments,
                appointmentCount = appointments.Count,
                canDelete = appointments.Count == 0
            });
        }

        // In AvailableTimeSlotController.cs - GetAllTimeSlotsForAdmin method
        [HttpGet("admin/all-slots")]
        public async Task<IActionResult> GetAllTimeSlotsForAdmin()
        {
            await ExpirePastTodaySlots();
            var today = GetPhilippinesTime().Date;
            var slots = await _context.AvailableTimeSlots
                .Where(s => s.Date >= today) // Only show future slots
                .OrderBy(s => s.Date)
                .ThenBy(s => s.Time)
                .ToListAsync();

            // Update counts for each slot - ONLY count approved appointments
            foreach (var slot in slots)
            {
                var approvedCount = await _context.GuidanceAppointments
                    .CountAsync(a => a.Date == slot.Date.ToString("yyyy-MM-dd") && a.Time == slot.Time &&
                                    a.Status.ToLower() == "approved");

                var pendingCount = await _context.GuidanceAppointments
                    .CountAsync(a => a.Date == slot.Date.ToString("yyyy-MM-dd") && a.Time == slot.Time &&
                                    a.Status.ToLower() == "pending");
                    
                // Set the current count to approved only for display
                slot.CurrentAppointmentCount = approvedCount;

                // You can add a separate property for pending if needed
                // slot.PendingAppointmentCount = pendingCount;
            }

            await _context.SaveChangesAsync();

            return Ok(slots);
        }

        private async Task<int> CompleteApprovedAppointmentsForSlot(AvailableTimeSlot slot)
        {
            var dateKey = slot.Date.ToString("yyyy-MM-dd");
            var now = GetPhilippinesTime();

            var approved = await _context.GuidanceAppointments
                .Where(a => a.Date == dateKey && a.Time == slot.Time && a.Status.ToLower() == "approved")
                .ToListAsync();

            foreach (var appt in approved)
            {
                appt.Status = "completed";
                appt.UpdatedAt = now;
            }

            if (approved.Count > 0)
            {
                await _context.SaveChangesAsync();
            }

            return approved.Count;
        }

        // Add helper to parse "h:mm tt"
        private bool TryParseSlotTime(string time, out DateTime parsed)
        {
            return DateTime.TryParseExact(time, "h:mm tt", null, System.Globalization.DateTimeStyles.None, out parsed);
        }

        // Deactivate today's past slots
        private async Task<int> ExpirePastTodaySlots()
        {
            var now = GetPhilippinesTime();
            var today = now.Date;
            var changed = 0;

            var todaysActive = await _context.AvailableTimeSlots
                .Where(s => s.IsActive && s.Date == today)
                .ToListAsync();

            foreach (var slot in todaysActive)
            {
                if (TryParseSlotTime(slot.Time, out var t))
                {
                    var slotDateTime = today.Add(t.TimeOfDay);
                    if (slotDateTime <= now)
                    {
                        slot.IsActive = false;
                        slot.UpdatedAt = now;
                        changed++;
                    }
                }
            }

            if (changed > 0) await _context.SaveChangesAsync();
            return changed;
        }
    }

    // Request models
    public class AvailableTimeSlotRequest
    {
        public DateTime Date { get; set; }
        public string Time { get; set; }
        public int MaxAppointments { get; set; } = 3;
    }

    public class BulkTimeSlotRequest
    {
        public DateTime Date { get; set; }
        public List<string> Times { get; set; }
        public int MaxAppointments { get; set; } = 3;
    }
}
