using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Globalization;
using GuidanceOfficeAPI.Data;
using GuidanceOfficeAPI.Models;
using GuidanceOfficeAPI.Services;

namespace GuidanceOfficeAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GuidancePassController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IActivityLogger _activityLogger;

        public GuidancePassController(AppDbContext context, IActivityLogger activityLogger)
        {
            _context = context;
            _activityLogger = activityLogger;
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
                .Where(gp => gp.Appointment.StudentId == studentId
                             && gp.Appointment.Status.ToLower() == "approved") // added
                .OrderByDescending(gp => gp.IssuedDate)
                .FirstOrDefaultAsync();

            if (guidancePass == null)
                return NotFound();

            return guidancePass;
        }

        // GET: api/guidancepass
        [HttpGet]
        public async Task<ActionResult<IEnumerable<GuidancePass>>> GetGuidancePasses([FromQuery] int? studentId = null)
        {
            var query = _context.GuidancePasses
                .Include(gp => gp.Counselor)
                .Include(gp => gp.Appointment)
                .Where(gp => gp.Appointment.Status.ToLower() == "approved"); // added

            if (studentId.HasValue)
                query = query.Where(gp => gp.Appointment.StudentId == studentId.Value);

            var passes = await query
                .OrderByDescending(gp => gp.IssuedDate)
                .ToListAsync();

            return Ok(passes);
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
            await _activityLogger.LogAsync("guidancepass", guidancePass.PassId, "created", "counselor", guidancePass.CounselorId, new
            {
                appointmentId = guidancePass.AppointmentId
            });

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
            }

            // Important: mark the appointment as completed/closed so the student is unblocked
            appointment.Status = "completed"; // or "closed", but be consistent with the rest of your app
            appointment.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Time slot deactivated and appointment marked completed." });
        }

    }

    public class CreateGuidancePassRequest
    {
        public int AppointmentId { get; set; }
        public string Notes { get; set; }
        public int CounselorId { get; set; }
    }
}