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
            var appointments = _context.GuidanceAppointments.ToList();
            return Ok(appointments);
        }

        // GET: api/guidanceappointment/pending-appointments
        [HttpGet("pending-appointments")]
        public IActionResult GetPendingAppointments()
        {
            var appointments = _context.GuidanceAppointments
                .Where(a => a.Status.ToLower() == "pending")
                .ToList();
            return Ok(appointments);
        }

        // GET: api/guidanceappointment/{id} (getting appointment by ID)
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

            _context.GuidanceAppointments.Add(appointment);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Appointment submitted successfully." });
        }

    }

}
