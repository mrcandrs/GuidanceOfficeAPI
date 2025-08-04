using GuidanceOfficeAPI.Data;
using GuidanceOfficeAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GuidanceOfficeAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InventoryController : ControllerBase
    {
        private readonly AppDbContext _context; // Replace with your DbContext name

        public InventoryController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/inventory/{studentId}
        [HttpGet("{studentId}")]
        public async Task<ActionResult<InventoryForm>> GetIndividualInventoryForm(int studentId)
        {
            try
            {
                var inventoryForm = await _context.InventoryForms
                    .Include(i => i.Siblings)
                    .Include(i => i.WorkExperience)
                    .FirstOrDefaultAsync(i => i.StudentId == studentId);

                if (inventoryForm == null)
                {
                    return NotFound($"No inventory form found for student ID: {studentId}");
                }

                //Convert to format expected by Android
                var response = new
                {
                    studentId = inventoryForm.StudentId,
                    fullName = inventoryForm.FullName,
                    studentNumber = inventoryForm.StudentNumber,
                    program = inventoryForm.Program,
                    nickname = inventoryForm.Nickname,
                    nationality = inventoryForm.Nationality,
                    gender = inventoryForm.Gender,
                    civilStatus = inventoryForm.CivilStatus,
                    religion = inventoryForm.Religion,
                    birthday = inventoryForm.Birthday?.ToString("yyyy-MM-dd"), //Convert DateTime to string
                    phoneNumber = inventoryForm.PhoneNumber,
                    email1 = inventoryForm.Email1,
                    email2 = inventoryForm.Email2,
                    presentAddress = inventoryForm.PresentAddress,
                    permanentAddress = inventoryForm.PermanentAddress,
                    provincialAddress = inventoryForm.ProvincialAddress,

                    //Spouse Info
                    spouseName = inventoryForm.SpouseName,
                    spouseAge = inventoryForm.SpouseAge,
                    spouseOccupation = inventoryForm.SpouseOccupation,
                    spouseContact = inventoryForm.SpouseContact,

                    //Family Info
                    fatherName = inventoryForm.FatherName,
                    fatherOccupation = inventoryForm.FatherOccupation,
                    fatherContact = inventoryForm.FatherContact,
                    fatherIncome = inventoryForm.FatherIncome,
                    motherName = inventoryForm.MotherName,
                    motherOccupation = inventoryForm.MotherOccupation,
                    motherContact = inventoryForm.MotherContact,
                    motherIncome = inventoryForm.MotherIncome,
                    fatherStatus = inventoryForm.FatherStatus,
                    motherStatus = inventoryForm.MotherStatus,
                    guardianName = inventoryForm.GuardianName,
                    guardianContact = inventoryForm.GuardianContact,

                    //Education
                    elementary = inventoryForm.Elementary,
                    juniorHigh = inventoryForm.JuniorHigh,
                    seniorHigh = inventoryForm.SeniorHigh,
                    college = inventoryForm.College,

                    //Interests
                    sports = inventoryForm.Sports,
                    hobbies = inventoryForm.Hobbies,
                    talents = inventoryForm.Talents,
                    socioCivic = inventoryForm.SocioCivic,
                    schoolOrg = inventoryForm.SchoolOrg,

                    //Health
                    wasHospitalized = inventoryForm.WasHospitalized,
                    hospitalizedReason = inventoryForm.HospitalizedReason,
                    hadOperation = inventoryForm.HadOperation,
                    operationReason = inventoryForm.OperationReason,
                    hasIllness = inventoryForm.HasIllness,
                    illnessDetails = inventoryForm.IllnessDetails,
                    takesMedication = inventoryForm.TakesMedication,
                    medicationDetails = inventoryForm.MedicationDetails,
                    hasMedicalCertificate = inventoryForm.HasMedicalCertificate,
                    familyIllness = inventoryForm.FamilyIllness,
                    lastDoctorVisit = inventoryForm.LastDoctorVisit?.ToString("yyyy-MM-dd"), //Convert DateTime to string
                    visitReason = inventoryForm.VisitReason,

                    //Life Circumstances
                    lossExperience = inventoryForm.LossExperience,
                    problems = inventoryForm.Problems,
                    relationshipConcerns = inventoryForm.RelationshipConcerns,

                    //Collections
                    siblings = inventoryForm.Siblings?.Select(s => new {
                        name = s.Name,
                        age = s.Age,
                        gender = s.Gender,
                        programOrOccupation = s.ProgramOrOccupation,
                        schoolOrCompany = s.SchoolOrCompany
                    }).ToList(),

                    workExperience = inventoryForm.WorkExperience?.Select(w => new {
                        company = w.Company,
                        position = w.Position,
                        duration = w.Duration
                    }).ToList()
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // PUT: api/inventory/{studentId}
        [HttpPut("{studentId}")]
        public async Task<IActionResult> UpdateInventory(int studentId, [FromBody] InventoryForm form)
        {
            try
            {
                if (studentId != form.StudentId)
                {
                    return BadRequest("Student ID mismatch");
                }

                var existingForm = await _context.InventoryForms
                    .Include(i => i.Siblings)
                    .Include(i => i.WorkExperience)
                    .FirstOrDefaultAsync(i => i.StudentId == studentId);

                if (existingForm == null)
                {
                    return NotFound($"No inventory form found for student ID: {studentId}");
                }

                // Update all properties
                UpdateInventoryFormProperties(existingForm, form);

                // Handle siblings
                _context.Siblings.RemoveRange(existingForm.Siblings);
                if (form.Siblings != null)
                {
                    foreach (var sibling in form.Siblings)
                    {
                        sibling.InventoryFormId = existingForm.InventoryId;
                        existingForm.Siblings.Add(sibling);
                    }
                }

                // Handle work experiences
                _context.WorkExperiences.RemoveRange(existingForm.WorkExperience);
                if (form.WorkExperience != null)
                {
                    foreach (var work in form.WorkExperience)
                    {
                        work.InventoryFormId = existingForm.InventoryId;
                        existingForm.WorkExperience.Add(work);
                    }
                }

                await _context.SaveChangesAsync();
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        private void UpdateInventoryFormProperties(InventoryForm existing, InventoryForm updated)
        {
            // Basic Info
            existing.FullName = updated.FullName;
            existing.StudentNumber = updated.StudentNumber;
            existing.Program = updated.Program;
            existing.Nickname = updated.Nickname;
            existing.Nationality = updated.Nationality;
            existing.Gender = updated.Gender;
            existing.CivilStatus = updated.CivilStatus;
            existing.Religion = updated.Religion;
            existing.Birthday = updated.Birthday;
            existing.PhoneNumber = updated.PhoneNumber;
            existing.Email1 = updated.Email1;
            existing.Email2 = updated.Email2;
            existing.PresentAddress = updated.PresentAddress;
            existing.PermanentAddress = updated.PermanentAddress;
            existing.ProvincialAddress = updated.ProvincialAddress;

            // Spouse Info
            existing.SpouseName = updated.SpouseName;
            existing.SpouseAge = updated.SpouseAge;
            existing.SpouseOccupation = updated.SpouseOccupation;
            existing.SpouseContact = updated.SpouseContact;

            // Family Info
            existing.FatherName = updated.FatherName;
            existing.FatherOccupation = updated.FatherOccupation;
            existing.FatherContact = updated.FatherContact;
            existing.FatherIncome = updated.FatherIncome;
            existing.MotherName = updated.MotherName;
            existing.MotherOccupation = updated.MotherOccupation;
            existing.MotherContact = updated.MotherContact;
            existing.MotherIncome = updated.MotherIncome;
            existing.FatherStatus = updated.FatherStatus;
            existing.MotherStatus = updated.MotherStatus;
            existing.GuardianName = updated.GuardianName;
            existing.GuardianContact = updated.GuardianContact;

            // Education
            existing.Elementary = updated.Elementary;
            existing.JuniorHigh = updated.JuniorHigh;
            existing.SeniorHigh = updated.SeniorHigh;
            existing.College = updated.College;

            // Interests
            existing.Sports = updated.Sports;
            existing.Hobbies = updated.Hobbies;
            existing.Talents = updated.Talents;
            existing.SocioCivic = updated.SocioCivic;
            existing.SchoolOrg = updated.SchoolOrg;

            // Health
            existing.WasHospitalized = updated.WasHospitalized;
            existing.HospitalizedReason = updated.HospitalizedReason;
            existing.HadOperation = updated.HadOperation;
            existing.OperationReason = updated.OperationReason;
            existing.HasIllness = updated.HasIllness;
            existing.IllnessDetails = updated.IllnessDetails;
            existing.TakesMedication = updated.TakesMedication;
            existing.MedicationDetails = updated.MedicationDetails;
            existing.HasMedicalCertificate = updated.HasMedicalCertificate;
            existing.FamilyIllness = updated.FamilyIllness;
            existing.LastDoctorVisit = updated.LastDoctorVisit;
            existing.VisitReason = updated.VisitReason;

            // Life Circumstances
            existing.LossExperience = updated.LossExperience;
            existing.Problems = updated.Problems;
            existing.RelationshipConcerns = updated.RelationshipConcerns;
        }
    }
}
