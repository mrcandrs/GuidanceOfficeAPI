using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Globalization;
using GuidanceOfficeAPI.Data;
using GuidanceOfficeAPI.Models;

namespace GuidanceOfficeAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GuidancePassController : ControllerBase
    {
        private readonly AppDbContext _context;

        public GuidancePassController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/guidancepass/{id}
        [HttpGet("{id:int}")]
        public async Task<ActionResult<GuidancePass>> GetGuidancePass(int id)
        {
            var guidancePass = await _context.GuidancePasses
                .Include(gp => gp.Counselor)
                .Include(gp => gp.Appointment)
                .FirstOrDefaultAsync(gp => gp.PassId == id);

            if (guidancePass == null)
                return NotFound();

            return guidancePass;
        }

        // GET: api/guidancepass/student/{studentId}
        [HttpGet("student/{studentId:int}")]
        public async Task<ActionResult<GuidancePass>> GetGuidancePassByStudent(int studentId)
        {
            var guidancePass = await _context.GuidancePasses
                .Include(gp => gp.Counselor)
                .Include(gp => gp.Appointment)
                .Where(gp => gp.Appointment.StudentId == studentId)
                .OrderByDescending(gp => gp.IssuedDate)
                .FirstOrDefaultAsync();

            if (guidancePass == null)
                return NotFound();

            return guidancePass;
        }

        // POST: api/guidancepass
        [HttpPost]
        public async Task<ActionResult<GuidancePass>> CreateGuidancePass(CreateGuidancePassRequest request)
        {
            var appointment = await _context.GuidanceAppointments
                .FirstOrDefaultAsync(a => a.AppointmentId == request.AppointmentId);

            if (appointment == null)
                return BadRequest("Appointment not found");

            if (!string.Equals(appointment.Status, "approved", StringComparison.OrdinalIgnoreCase))
                return BadRequest("Can only create guidance pass for approved appointments");

            var existingPass = await _context.GuidancePasses
                .FirstOrDefaultAsync(gp => gp.AppointmentId == request.AppointmentId);

            if (existingPass != null)
                return BadRequest("Guidance pass already exists for this appointment");

            var guidancePass = new GuidancePass
            {
                AppointmentId = request.AppointmentId,
                IssuedDate = DateTime.UtcNow,
                Notes = request.Notes,
                CounselorId = request.CounselorId
            };

            _context.GuidancePasses.Add(guidancePass);
            await _context.SaveChangesAsync();

            var createdPass = await _context.GuidancePasses
                .Include(gp => gp.Counselor)
                .Include(gp => gp.Appointment)
                .FirstOrDefaultAsync(gp => gp.PassId == guidancePass.PassId);

            return CreatedAtAction(nameof(GetGuidancePass), new { id = guidancePass.PassId }, createdPass);
        }

        // POST: api/guidancepass/deactivate-slot/{appointmentId}
        [HttpPost("deactivate-slot/{appointmentId:int}")]
        public async Task<IActionResult> DeactivateSlotForAppointment(int appointmentId)
        {
            var appointment = await _context.GuidanceAppointments
                .FirstOrDefaultAsync(a => a.AppointmentId == appointmentId);

            if (appointment == null)
                return NotFound("Appointment not found");

            if (!string.Equals(appointment.Status, "approved", StringComparison.OrdinalIgnoreCase))
                return BadRequest("Can only deactivate slots for approved appointments");

            // appointment.Date is a string like "yyyy-MM-dd"
            // Normalize it to DateTime (date-only) before comparing with ts.Date (DateTime)
            if (!DateTime.TryParseExact(
                    appointment.Date,
                    "yyyy-MM-dd",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                    out var apptDateUtc))
            {
                return BadRequest("Invalid appointment date format. Expected yyyy-MM-dd.");
            }

            var apptDateOnly = apptDateUtc.Date;

            var timeSlot = await _context.AvailableTimeSlots
                .FirstOrDefaultAsync(ts => ts.Date.Date == apptDateOnly && ts.Time == appointment.Time);

            if (timeSlot != null)
            {
                timeSlot.IsActive = false;
                await _context.SaveChangesAsync();
            }

            return Ok(new { message = "Time slot deactivated successfully" });
        }
    }

    public class CreateGuidancePassRequest
    {
        public int AppointmentId { get; set; }
        public string Notes { get; set; }
        public int CounselorId { get; set; }
    }
}