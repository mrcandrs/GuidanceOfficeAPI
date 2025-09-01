namespace GuidanceOfficeAPI.Dtos
{
    public class ConsultationConferenceFormDto
    {
        public int ConsultationId { get; set; }
        public int StudentId { get; set; }
        public int CounselorId { get; set; }
        public DateTime Date { get; set; }
        public string? Time { get; set; }
        public string? GradeYearLevel { get; set; }
        public string? Section { get; set; }
        public string? Concerns { get; set; }
        public string? Remarks { get; set; }
        public string? CounselorName { get; set; }
        public string? ParentGuardian { get; set; }
        public string? SchoolPersonnel { get; set; }
        public string? ParentContactNumber { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public StudentDto? Student { get; set; }
        public CounselorDto? Counselor { get; set; }
    }
}
