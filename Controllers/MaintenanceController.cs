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

        // Dictionaries aggregate
        [HttpGet("dictionaries")]
        public async Task<IActionResult> GetDictionaries()
        {
            var all = await _ctx.DictionaryItems.Where(d => d.IsActive).ToListAsync();
            object group(string g) => all.Where(x => x.Group == g).Select(x => x.Value).OrderBy(x => x).ToList();

            var reasons = await _ctx.AppointmentReasons.Where(r => r.IsActive).OrderBy(r => r.Code).Select(r => r.Name).ToListAsync();
            var programs = await _ctx.Programs.Where(p => p.IsActive).OrderBy(p => p.Code).Select(p => new { p.Code, p.Name }).ToListAsync();
            var sections = await _ctx.Sections.Where(s => s.IsActive).OrderBy(s => s.ProgramCode).ThenBy(s => s.Name).ToListAsync();

            // Build sectionsByProgram: { "BSIT": ["4A","4B"], ... }
            var sectionsByProgram = sections
                .GroupBy(s => s.ProgramCode)
                .ToDictionary(g => g.Key, g => g.Select(x => x.Name).OrderBy(x => x).ToList());

            return Ok(new
            {
                gradeYears = group("gradeYears"),
                genders = group("genders"),
                academicLevels = group("academicLevels"),
                referredBy = group("referredBy"),
                areasOfConcern = group("areasOfConcern"),
                actionRequested = group("actionRequested"),
                referralPriorities = group("referralPriorities"),
                appointmentReasons = reasons,
                moodLevels = group("moodLevels"),
                programs = programs,               // [{code,name}]
                sectionsByProgram = sectionsByProgram
            });
        }

        // Mobile config single row
        [HttpGet("mobile-config")]
        public async Task<IActionResult> GetMobileConfig()
            => Ok(await _ctx.MobileConfigs.FirstOrDefaultAsync() ?? new MobileConfig { Id = 1 });

        [HttpPost("mobile-config")]
        public async Task<IActionResult> UpsertMobileConfig([FromBody] MobileConfig cfg)
        {
            cfg.Id = 1;
            if (!await _ctx.MobileConfigs.AnyAsync()) _ctx.MobileConfigs.Add(cfg);
            else _ctx.MobileConfigs.Update(cfg);
            await _ctx.SaveChangesAsync();
            return Ok(cfg);
        }

        // Quotes
        [HttpGet("quotes")]
        public async Task<IActionResult> GetQuotes() => Ok(await _ctx.Quotes.OrderByDescending(q => q.Id).ToListAsync());

        [HttpPost("quotes")]
        public async Task<IActionResult> CreateQuote([FromBody] Quote q) { _ctx.Quotes.Add(q); await _ctx.SaveChangesAsync(); return Ok(q); }

        [HttpPut("quotes/{id:int}")]
        public async Task<IActionResult> UpdateQuote(int id, [FromBody] Quote q) { q.Id = id; _ctx.Entry(q).State = EntityState.Modified; await _ctx.SaveChangesAsync(); return Ok(q); }

        [HttpDelete("quotes/{id:int}")]
        public async Task<IActionResult> DeleteQuote(int id) { var q = await _ctx.Quotes.FindAsync(id); if (q == null) return NotFound(); _ctx.Quotes.Remove(q); await _ctx.SaveChangesAsync(); return Ok(); }

        // Year Levels
        [HttpGet("year-levels")]
        public async Task<IActionResult> GetYearLevels() => Ok(await _ctx.YearLevels.OrderBy(x => x.Value).ToListAsync());

        [HttpPost("year-levels")]
        public async Task<IActionResult> CreateYearLevel([FromBody] YearLevel e) { _ctx.YearLevels.Add(e); await _ctx.SaveChangesAsync(); return Ok(e); }

        [HttpPut("year-levels/{id:int}")]
        public async Task<IActionResult> UpdateYearLevel(int id, [FromBody] YearLevel e) { e.Id = id; _ctx.Entry(e).State = EntityState.Modified; await _ctx.SaveChangesAsync(); return Ok(e); }

        [HttpDelete("year-levels/{id:int}")]
        public async Task<IActionResult> DeleteYearLevel(int id) { var e = await _ctx.YearLevels.FindAsync(id); if (e == null) return NotFound(); _ctx.YearLevels.Remove(e); await _ctx.SaveChangesAsync(); return Ok(); }

        // Dictionary items CRUD (optional UI)
        [HttpGet("dictionary-items")]
        public async Task<IActionResult> GetDictionaryItems() => Ok(await _ctx.DictionaryItems.OrderBy(d => d.Group).ThenBy(d => d.Value).ToListAsync());

        [HttpPost("dictionary-items")]
        public async Task<IActionResult> CreateDictionaryItem([FromBody] DictionaryItem d) { _ctx.DictionaryItems.Add(d); await _ctx.SaveChangesAsync(); return Ok(d); }

        [HttpPut("dictionary-items/{id:int}")]
        public async Task<IActionResult> UpdateDictionaryItem(int id, [FromBody] DictionaryItem d) { d.Id = id; _ctx.Entry(d).State = EntityState.Modified; await _ctx.SaveChangesAsync(); return Ok(d); }

        [HttpDelete("dictionary-items/{id:int}")]
        public async Task<IActionResult> DeleteDictionaryItem(int id) { var d = await _ctx.DictionaryItems.FindAsync(id); if (d == null) return NotFound(); _ctx.DictionaryItems.Remove(d); await _ctx.SaveChangesAsync(); return Ok(); }
    }
}
