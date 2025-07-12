using GuidanceOfficeAPI.Models;

namespace GuidanceOfficeAPI.Dtos
{
    public class FullRegistrationDto
    {
        public Student Student { get; set; }
        public ConsentForm ConsentForm { get; set; }
        public InventoryForm InventoryForm { get; set; }
        public CareerPlanningForm CareerPlanningForm { get; set; }
    }
}
