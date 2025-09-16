namespace GuidanceOfficeAPI.Dtos
{
    public class ReferralLatestDto
    {
        public int ReferralId { get; set; }
        public int StudentId { get; set; }
        public DateTime SubmissionDate { get; set; }

        // canonical student fields
        public string StudentFullName { get; set; }
        public string StudentNumber { get; set; }
        public string Program { get; set; }
        public string Section { get; set; } // from CareerPlanningForm

        // referral fields you still use
        public string FullName { get; set; }
        public string PersonWhoReferred { get; set; }
        public DateTime DateReferred { get; set; }
        public string CounselorFeedbackStudentName { get; set; }
        public DateTime? CounselorFeedbackDateReferred { get; set; }
        public DateTime? CounselorSessionDate { get; set; }
        public string CounselorActionsTaken { get; set; }
        public string CounselorName { get; set; }
    }
}
