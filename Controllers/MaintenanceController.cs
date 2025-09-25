// Controllers/MaintenanceController.cs
using GuidanceOfficeAPI.Data;
using GuidanceOfficeAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GuidanceOfficeAPI.Controllers
{
    [ApiController]
    [Route("api/maintenance")]
    public class MaintenanceController : ControllerBase
    {
        private readonly AppDbContext _ctx;
        public MaintenanceController(AppDbContext ctx) { _ctx = ctx; }

        // Programs
        [HttpGet("programs")]
        public async Task<IActionResult> GetPrograms() => Ok(await _ctx.Programs.OrderBy(x => x.Code).ToListAsync());

        [HttpPost("programs")]
        public async Task<IActionResult> CreateProgram([FromBody] ProgramEntity e) { _ctx.Programs.Add(e); await _ctx.SaveChangesAsync(); return Ok(e); }

        [HttpPut("programs/{id:int}")]
        public async Task<IActionResult> UpdateProgram(int id, [FromBody] ProgramEntity e) { e.Id = id; _ctx.Entry(e).State = EntityState.Modified; await _ctx.SaveChangesAsync(); return Ok(e); }

        [HttpDelete("programs/{id:int}")]
        public async Task<IActionResult> DeleteProgram(int id) { var e = await _ctx.Programs.FindAsync(id); if (e == null) return NotFound(); _ctx.Programs.Remove(e); await _ctx.SaveChangesAsync(); return Ok(); }

        // Sections
        [HttpGet("sections")]
        public async Task<IActionResult> GetSections() => Ok(await _ctx.Sections.OrderBy(x => x.ProgramCode).ThenBy(x => x.Name).ToListAsync());

        [HttpPost("sections")]
        public async Task<IActionResult> CreateSection([FromBody] SectionEntity e) { _ctx.Sections.Add(e); await _ctx.SaveChangesAsync(); return Ok(e); }

        [HttpPut("sections/{id:int}")]
        public async Task<IActionResult> UpdateSection(int id, [FromBody] SectionEntity e) { e.Id = id; _ctx.Entry(e).State = EntityState.Modified; await _ctx.SaveChangesAsync(); return Ok(e); }

        [HttpDelete("sections/{id:int}")]
        public async Task<IActionResult> DeleteSection(int id) { var e = await _ctx.Sections.FindAsync(id); if (e == null) return NotFound(); _ctx.Sections.Remove(e); await _ctx.SaveChangesAsync(); return Ok(); }

        // Appointment Reasons
        [HttpGet("appointment-reasons")]
        public async Task<IActionResult> GetReasons() => Ok(await _ctx.AppointmentReasons.OrderBy(x => x.Code).ToListAsync());

        [HttpPost("appointment-reasons")]
        public async Task<IActionResult> CreateReason([FromBody] AppointmentReason e) { _ctx.AppointmentReasons.Add(e); await _ctx.SaveChangesAsync(); return Ok(e); }

        [HttpPut("appointment-reasons/{id:int}")]
        public async Task<IActionResult> UpdateReason(int id, [FromBody] AppointmentReason e) { e.Id = id; _ctx.Entry(e).State = EntityState.Modified; await _ctx.SaveChangesAsync(); return Ok(e); }

        [HttpDelete("appointment-reasons/{id:int}")]
        public async Task<IActionResult> DeleteReason(int id) { var e = await _ctx.AppointmentReasons.FindAsync(id); if (e == null) return NotFound(); _ctx.AppointmentReasons.Remove(e); await _ctx.SaveChangesAsync(); return Ok(); }

        // Referral Categories
        [HttpGet("referral-categories")]
        public async Task<IActionResult> GetRefCats() => Ok(await _ctx.ReferralCategories.OrderBy(x => x.Code).ToListAsync());

        [HttpPost("referral-categories")]
        public async Task<IActionResult> CreateRefCat([FromBody] ReferralCategory e) { _ctx.ReferralCategories.Add(e); await _ctx.SaveChangesAsync(); return Ok(e); }

        [HttpPut("referral-categories/{id:int}")]
        public async Task<IActionResult> UpdateRefCat(int id, [FromBody] ReferralCategory e) { e.Id = id; _ctx.Entry(e).State = EntityState.Modified; await _ctx.SaveChangesAsync(); return Ok(e); }

        [HttpDelete("referral-categories/{id:int}")]
        public async Task<IActionResult> DeleteRefCat(int id) { var e = await _ctx.ReferralCategories.FindAsync(id); if (e == null) return NotFound(); _ctx.ReferralCategories.Remove(e); await _ctx.SaveChangesAsync(); return Ok(); }

        // Time Slot Defaults (singleton)
        [HttpGet("timeslot-defaults")]
        public async Task<IActionResult> GetTsDefaults() => Ok(await _ctx.TimeSlotDefaults.FirstOrDefaultAsync() ?? new TimeSlotDefaults { Id = 1 });

        [HttpPost("timeslot-defaults")]
        public async Task<IActionResult> CreateTsDefaults([FromBody] TimeSlotDefaults e) { e.Id = 1; if (!await _ctx.TimeSlotDefaults.AnyAsync()) _ctx.TimeSlotDefaults.Add(e); else _ctx.TimeSlotDefaults.Update(e); await _ctx.SaveChangesAsync(); return Ok(e); }

        [HttpPut("timeslot-defaults/{id:int}")]
        public async Task<IActionResult> UpdateTsDefaults(int id, [FromBody] TimeSlotDefaults e) { e.Id = 1; _ctx.TimeSlotDefaults.Update(e); await _ctx.SaveChangesAsync(); return Ok(e); }

        // Mood Thresholds (singleton)
        [HttpGet("mood-thresholds")]
        public async Task<IActionResult> GetThresholds() => Ok(await _ctx.MoodThresholds.FirstOrDefaultAsync() ?? new MoodThresholds { Id = 1 });

        [HttpPost("mood-thresholds")]
        public async Task<IActionResult> CreateThresholds([FromBody] MoodThresholds e) { e.Id = 1; if (!await _ctx.MoodThresholds.AnyAsync()) _ctx.MoodThresholds.Add(e); else _ctx.MoodThresholds.Update(e); await _ctx.SaveChangesAsync(); return Ok(e); }

        [HttpPut("mood-thresholds/{id:int}")]
        public async Task<IActionResult> UpdateThresholds(int id, [FromBody] MoodThresholds e) { e.Id = 1; _ctx.MoodThresholds.Update(e); await _ctx.SaveChangesAsync(); return Ok(e); }
    }
}
