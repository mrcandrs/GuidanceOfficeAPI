using System.ComponentModel.DataAnnotations;

namespace GuidanceOfficeAPI.Dtos
{
    public class UpdateGuidanceNoteDto
    {
        [Required]
        public int StudentId { get; set; }
        [Required]
        public DateTime InterviewDate { get; set; }
        public string TimeStarted { get; set; }
        public string TimeEnded { get; set; }
        public string SchoolYear { get; set; }
        public string TertiarySemester { get; set; }
        public string SeniorHighQuarter { get; set; }
        public string GradeYearLevelSection { get; set; }
        public string Program { get; set; }
        // Nature of Counseling
        public bool IsAcademic { get; set; }
        public bool IsBehavioral { get; set; }
        public bool IsPersonal { get; set; }
        public bool IsSocial { get; set; }
        public bool IsCareer { get; set; }
        // Counseling Situation
        public bool IsIndividual { get; set; }
        public bool IsGroup { get; set; }
        public bool IsClass { get; set; }
        public bool IsCounselorInitiated { get; set; }
        public bool IsWalkIn { get; set; }
        public bool IsFollowUp { get; set; }
        public string ReferredBy { get; set; }
        // Notes sections
        [Required]
        public string PresentingProblem { get; set; }
        public string Assessment { get; set; }
        public string Interventions { get; set; }
        public string PlanOfAction { get; set; }
        // Recommendations
        public bool IsFollowThroughSession { get; set; }
        public DateTime? FollowThroughDate { get; set; }
        public bool IsReferral { get; set; }
        public string ReferralAgencyName { get; set; }
        // Counselor name (new field)
        public string CounselorName { get; set; }
    }
}
