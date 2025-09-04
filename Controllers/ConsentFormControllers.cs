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

                    if (acroForm == null)
                    {
                        return BadRequest("The PDF template does not contain fillable form fields.");
                    }

                    // For iText 7.2.5 - this method works
                    var fields = acroForm.GetFormFields();

                    // Create font for filling fields
                    var font = PdfFontFactory.CreateFont(StandardFonts.HELVETICA, PdfEncodings.WINANSI);
                    acroForm.SetGenerateAppearance(true);

                    // Helper function to safely set field values
                    void TrySet(string fieldName, string value, float size = 11f)
                    {
                        if (fields.TryGetValue(fieldName, out var field) && !string.IsNullOrWhiteSpace(value))
                        {
                            try
                            {
                                field.SetValue(value, font, size);
                            }
                            catch (Exception ex)
                            {
                                // Log the error but continue processing other fields
                                Console.WriteLine($"Error setting field {fieldName}: {ex.Message}");
                            }
                        }
                    }

                    // Set field values
                    TrySet("StudentName", studentName);
                    TrySet("StudentNumber", studentNumber);
                    TrySet("ParentName", parentName);
                    TrySet("CounselorName", counselorName);
                    TrySet("SignedDate", signedDate);
                    TrySet("StudentId", studentId);
                    TrySet("ConsentId", consentId);

                    // Handle checkbox for agreement
                    if (fields.TryGetValue("IsAgreed", out var agreeField))
                    {
                        try
                        {
                            if (agreeField is PdfButtonFormField checkbox)
                            {
                                // Try different possible values for the checkbox
                                var checkValue = form.IsAgreed ? "Yes" : "Off";
                                checkbox.SetValue(checkValue);
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error setting checkbox: {ex.Message}");
                        }
                    }

                    // 4) Flatten the form to make it non-editable
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