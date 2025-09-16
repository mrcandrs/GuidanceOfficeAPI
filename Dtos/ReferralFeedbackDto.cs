namespace GuidanceOfficeAPI.Dtos
{
    public class ReferralFeedbackDto
    {
        public string? CounselorFeedbackStudentName { get; set; }
        public DateTime? CounselorFeedbackDateReferred { get; set; }
        public DateTime? CounselorSessionDate { get; set; }
        public string? CounselorActionsTaken { get; set; } // e.g., "Counseling, Classroom Observation"
        public string? CounselorName { get; set; }
    }
}
