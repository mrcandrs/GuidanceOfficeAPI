using GuidanceOfficeAPI.Data;
using GuidanceOfficeAPI.Models;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using iText.Forms.Fields;
using iText.Forms;
using iText.IO.Font.Constants;
using iText.Kernel.Font;
using iText.Kernel.Pdf;

namespace GuidanceOfficeAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CareerPlanningController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CareerPlanningController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/careerplanning/5
        [HttpGet("{studentId}")]
        public async Task<ActionResult<CareerPlanningForm>> GetCareerPlanningForm(int studentId)
        {
            var form = await _context.CareerPlanningForms.FirstOrDefaultAsync(f => f.StudentId == studentId);

            if (form == null)
            {
                return NotFound();
            }

            return form;
        }

        // PUT: api/careerplanning/5
        [HttpPut("{studentId}")]
        public async Task<IActionResult> UpdateCareerPlanningForm(int studentId, CareerPlanningForm updatedForm)
        {
            if (studentId != updatedForm.StudentId)
            {
                return BadRequest("Student ID mismatch.");
            }

            var existingForm = await _context.CareerPlanningForms.FirstOrDefaultAsync(f => f.StudentId == studentId);
            if (existingForm == null)
            {
                return NotFound();
            }

            existingForm.Program = updatedForm.Program;
            existingForm.Section = updatedForm.Section;
            existingForm.ContactNumber = updatedForm.ContactNumber;
            existingForm.Birthday = updatedForm.Birthday;
            existingForm.Gender = updatedForm.Gender;
            existingForm.GradeYear = updatedForm.GradeYear;

            existingForm.TopValue1 = updatedForm.TopValue1;
            existingForm.TopValue2 = updatedForm.TopValue2;
            existingForm.TopValue3 = updatedForm.TopValue3;

            existingForm.TopStrength1 = updatedForm.TopStrength1;
            existingForm.TopStrength2 = updatedForm.TopStrength2;
            existingForm.TopStrength3 = updatedForm.TopStrength3;

            existingForm.TopSkill1 = updatedForm.TopSkill1;
            existingForm.TopSkill2 = updatedForm.TopSkill2;
            existingForm.TopSkill3 = updatedForm.TopSkill3;

            existingForm.TopInterest1 = updatedForm.TopInterest1;
            existingForm.TopInterest2 = updatedForm.TopInterest2;
            existingForm.TopInterest3 = updatedForm.TopInterest3;

            existingForm.ProgramChoice = updatedForm.ProgramChoice;
            existingForm.FirstChoice = updatedForm.FirstChoice;
            existingForm.OriginalChoice = updatedForm.OriginalChoice;
            existingForm.ProgramExpectation = updatedForm.ProgramExpectation;
            existingForm.EnrollmentReason = updatedForm.EnrollmentReason;
            existingForm.FutureVision = updatedForm.FutureVision;

            existingForm.MainPlan = updatedForm.MainPlan;
            existingForm.AnotherCourse = updatedForm.AnotherCourse;
            existingForm.MastersProgram = updatedForm.MastersProgram;
            existingForm.CourseField = updatedForm.CourseField;
            existingForm.LocalEmployment = updatedForm.LocalEmployment;
            existingForm.WorkAbroad = updatedForm.WorkAbroad;
            existingForm.NatureJob1 = updatedForm.NatureJob1;
            existingForm.CurrentWorkAbroad = updatedForm.CurrentWorkAbroad;
            existingForm.AimPromotion = updatedForm.AimPromotion;
            existingForm.NatureJob2 = updatedForm.NatureJob2;
            existingForm.BusinessNature = updatedForm.BusinessNature;

            existingForm.EmploymentNature = updatedForm.EmploymentNature;
            existingForm.CurrentWorkNature = updatedForm.CurrentWorkNature;
            existingForm.ProgramChoiceReason = updatedForm.ProgramChoiceReason;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("{studentId}/pdf")]
        public async Task<IActionResult> GetCareerPlanningPdf(int studentId)
        {
            var form = await _context.CareerPlanningForms
                .FirstOrDefaultAsync(f => f.StudentId == studentId);
            if (form == null) return NotFound(new { message = "Career Planning Form not found" });

            var templatePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "pdf-templates", "CareerPlanningFormTemplate.pdf");
            if (!System.IO.File.Exists(templatePath))
                return NotFound(new { message = "Template not found", path = templatePath });

            using var ms = new MemoryStream();
            using (var reader = new PdfReader(templatePath))
            using (var writer = new PdfWriter(ms))
            using (var pdf = new PdfDocument(reader, writer))
            {
                var acro = PdfAcroForm.GetAcroForm(pdf, true);
                var fields = acro.GetFormFields();
                acro.SetNeedAppearances(true);
                acro.SetGenerateAppearance(true);
                var font = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);

                // ---------- Text ----------
                TrySetText(fields, font, "StudentNo", form.StudentNo);
                TrySetText(fields, font, "Name", form.FullName);
                TrySetText(fields, font, "Program", form.Program);
                TrySetText(fields, font, "GradeYearSection", form.GradeYear);
                TrySetText(fields, font, "Section", form.Section);
                TrySetText(fields, font, "ContactNumber", form.ContactNumber);
                TrySetText(fields, font, "Birthday", form.Birthday);

                TrySetText(fields, font, "TopValue1", form.TopValue1);
                TrySetText(fields, font, "TopValue2", form.TopValue2);
                TrySetText(fields, font, "TopValue3", form.TopValue3);

                TrySetText(fields, font, "TopStrength1", form.TopStrength1);
                TrySetText(fields, font, "TopStrength2", form.TopStrength2);
                TrySetText(fields, font, "TopStrength3", form.TopStrength3);

                TrySetText(fields, font, "TopSkill1", form.TopSkill1);
                TrySetText(fields, font, "TopSkill2", form.TopSkill2);
                TrySetText(fields, font, "TopSkill3", form.TopSkill3);

                TrySetText(fields, font, "TopInterest1", form.TopInterest1);
                TrySetText(fields, font, "TopInterest2", form.TopInterest2);
                TrySetText(fields, font, "TopInterest3", form.TopInterest3);

                TrySetText(fields, font, "ProgramChoice", form.ProgramChoice);
                TrySetText(fields, font, "OriginalChoice", form.OriginalChoice);
                TrySetText(fields, font, "ProgramExpectation", form.ProgramExpectation);
                TrySetText(fields, font, "EnrollmentReason", form.EnrollmentReason);
                TrySetText(fields, font, "FutureVision", form.FutureVision);
                TrySetText(fields, font, "WhoseChoice", form.ProgramChoiceReason);
                // optional extra texts if your template has these fields
                TrySetText(fields, font, "EmploymentNature", form.EmploymentNature);
                TrySetText(fields, font, "CurrentWorkNature", form.CurrentWorkNature);

                // ---------- Radios (Option Buttons with Group name + Reference value) ----------
                SetRadio(fields, "FirstChoice", NormalizeYesNo(form.FirstChoice)); // FirstChoice stored as string

                // ---------- Main plan (radio group preferred; checkbox fallback) ----------
                var mainPlan = form.MainPlan; // ContinueSchooling|GetEmployed|ContinueCurrentWork|GoIntoBusiness
                if (!SetRadio(fields, "MainPlan", mainPlan))
                {
                    SetSingleChoiceCheckboxes(fields, "ContinueSchooling", "GetEmployed", "ContinueCurrentWork", "GoIntoBusiness", mainPlan);
                }

                // ---------- Sub-options (independent checkboxes + texts) ----------
                SetCheckbox(fields, "AnotherCourse", form.AnotherCourse);
                SetCheckbox(fields, "MastersProgram", form.MastersProgram);
                TrySetText(fields, font, "CourseField", form.CourseField);

                SetCheckbox(fields, "LocalEmployment", form.LocalEmployment);
                SetCheckbox(fields, "WorkAbroad", form.WorkAbroad);
                TrySetText(fields, font, "NatureJob1", form.NatureJob1);

                SetCheckbox(fields, "AimPromotion", form.AimPromotion);
                SetCheckbox(fields, "CurrentWorkAbroad", form.CurrentWorkAbroad);
                TrySetText(fields, font, "NatureJob2", form.NatureJob2);

                TrySetText(fields, font, "BusinessNature", form.BusinessNature);



                // Optional footer
                // AssessedBy = Counselor Name (ephemeral: from auth claims; no DB change)
                var counselorName =
                    User?.FindFirst("name")?.Value ??           // OIDC standard
                    User?.FindFirst("given_name")?.Value + " " + User?.FindFirst("family_name")?.Value ??
                    User?.FindFirst("preferred_username")?.Value ??
                    User?.Identity?.Name ??
                    "";

                TrySetText(fields, font, "AssessedBy", counselorName?.Trim());
                TrySetText(fields, font, "DateSigned", form.SubmittedAt.ToString("MMMM dd, yyyy"));

                acro.FlattenFields();
            }

            return File(ms.ToArray(), "application/pdf", $"CareerPlanningForm_{studentId}.pdf");
        }

        // ===== iText 7 helpers =====
        private static void TrySetText(IDictionary<string, PdfFormField> fields, PdfFont font, string name, string? value, float size = 11f)
        {
            if (value == null) value = "";
            if (fields.TryGetValue(name, out var f)) { try { f.SetValue(value, font, size); } catch { } }
        }

        private static void SetCheckbox(IDictionary<string, PdfFormField> fields, string name, bool isChecked)
        {
            if (!fields.TryGetValue(name, out var f)) return;
            if (f is PdfButtonFormField btn)
            {
                var states = btn.GetAppearanceStates(); // e.g. ["Off","Yes"] or ["Off","On"]
                var on = states?.FirstOrDefault(s => !string.Equals(s, "Off", StringComparison.OrdinalIgnoreCase)) ?? "Yes";
                try { btn.SetValue(isChecked ? on : "Off"); } catch { }
            }
        }

        private static bool SetRadio(IDictionary<string, PdfFormField> fields, string groupName, string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) value = "";
            if (!fields.TryGetValue(groupName, out var field)) return false;
            try { field.SetValue(value); return true; } catch { return false; }
        }

        private static void SetSingleChoiceCheckboxes(IDictionary<string, PdfFormField> fields, string a, string b, string c, string d, string? selected)
        {
            var choices = new[] { a, b, c, d };
            foreach (var name in choices)
            {
                SetCheckbox(fields, name, string.Equals(name, selected, StringComparison.OrdinalIgnoreCase));
            }
        }

        private static string NormalizeYesNo(string? v)
        {
            if (string.IsNullOrWhiteSpace(v)) return "";
            var s = v.Trim().ToLowerInvariant();
            if (s is "yes" or "y" or "true" or "1") return "Yes";
            if (s is "no" or "n" or "false" or "0") return "No";
            return v;
        }
    }

}
