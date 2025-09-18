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
        public string StudentProgram { get; set; }
        public string Section { get; set; }

        // referral form fields (add these missing ones)
        public string FullName { get; set; }
        public string Program { get; set; }
        public int? Age { get; set; }
        public string Gender { get; set; }
        public string AcademicLevel { get; set; }
        public string ReferredBy { get; set; }
        public string AreasOfConcern { get; set; }
        public string AreasOfConcernOtherDetail { get; set; }
        public string ActionRequested { get; set; }
        public string ActionRequestedOtherDetail { get; set; }
        public string PriorityLevel { get; set; }
        public string PriorityDate { get; set; }
        public string ActionsTakenBefore { get; set; }
        public string ReferralReasons { get; set; }
        public string CounselorInitialAction { get; set; }
        public string PersonWhoReferred { get; set; }
        public DateTime DateReferred { get; set; }

        // counselor feedback fields
        public string CounselorFeedbackStudentName { get; set; }
        public DateTime? CounselorFeedbackDateReferred { get; set; }
        public DateTime? CounselorSessionDate { get; set; }
        public string CounselorActionsTaken { get; set; }
        public string CounselorName { get; set; }
    }
}