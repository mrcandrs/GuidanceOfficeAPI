using System.ComponentModel.DataAnnotations;

namespace GuidanceOfficeAPI.Dtos
    {
        // DTO for returning endorsement custody form data
        public class EndorsementCustodyFormDto
        {
            public int CustodyId { get; set; }
            public int StudentId { get; set; }
            public int CounselorId { get; set; }
            public DateTime Date { get; set; }
            public string? GradeYearLevel { get; set; }
            public string? Section { get; set; }
            public string? Concerns { get; set; }
            public string? Interventions { get; set; }
            public string? Recommendations { get; set; }
            public string? Referrals { get; set; }
            public string? EndorsedBy { get; set; }
            public string? EndorsedTo { get; set; }
            public DateTime CreatedAt { get; set; }
            public DateTime? UpdatedAt { get; set; }
            public StudentDto? Student { get; set; }
            public CounselorDto? Counselor { get; set; }
        }
    }
