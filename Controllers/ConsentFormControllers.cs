using GuidanceOfficeAPI.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using iText.Kernel.Pdf;
using iText.Forms;
using iText.Forms.Fields;
using iText.Kernel.Font;
using iText.IO.Font.Constants;
using System.Text;
using iText.IO.Font;

namespace GuidanceOfficeAPI.Controllers
{
    [ApiController]
    [Route("api/consentform")]
    public class ConsentFormsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly TimeZoneInfo _manilaTz;

        public ConsentFormsController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
            _manilaTz = TimeZoneInfo.FindSystemTimeZoneById("Asia/Manila");
        }

        [HttpGet("{id:int}/pdf")]
        public async Task<IActionResult> GetConsentFormPdf([FromRoute] int id)
        {
            try
            {
                // 1) Load your data
                var form = await _context.ConsentForms
                    .Include(c => c.Student)
                    .Include(c => c.Counselor)
                    .FirstOrDefaultAsync(c => c.ConsentId == id);

                if (form == null)
                    return NotFound($"Consent form with ID {id} not found.");

                // 2) Locate template - check multiple possible paths
                string templatePath = null;
                var possiblePaths = new[]
                {
                    Path.Combine(_env.WebRootPath ?? "", "pdf-templates", "ConsentFormTemplate.pdf"),
                    Path.Combine(_env.ContentRootPath, "wwwroot", "pdf-templates", "ConsentFormTemplate.pdf"),
                    Path.Combine(_env.ContentRootPath, "pdf-templates", "ConsentFormTemplate.pdf"),
                    Path.Combine(_env.ContentRootPath, "Templates", "ConsentFormTemplate.pdf")
                };

                foreach (var path in possiblePaths)
                {
                    if (!string.IsNullOrEmpty(path) && System.IO.File.Exists(path))
                    {
                        templatePath = path;
                        break;
                    }
                }

                if (string.IsNullOrEmpty(templatePath))
                {
                    return NotFound("Consent form template not found on server. Please ensure ConsentFormTemplate.pdf exists in the pdf-templates directory.");
                }

                // Prepare data outside the using block
                string studentName = form.Student?.FullName ?? "N/A";
                string studentNumber = form.Student?.StudentNumber ?? "N/A";
                string parentName = form.ParentName ?? "N/A";
                string counselorName = form.Counselor?.Name ?? "N/A";
                string studentId = form.StudentId?.ToString() ?? "N/A";
                string consentId = form.ConsentId.ToString();

                // Handle SignedDate (non-nullable DateTime)
                string signedDate = form.SignedDate == DateTime.MinValue
                    ? "N/A"
                    : TimeZoneInfo.ConvertTimeFromUtc(
                          DateTime.SpecifyKind(form.SignedDate, DateTimeKind.Utc), _manilaTz
                      ).ToString("MMMM dd, yyyy");

                // 3) Fill the template
                using var ms = new MemoryStream();

                // Use FileStream to read the template to avoid file locking issues
                using (var templateStream = new FileStream(templatePath, FileMode.Open, FileAccess.Read))
                using (var reader = new PdfReader(templateStream))
                using (var writer = new PdfWriter(ms))
                using (var pdf = new PdfDocument(reader, writer))
                {
                    var acroForm = PdfAcroForm.GetAcroForm(pdf, true);
                    if (acroForm == null) return BadRequest("The PDF template does not contain fillable form fields.");

                    var fields = acroForm.GetFormFields();

                    // Ensure text renders
                    acroForm.SetNeedAppearances(true);
                    acroForm.SetGenerateAppearance(true);

                    // Declare font BEFORE using it anywhere
                    var font = PdfFontFactory.CreateFont(StandardFonts.HELVETICA, PdfEncodings.WINANSI);

                    // Optional: log actual field names so you can align LibreOffice names
                    foreach (var kv in fields)
                    {
                        Console.WriteLine($"PDF field: '{kv.Key}' type={kv.Value.GetFormType()}");
                    }

                    // Helper: try aliases and then case-insensitive match
                    void TrySetAny(string[] names, string value, float size = 11f)
                    {
                        if (string.IsNullOrWhiteSpace(value)) return;

                        foreach (var n in names)
                        {
                            if (fields.TryGetValue(n, out var f))
                            {
                                try { f.SetValue(value, font, size); return; } catch { /* try next */ }
                            }
                        }

                        var match = fields.Keys.FirstOrDefault(k => string.Equals(k, names[0], StringComparison.OrdinalIgnoreCase));
                        if (match != null)
                        {
                            try { fields[match].SetValue(value, font, size); } catch { }
                        }
                    }

                    // Fill fields (add/adjust aliases to match your LibreOffice field names)
                    TrySetAny(new[] { "StudentName", "Student_Name", "Name_Student", "Text_StudentName" }, studentName);
                    TrySetAny(new[] { "StudentNumber", "StudentNo", "Student_Number" }, studentNumber);
                    TrySetAny(new[] { "ParentName", "Parent_Guardian_Name" }, parentName);
                    TrySetAny(new[] { "CounselorName", "Counselor_Name" }, counselorName);
                    TrySetAny(new[] { "SignedDate", "DateSigned", "Date_Signed" }, signedDate);
                    TrySetAny(new[] { "StudentId", "Student_ID" }, studentId);
                    TrySetAny(new[] { "ConsentId", "Consent_ID" }, consentId);

                    // Checkbox: pick real "on" value from appearance states (compatible with your iText)
                    if (fields.TryGetValue("IsAgreed", out var agreeField) && agreeField is PdfButtonFormField cb)
                    {
                        var states = cb.GetAppearanceStates(); // e.g. ["Off","Yes"] or ["Off","On"]
                        var onValue = states?.FirstOrDefault(s => !string.Equals(s, "Off", StringComparison.OrdinalIgnoreCase)) ?? "Yes";
                        try { cb.SetValue(form.IsAgreed ? onValue : "Off"); } catch { }
                    }

                    // Make non-editable
                    acroForm.FlattenFields();
                }

                var pdfBytes = ms.ToArray();

                // 5) Return the PDF file
                // Use StudentNumber for filename, fallback to StudentName, then ConsentId
                string fileIdentifier = studentNumber != "N/A" ? studentNumber :
                                       (studentName != "N/A" ? studentName.Replace(" ", "_") :
                                       consentId);
                var fileName = $"ConsentForm_{fileIdentifier}.pdf";

                // Set proper headers for PDF download
                Response.Headers.Add("Content-Disposition", $"attachment; filename=\"{fileName}\"");
                Response.Headers.Add("Content-Type", "application/pdf");
                Response.Headers.Add("Content-Length", pdfBytes.Length.ToString());

                return File(pdfBytes, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                // Log the full exception for debugging
                Console.WriteLine($"Error generating PDF: {ex}");
                return StatusCode(500, $"An error occurred while generating the PDF: {ex.Message}");
            }
        }
    }
}