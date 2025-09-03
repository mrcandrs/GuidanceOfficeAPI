using GuidanceOfficeAPI.Data;
using GuidanceOfficeAPI.Models;
using Microsoft.AspNetCore.Mvc;

namespace GuidanceOfficeAPI.Controllers
{
    /*[ApiController]
    [Route("api/[controller]")]
    public class StudentController : CrudController<Student>
    {
        public StudentController(AppDbContext context) : base(context) { }
    }*/

    [ApiController]
    [Route("api/[controller]")]
    public class ConsentFormController : CrudController<ConsentForm>
    {
        public ConsentFormController(AppDbContext context) : base(context) { }
    }

    /*[ApiController]
    [Route("api/[controller]")]
    public class InventoryFormController : CrudController<InventoryForm>
    {
        public InventoryFormController(AppDbContext context) : base(context) { }
    }*/ 

    [ApiController]
    [Route("api/[controller]")]
    public class CareerPlanningFormController : CrudController<CareerPlanningForm>
    {
        public CareerPlanningFormController(AppDbContext context) : base(context) { }
    }


    /*[ApiController]
    [Route("api/[controller]")]
    public class MoodTrackerController : CrudController<MoodTracker>
    {
        public MoodTrackerController(AppDbContext context) : base(context) { }
    }*/

    /*[ApiController]
    [Route("api/[controller]")]
    public class AppointmentRequestController : CrudController<AppointmentRequest>
    {
        public AppointmentRequestController(AppDbContext context) : base(context) { }
    }*/

    [ApiController]
    [Route("api/[controller]")]
    public class GuidancePassController : CrudController<GuidancePass>
    {
        public GuidancePassController(AppDbContext context) : base(context) { }
    }

    [ApiController]
    [Route("api/[controller]")]
    public class ExitInterviewFormController : CrudController<ExitInterviewForm>
    {
        public ExitInterviewFormController(AppDbContext context) : base(context) { }
    }

    [ApiController]
    [Route("api/[controller]")]
    public class ReferralFormController : CrudController<ReferralForm>
    {
        public ReferralFormController(AppDbContext context) : base(context) { }
    }

    [ApiController]
    [Route("api/[controller]")]
    public class EndorsementCustodyFormController : CrudController<EndorsementCustodyForm>
    {
        public EndorsementCustodyFormController(AppDbContext context) : base(context) { }
    }

    [ApiController]
    [Route("api/[controller]")]
    public class ConsultationFormController : CrudController<ConsultationForm>
    {
        public ConsultationFormController(AppDbContext context) : base(context) { }
    }

    [ApiController]
    [Route("api/[controller]")]
    public class CounselorController : CrudController<Counselor>
    {
        public CounselorController(AppDbContext context) : base(context) { }
    }

    [ApiController]
    [Route("api/[controller]")]
    public class GuidanceNoteController : CrudController<GuidanceNote>
    {
        public GuidanceNoteController(AppDbContext context) : base(context) { }
    }
}
