using System.ComponentModel.DataAnnotations;

namespace GuidanceOfficeAPI.Dtos
{
    // DTO for updating an existing endorsement custody form
    public class UpdateEndorsementCustodyFormDto
    {
        [Required(ErrorMessage = "Student ID is required")]
        public int StudentId { get; set; }

        [Required(ErrorMessage = "Date is required")]
        public DateTime Date { get; set; }

        [StringLength(50, ErrorMessage = "Grade/Year Level cannot exceed 50 characters")]
        public string? GradeYearLevel { get; set; }

        [StringLength(50, ErrorMessage = "Section cannot exceed 50 characters")]
        public string? Section { get; set; }

        [StringLength(2000, ErrorMessage = "Comments cannot exceed 2000 characters")]
        public string? Concerns { get; set; }

        [StringLength(2000, ErrorMessage = "Interventions cannot exceed 2000 characters")]
        public string? Interventions { get; set; }

        [StringLength(2000, ErrorMessage = "Recommendations cannot exceed 2000 characters")]
        public string? Recommendations { get; set; }

        [StringLength(1000, ErrorMessage = "Referrals cannot exceed 1000 characters")]
        public string? Referrals { get; set; }

        [StringLength(100, ErrorMessage = "Endorsed By cannot exceed 100 characters")]
        public string? EndorsedBy { get; set; }

        [StringLength(100, ErrorMessage = "Endorsed To cannot exceed 100 characters")]
        public string? EndorsedTo { get; set; }
    }
}
