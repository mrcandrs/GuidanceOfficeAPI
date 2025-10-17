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
using System;
using System.Collections.Generic;
using System.Reflection;

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

        // GET: api/careerplanning/debug/field-names
        [HttpGet("debug/field-names")]
        public IActionResult GetDebugFieldNames()
        {
            // 1) Model properties via reflection
            var modelProps = typeof(CareerPlanningForm)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Select(p => new { p.Name, Type = p.PropertyType.Name })
                .OrderBy(p => p.Name)
                .ToArray();

            // 2) Field names the controller tries to populate (hard-coded list)
            var controllerFieldNames = new[]
            {
                // text fields
                "StudentNo","FullName","Program","GradeYear","Section","ContactNumber","Birthday",
                "TopValue1","TopValue2","TopValue3",
                "TopStrength1","TopStrength2","TopStrength3",
                "TopSkill1","TopSkill2","TopSkill3",
                "TopInterest1","TopInterest2","TopInterest3",
                "ProgramChoice","OriginalChoice","ProgramExpectation","EnrollmentReason","FutureVision",
                "WhoseChoice","EmploymentNature","CurrentWorkNature",
                // radios / groups
                "FirstChoice","MainPlan",
                // checkboxes + extra texts
                "AnotherCourse","MastersProgram","CourseField",
                "LocalEmployment","WorkAbroad","NatureJob1",
                "AimPromotion","CurrentWorkAbroad","NatureJob2",
                "BusinessNature",
                // footer (optional)
                "AssessedBy","DateSigned"
            };

            // 3) Actual PDF field names discovered from the template (if available)
            var templatePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "pdf-templates", "CareerPlanningFormTemplate.pdf");
            string[] pdfFieldNames;
            if (System.IO.File.Exists(templatePath))
            {
                using var reader = new PdfReader(templatePath);
                using var pdfDoc = new PdfDocument(reader);
                var acro = PdfAcroForm.GetAcroForm(pdfDoc, false);
                var fields = acro?.GetFormFields();
                pdfFieldNames = fields?.Keys?.OrderBy(k => k).ToArray() ?? Array.Empty<string>();
            }
            else
            {
                pdfFieldNames = Array.Empty<string>();
            }

            return Ok(new
            {
                modelProperties = modelProps,
                controllerFields = controllerFieldNames,
                pdfFields = pdfFieldNames,
                templateFound = System.IO.File.Exists(templatePath),
                templatePath
            });
        }

        // GET: api/careerplanning/debug/radio-groups/{studentId}
        [HttpGet("debug/radio-groups/{studentId}")]
        public async Task<IActionResult> DebugRadioGroups(int studentId)
        {
            var form = await _context.CareerPlanningForms.FirstOrDefaultAsync(f => f.StudentId == studentId);
            if (form == null) return NotFound();

            var templatePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "pdf-templates", "CareerPlanningFormTemplate.pdf");
            if (!System.IO.File.Exists(templatePath))
                return NotFound("Template not found");

            using var reader = new PdfReader(templatePath);
            using var pdfDoc = new PdfDocument(reader);
            var acro = PdfAcroForm.GetAcroForm(pdfDoc, false);
            var fields = acro?.GetFormFields();

            // Find all button fields (radio buttons and checkboxes)
            var buttonFields = fields?.Where(f => f.Value is PdfButtonFormField)
                                   .Select(f => new {
                                       Name = f.Key,
                                       FieldName = f.Value.GetFieldName(),
                                       FieldType = f.Value.GetFormType().ToString(),
                                       AppearanceStates = (f.Value as PdfButtonFormField)?.GetAppearanceStates()
                                   })
                                   .OrderBy(f => f.Name)
                                   .ToList();

            var debugInfo = new
            {
                StudentId = studentId,
                FirstChoice = form.FirstChoice,
                FirstChoiceNormalized = NormalizeYesNo(form.FirstChoice),
                ProgramChoiceReason = form.ProgramChoiceReason,
                DidChooseYes = string.IsNullOrWhiteSpace(form.ProgramChoiceReason) ||
                               string.Equals(form.ProgramChoiceReason.Trim(), "Me", StringComparison.OrdinalIgnoreCase) ||
                               string.Equals(form.ProgramChoiceReason.Trim(), "Yes", StringComparison.OrdinalIgnoreCase) ||
                               string.Equals(form.ProgramChoiceReason.Trim(), "Myself", StringComparison.OrdinalIgnoreCase),
                MainPlan = form.MainPlan,
                MainPlanNormalized = NormalizeMainPlan(form.MainPlan),
                ButtonFields = buttonFields,
                AllFields = fields?.Keys?.OrderBy(k => k).ToArray()
            };

            return Ok(debugInfo);
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

                // ---------- Text Fields ----------
                TrySetText(fields, font, "StudentNo", form.StudentNo);
                TrySetText(fields, font, "FullName", form.FullName);
                TrySetText(fields, font, "Program", form.Program);
                TrySetText(fields, font, "GradeYear", form.GradeYear);
                TrySetText(fields, font, "Section", form.Section);
                TrySetText(fields, font, "ContactNumber", form.ContactNumber);
                TrySetText(fields, font, "Birthday", form.Birthday);
                TrySetText(fields, font, "Gender", form.Gender);

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
                TrySetText(fields, font, "ProgramChoiceReason", form.ProgramChoiceReason);
                TrySetText(fields, font, "FullName_2", form.FullName);
                TrySetText(fields, font, "EmploymentNature", form.EmploymentNature);
                TrySetText(fields, font, "CurrentWorkNature", form.CurrentWorkNature);

                // ---------- Radio Buttons (Option Button Groups) ----------
                // "Was this your first choice?" - FirstChoice field
                var firstChoiceYes = string.Equals(NormalizeYesNo(form.FirstChoice), "Yes", StringComparison.OrdinalIgnoreCase);
                SetRadio(fields, "FirstChoice", firstChoiceYes ? "Yes" : "No");

                // "Did you choose this program?" - ProgramChoiceReason field
                // Logic: If ProgramChoiceReason is empty, "Me", "Yes", "Myself" → Yes
                // If it contains other text (like "test", "Parent", "Teacher") → No
                var didChooseYes = string.IsNullOrWhiteSpace(form.ProgramChoiceReason) ||
                                   string.Equals(form.ProgramChoiceReason.Trim(), "Me", StringComparison.OrdinalIgnoreCase) ||
                                   string.Equals(form.ProgramChoiceReason.Trim(), "Yes", StringComparison.OrdinalIgnoreCase) ||
                                   string.Equals(form.ProgramChoiceReason.Trim(), "Myself", StringComparison.OrdinalIgnoreCase);

                SetRadio(fields, "DidChooseProgram", didChooseYes ? "Yes" : "No");

                // ---------- Main plan (radio group) ----------
                var mainPlan = NormalizeMainPlan(form.MainPlan);
                SetRadio(fields, "MainPlan", mainPlan);

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
                var counselorNameQuery = Request?.Query.ContainsKey("assessedBy") == true ? Request.Query["assessedBy"].ToString() : null;
                var counselorName = !string.IsNullOrWhiteSpace(counselorNameQuery)
                    ? counselorNameQuery
                    : (
                        User?.FindFirst("name")?.Value ??
                        (User?.FindFirst("given_name")?.Value + " " + User?.FindFirst("family_name")?.Value) ??
                        User?.FindFirst("preferred_username")?.Value ??
                        User?.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value ??
                        User?.Identity?.Name ??
                        Environment.GetEnvironmentVariable("DEFAULT_COUNSELOR_NAME") ??
                        ""
                    );

                TrySetText(fields, font, "AssessedBy", counselorName?.Trim());
                TrySetText(fields, font, "DateSigned", form.SubmittedAt.ToString("MMMM dd, yyyy"));

                acro.FlattenFields();
            }

            return File(ms.ToArray(), "application/pdf", $"CareerPlanningForm_{studentId}.pdf");
        }

        // ===== iText 7 helpers =====
        private static void TrySetText(IDictionary<string, PdfFormField> fields, PdfFont font, string name, string? value, float size = 8f)
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
            if (string.IsNullOrWhiteSpace(value)) return false;

            // Log what we're trying to set
            Console.WriteLine($"Attempting to set radio: Group='{groupName}', Value='{value}'");

            // Try different naming patterns that PDF forms might use for radio groups
            var patterns = new[]
            {
        groupName,                                    // Direct group name
        $"{groupName}_{value}",                      // groupName_Yes, groupName_No
        $"{groupName}.{value}",                      // groupName.Yes, groupName.No
        $"{groupName}_{value}_1",                    // groupName_Yes_1
        $"{groupName}_{value}.1",                    // groupName_Yes.1
        value,                                       // Just the value (Yes, No)
        $"{groupName}Group",                         // groupNameGroup
        $"{groupName}Group_{value}",                 // groupNameGroup_Yes
        $"{groupName}_{value}Button",                // groupName_YesButton
        $"{groupName}Button_{value}",                // groupNameButton_Yes
        $"{groupName}_{value}_Option",               // groupName_Yes_Option
        $"{groupName}Option_{value}"                 // groupNameOption_Yes
    };

            foreach (var pattern in patterns)
            {
                Console.WriteLine($"Trying pattern: '{pattern}'");

                if (fields.TryGetValue(pattern, out var field))
                {
                    Console.WriteLine($"Found field: '{pattern}'");

                    try
                    {
                        // For radio buttons, set the value directly
                        field.SetValue(value);
                        Console.WriteLine($"Successfully set '{pattern}' to '{value}'");
                        return true;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Failed to set '{pattern}' to '{value}': {ex.Message}");

                        // If direct value setting fails, try setting the field as checked
                        if (field is PdfButtonFormField btn)
                        {
                            try
                            {
                                // Get the appearance states and set the appropriate one
                                var states = btn.GetAppearanceStates();
                                Console.WriteLine($"Appearance states for '{pattern}': [{string.Join(", ", states ?? new string[0])}]");

                                if (states != null && states.Length > 0)
                                {
                                    // Find a state that matches our value or use the first non-"Off" state
                                    var targetState = states.FirstOrDefault(s =>
                                        string.Equals(s, value, StringComparison.OrdinalIgnoreCase))
                                        ?? states.FirstOrDefault(s => !string.Equals(s, "Off", StringComparison.OrdinalIgnoreCase));

                                    if (targetState != null)
                                    {
                                        btn.SetValue(targetState);
                                        Console.WriteLine($"Successfully set '{pattern}' to state '{targetState}'");
                                        return true;
                                    }
                                }
                            }
                            catch (Exception ex2)
                            {
                                Console.WriteLine($"Failed to set appearance state for '{pattern}': {ex2.Message}");
                            }
                        }
                    }
                }
                else
                {
                    Console.WriteLine($"Field not found: '{pattern}'");
                }
            }

            Console.WriteLine($"Failed to set radio: Group='{groupName}', Value='{value}'");
            return false;
        }

        private static string NormalizeYesNo(string? v)
        {
            if (string.IsNullOrWhiteSpace(v)) return "";
            var s = v.Trim().ToLowerInvariant();
            if (s is "yes" or "y" or "true" or "1") return "Yes";
            if (s is "no" or "n" or "false" or "0") return "No";
            return v;
        }

        private static string NormalizeMainPlan(string? v)
        {
            if (string.IsNullOrWhiteSpace(v)) return "";
            var s = v.Trim().Replace(" ", string.Empty, StringComparison.OrdinalIgnoreCase);
            // Accept variants with spaces or different casing
            if (s.Equals("continueschooling", StringComparison.OrdinalIgnoreCase)) return "ContinueSchooling";
            if (s.Equals("getemployed", StringComparison.OrdinalIgnoreCase)) return "GetEmployed";
            if (s.Equals("continuewithcurrentwork", StringComparison.OrdinalIgnoreCase) || s.Equals("continuecurrentwork", StringComparison.OrdinalIgnoreCase)) return "ContinueCurrentWork";
            if (s.Equals("gointobusiness", StringComparison.OrdinalIgnoreCase)) return "GoIntoBusiness";
            return v;
        }
    }
}