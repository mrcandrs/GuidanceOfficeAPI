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
