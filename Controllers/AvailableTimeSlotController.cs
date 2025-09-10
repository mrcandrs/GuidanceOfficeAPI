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

            return Ok(new { message = $"Time slot {(slot.IsActive ? "activated" : "deactivated")} successfully", slot });
        }

        // POST: api/availabletimeslot/bulk
        [HttpPost("bulk")]
        public async Task<IActionResult> CreateBulkTimeSlots([FromBody] BulkTimeSlotRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var slots = new List<AvailableTimeSlot>();
            var createdSlots = new List<AvailableTimeSlot>();

            foreach (var time in request.Times)
            {
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
                skipped = request.Times.Count - createdSlots.Count
            });
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
