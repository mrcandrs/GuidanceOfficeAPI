using System.ComponentModel.DataAnnotations;

namespace GuidanceOfficeAPI.Dtos
{
    public class CreateConsultationConferenceFormDto
    {
        [Required(ErrorMessage = "Student ID is required")]
        public int StudentId { get; set; }

        [Required(ErrorMessage = "Date is required")]
        public DateTime Date { get; set; }

        // Accept time as a string from frontend
        public string? Time { get; set; }

        [StringLength(50, ErrorMessage = "Grade/Year Level cannot exceed 50 characters")]
        public string? GradeYearLevel { get; set; }

        [StringLength(50, ErrorMessage = "Section cannot exceed 50 characters")]
        public string? Section { get; set; }

        [StringLength(2000, ErrorMessage = "Concerns cannot exceed 2000 characters")]
        public string? Concerns { get; set; }

        [StringLength(2000, ErrorMessage = "Remarks cannot exceed 2000 characters")]
        public string? Remarks { get; set; }

        [StringLength(100, ErrorMessage = "CounselorName cannot exceed 2000 characters")]
        public string? CounselorName { get; set; }

        [StringLength(100, ErrorMessage = "ParentGuardian cannot exceed 100 characters")]
        public string? ParentGuardian { get; set; }

        [StringLength(100, ErrorMessage = "School Personnel cannot exceed 100 characters")]
        public string? SchoolPersonnel { get; set; }

        [StringLength(100, ErrorMessage = "ParentContactNumber cannot exceed 100 characters")]
        public string? ParentContactNumber { get; set; }
    }
}
