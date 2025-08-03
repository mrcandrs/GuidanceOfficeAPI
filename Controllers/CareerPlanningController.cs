using GuidanceOfficeAPI.Data;
using GuidanceOfficeAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
    }

}
