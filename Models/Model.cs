using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography.X509Certificates;

namespace GuidanceOfficeAPI.Models
{

    [Index(nameof(StudentNumber), IsUnique = true)]
    [Index(nameof(Email), IsUnique = true)]
    public class Student
    {
        [Key]
        public int StudentId { get; set; }

        [Required, MaxLength(11)]
        public string StudentNumber { get; set; }

        public string FullName { get; set; }

        public string Program { get; set; }

        public string GradeYear { get; set; }

        public string Email { get; set; }

        public string Username { get; set; }

        [Required, MaxLength(255)]
        public string Password { get; set; }

        public byte[]? ProfileImage { get; set; } // Save image as byte[]

        public DateTime DateRegistered { get; set; } = DateTime.Now;
        public DateTime? LastLogin { get; set; }
    }

    public class ConsentForm
    {
        [Key]
        public int ConsentId { get; set; }

        public int? StudentId { get; set; }

        public string ParentName { get; set; }

        public DateTime SignedDate { get; set; }
        public bool IsAgreed { get; set; }

        public int? CounselorId { get; set; }

        [ForeignKey("StudentId")]
        public Student? Student { get; set; }

        [ForeignKey("CounselorId")]
        public Counselor? Counselor { get; set; }

    }

    public class InventoryForm
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // ✅ Ensures auto-increment
        public int InventoryId { get; set; }

        public int StudentId { get; set; }

        [ForeignKey("StudentId")]
        public Student? Student { get; set; }

        public DateTime? SubmissionDate { get; set; }

        // Form fields
        public string? FullName { get; set; }
        public string? StudentNumber { get; set; }
        public string? Program { get; set; }

        // Basic Info
        public string? Nickname { get; set; }
        public string? Nationality { get; set; }
        public string? Gender { get; set; }
        public string? CivilStatus { get; set; }
        public string? Religion { get; set; }
        public DateTime? Birthday { get; set; }

        // Contact Info
        public string? PhoneNumber { get; set; }
        public string? Email1 { get; set; }
        public string? Email2 { get; set; }
        public string? PresentAddress { get; set; }
        public string? PermanentAddress { get; set; }
        public string? ProvincialAddress { get; set; }

        // Spouse Info
        public string? SpouseName { get; set; }
        public int? SpouseAge { get; set; }
        public string? SpouseOccupation { get; set; }
        public string? SpouseContact { get; set; }

        // Family Background
        public string? FatherName { get; set; }
        public string? FatherOccupation { get; set; }
        public string? FatherContact { get; set; }
        public string? FatherIncome { get; set; }

        public string? MotherName { get; set; }
        public string? MotherOccupation { get; set; }
        public string? MotherContact { get; set; }
        public string? MotherIncome { get; set; }

        public string? FatherStatus { get; set; }
        public string? MotherStatus { get; set; }

        public string? GuardianName { get; set; }
        public string? GuardianContact { get; set; }

        // Educational Background
        public string? Elementary { get; set; }
        public string? JuniorHigh { get; set; }
        public string? SeniorHigh { get; set; }
        public string? College { get; set; }

        // Interests
        public string? Sports { get; set; }
        public string? Hobbies { get; set; }
        public string? Talents { get; set; }
        public string? SocioCivic { get; set; }
        public string? SchoolOrg { get; set; }

        // Health History
        public bool? WasHospitalized { get; set; }
        public string? HospitalizedReason { get; set; }
        public bool? HadOperation { get; set; }
        public string? OperationReason { get; set; }
        public bool? HasIllness { get; set; }
        public string? IllnessDetails { get; set; }
        public bool? TakesMedication { get; set; }
        public string? MedicationDetails { get; set; }
        public bool? HasMedicalCertificate { get; set; }

        public string? FamilyIllness { get; set; }
        public DateTime? LastDoctorVisit { get; set; }
        public string? VisitReason { get; set; }

        // Life Circumstances
        public string? LossExperience { get; set; }
        public string? Problems { get; set; }
        public string? RelationshipConcerns { get; set; }

        // ✅ Relationships
        public ICollection<Sibling>? Siblings { get; set; }
        public ICollection<WorkExperience>? WorkExperience { get; set; }
    }

    public class Sibling
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // ✅ Important to auto-generate ID
        public int SiblingId { get; set; }

        public int InventoryFormId { get; set; }

        [ForeignKey("InventoryFormId")]
        [JsonIgnore]
        public InventoryForm? InventoryForm { get; set; }

        public string? Name { get; set; }
        public int? Age { get; set; }
        public string? Gender { get; set; }
        public string? ProgramOrOccupation { get; set; }
        public string? SchoolOrCompany { get; set; }
    }

    public class WorkExperience
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // ✅ Important to auto-generate ID
        public int WorkId { get; set; }

        public int InventoryFormId { get; set; }

        [ForeignKey("InventoryFormId")]
        [JsonIgnore]
        public InventoryForm? InventoryForm { get; set; }

        public string? Company { get; set; }
        public string? Position { get; set; }
        public string? Duration { get; set; }
    }




    public class CareerPlanningForm
    {
        [Key]
        public int CareerId { get; set; }

        public int StudentId { get; set; }

        // Personal Information
        public string StudentNo { get; set; }
        public string FullName { get; set; }
        public string Program { get; set; }
        public string GradeYear { get; set; }
        public string Section { get; set; }
        public string Gender { get; set; }
        public string ContactNumber { get; set; }
        public string Birthday { get; set; }

        // Self-Assessment
        public string TopValue1 { get; set; }
        public string TopValue2 { get; set; }
        public string TopValue3 { get; set; }

        public string TopStrength1 { get; set; }
        public string TopStrength2 { get; set; }
        public string TopStrength3 { get; set; }

        public string TopSkill1 { get; set; }
        public string TopSkill2 { get; set; }
        public string TopSkill3 { get; set; }

        public string TopInterest1 { get; set; }
        public string TopInterest2 { get; set; }
        public string TopInterest3 { get; set; }

        // Career Choices
        [Column(TypeName = "text")]
        public string ProgramChoice { get; set; }

        [Column(TypeName = "text")]
        public string NatureJob1 { get; set; }

        [Column(TypeName = "text")]
        public string NatureJob2 { get; set; }
        public string ProgramChoiceReason { get; set; }
        public string FirstChoice { get; set; }
        public string OriginalChoice { get; set; }

        [Column(TypeName = "text")]
        public string ProgramExpectation { get; set; }

        [Column(TypeName = "text")]
        public string EnrollmentReason { get; set; }

        [Column(TypeName = "text")]
        public string FutureVision { get; set; }

        // Plans After Graduation
        public string MainPlan { get; set; } // Continue Schooling, Get Employed, etc.

        public bool AnotherCourse { get; set; }
        public bool MastersProgram { get; set; }
        public string CourseField { get; set; }

        public bool LocalEmployment { get; set; }
        public bool WorkAbroad { get; set; }
        public string EmploymentNature { get; set; }

        public bool AimPromotion { get; set; }
        public bool CurrentWorkAbroad { get; set; }
        public string CurrentWorkNature { get; set; }

        public string BusinessNature { get; set; }

        public DateTime SubmittedAt { get; set; }

        [ForeignKey("StudentId")]
        public Student? Student { get; set; }
    }

    public class MoodTracker
    {
        [Key]
        public int MoodId { get; set; }

        [Required]
        public int StudentId { get; set; }

        [Required]
        [MaxLength(50)]
        public string MoodLevel { get; set; }

        public DateTime EntryDate { get; set; } = DateTime.Now;

        /*[ForeignKey("StudentId")]
        public Student Student { get; set; }*/
    }

    public class GuidanceAppointment
    {
        [Key]
        public int AppointmentId { get; set; }

        [Required]
        public int StudentId { get; set; }

        [Required]
        [StringLength(100)]
        public string StudentName { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string ProgramSection { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string Reason { get; set; } = string.Empty;

        [Required]
        [StringLength(10)]
        public string Date { get; set; } = string.Empty;

        [Required]
        [StringLength(10)]
        public string Time { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "pending";

        [StringLength(500)]
        public string? RejectionReason { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? UpdatedAt { get; set; }
    }


    public class GuidancePass
    {
        [Key]
        public int PassId { get; set; }

        public int AppointmentId { get; set; }
        public DateTime IssuedDate { get; set; }
        public string Notes { get; set; }
        public int CounselorId { get; set; }

        [ForeignKey("AppointmentId")]
        public GuidanceAppointment Appointment { get; set; }

        [ForeignKey("CounselorId")]
        public Counselor Counselor { get; set; }
    }

    public class ExitInterviewForm
    {
        [Key]
        public int ExitFormId { get; set; }

        public int StudentId { get; set; }

        [ForeignKey("StudentId")]

        public DateTime SubmittedAt { get; set; }

        // Step 1
        public string? MainReason { get; set; }
        public string? SpecificReasons { get; set; }
        public string? OtherReason { get; set; }
        public string? PlansAfterLeaving { get; set; }

        // Step 2
        public string? ValuesLearned { get; set; }
        public string? SkillsLearned { get; set; }

        // Step 3
        public string? ServiceResponsesJson { get; set; }
        public string? OtherServicesDetail { get; set; }
        public string? OtherActivitiesDetail { get; set; }

        // Step 4
        public string? Comments { get; set; }
    }

    public class ReferralForm
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ReferralId { get; set; }

        //Relationship
        public int StudentId { get; set; }
        [ForeignKey("StudentId")]
        public Student? Student { get; set; }

        //Basic Info
        public string FullName { get; set; }
        public string StudentNumber { get; set; }
        public string Program { get; set; }
        public int Age { get; set; }
        public string Gender { get; set; }

        //Academic Level (from your chip logic)
        public string AcademicLevel { get; set; } // e.g., "Junior High", "Grade 11", etc.

        //Chips - These should be comma-separated lists or a separate table if normalized
        public string ReferredBy { get; set; } //e.g., "Student, Parent"
        public string AreasOfConcern { get; set; }
        public string AreasOfConcernOtherDetail { get; set; } //if "Others" selected
        public string ActionRequested { get; set; }
        public string ActionRequestedOtherDetail { get; set; } //if "Others" selected

        public string PriorityLevel { get; set; } //"Emergency", "ASAP", "Before this Date"
        public DateTime? PriorityDate { get; set; } //nullable, only used if "Before this Date" is selected

        //Text Fields
        public string ActionsTakenBefore { get; set; }
        public string ReferralReasons { get; set; }
        public string CounselorInitialAction { get; set; }

        //Footer Info
        public string PersonWhoReferred { get; set; }
        public DateTime DateReferred { get; set; }

        //Counselor Feedback (populated from admin/web)
        public string? CounselorFeedbackStudentName { get; set; }
        public DateTime? CounselorFeedbackDateReferred { get; set; }
        public DateTime? CounselorSessionDate { get; set; }
        public string? CounselorActionsTaken { get; set; }
        public string? CounselorName { get; set; }

        public DateTime SubmissionDate { get; set; } = DateTime.Now;
    }


    public class EndorsementCustodyForm
    {
        [Key]
        public int CustodyId { get; set; }
        public int StudentId { get; set; }
        public int CounselorId { get; set; }
        public DateTime Date { get; set; }

        [Column(TypeName = "time")]
        public TimeSpan? Time { get; set; }

        public string GradeYearLevel { get; set; }
        public string Section { get; set; }
        public string Concerns { get; set; }
        public string Interventions { get; set; }
        public string Recommendations { get; set; }
        public string Referrals { get; set; }
        public string EndorsedBy { get; set; }
        public string EndorsedTo { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        [ForeignKey("StudentId")]
        public Student Student { get; set; }
        [ForeignKey("CounselorId")]
        public Counselor Counselor { get; set; }
    }

    public class ConsultationForm
    {
        [Key]
        public int ConsultationId { get; set; }
        public int CounselorId { get; set; }
        public int StudentId { get; set; }
        public DateTime Date { get; set; }

        [Column(TypeName = "time")]
        public TimeSpan? Time { get; set; }
        public string GradeYearLevel { get; set; }
        public string Section { get; set; }
        public string Concerns { get; set; }
        public string Remarks {  get; set; }
        public string CounselorName { get; set; }
        public string ParentGuardian { get; set; }
        public string SchoolPersonnel { get; set; }
        public string ParentContactNumber { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        [ForeignKey("StudentId")]
        public Student Student { get; set; }

        [ForeignKey("CounselorId")]
        public Counselor Counselor { get; set; }
    }

    public class Counselor
    {
        [Key]
        public int CounselorId { get; set; }

        [Required, MaxLength(255)]
        public string Email { get; set; }
        public string Name { get; set; }

        [Required, MaxLength(255)]
        public string Password { get; set; }
    }

    public class GuidanceNote
    {
        [Key]
        public int NoteId { get; set; }
        // Foreign Keys
        public int StudentId { get; set; }
        public int CounselorId { get; set; }
        // Interview Info
        public DateTime InterviewDate { get; set; }
        public TimeSpan? TimeStarted { get; set; }
        public TimeSpan? TimeEnded { get; set; }
        // School Year Info
        public string SchoolYear { get; set; }
        public string TertiarySemester { get; set; }   // "1st", "2nd", "Summer"
        public string SeniorHighQuarter { get; set; }  // "1st", "2nd", "3rd", "4th", "Summer"
                                                       // Student Info
        public string GradeYearLevelSection { get; set; }
        public string Program { get; set; }
        // Nature of Counseling (Multiple Choice)
        public bool IsAcademic { get; set; }
        public bool IsBehavioral { get; set; }
        public bool IsPersonal { get; set; }
        public bool IsSocial { get; set; }
        public bool IsCareer { get; set; }
        // Counseling Situation/s (Multiple Choice)
        public bool IsIndividual { get; set; }
        public bool IsGroup { get; set; }
        public bool IsClass { get; set; }
        public bool IsCounselorInitiated { get; set; }
        public bool IsWalkIn { get; set; }
        public bool IsFollowUp { get; set; }
        public string ReferredBy { get; set; }
        // Counseling Notes Sections
        public string PresentingProblem { get; set; }
        public string Assessment { get; set; }
        public string Interventions { get; set; }
        public string PlanOfAction { get; set; }
        // Recommendations
        public bool IsFollowThroughSession { get; set; }
        public DateTime? FollowThroughDate { get; set; }
        public bool IsReferral { get; set; }
        public string ReferralAgencyName { get; set; }
        // Counselor Info (added field)
        public string CounselorName { get; set; }
        // Metadata
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        // Relationships
        [ForeignKey("StudentId")]
        public Student Student { get; set; }
        [ForeignKey("CounselorId")]
        public Counselor Counselor { get; set; }
    }

    public class JournalEntry
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // ✅ Important to auto-generate ID
        public int JournalId { get; set; }

        public int StudentId { get; set; }

        [Required]
        public string Date { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public string Content { get; set; }

        public string Mood { get; set; }
    }

    public class AvailableTimeSlot
    {
        [Key]
        public int SlotId { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Required]
        [StringLength(20)]
        public string Time { get; set; }

        [Required]
        public int MaxAppointments { get; set; } = 3; // Default max appointments per slot

        public int CurrentAppointmentCount { get; set; } = 0; // Track current appointments

        public bool IsActive { get; set; } = true; // Allow disabling slots

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }
    }

    public class ProgramEntity
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(20)]
        public string Code { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;
    }

    public class SectionEntity
    {
        [Key]
        public int Id { get; set; }
        [Required, MaxLength(20)]
        public string ProgramCode { get; set; } = string.Empty; // FK by code
        [Required, MaxLength(50)]
        public string Name { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
    }

    public class AppointmentReason
    {
        [Key]
        public int Id { get; set; }
        [Required, MaxLength(20)]
        public string Code { get; set; } = string.Empty;
        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
    }

    public class ReferralCategory
    {
        [Key]
        public int Id { get; set; }
        [Required, MaxLength(30)]
        public string Code { get; set; } = string.Empty;
        [Required, MaxLength(100)]
        public string Label { get; set; } = string.Empty;
        [Required, MaxLength(30)]
        public string DefaultPriority { get; set; } = "ASAP"; // EMERGENCY|ASAP|BEFORE_DATE
        public bool IsActive { get; set; } = true;
    }

    // Single-row settings
    public class TimeSlotDefaults
    {
        [Key]
        public int Id { get; set; } = 1;
        public int MaxAppointments { get; set; } = 3;
        [MaxLength(500)]
        public string DefaultTimesCsv { get; set; } = "9:00 AM, 10:00 AM, 1:00 PM";
        public bool IsActive { get; set; } = true;
    }

    public class MoodThresholds
    {
        [Key]
        public int Id { get; set; } = 1;
        public int MildMax { get; set; } = 3;
        public int ModerateMax { get; set; } = 6;
        public int HighMin { get; set; } = 7;
        public bool IsActive { get; set; } = true;
    }

    public class MobileConfig
    {
        [Key] 
        public int Id { get; set; } = 1;
        public int Version { get; set; } = 1;
        public int MoodCooldownHours { get; set; } = 24;
        public string StudentNumberRegex { get; set; } = @"^\d{11}$";
        public string PhoneRegex { get; set; } = @"^\d{11}$";
        public string PasswordRegex { get; set; } = @"^(?=.*[A-Z])(?=.*[^a-zA-Z0-9]).{6,}$";
        public int MaxSiblings { get; set; } = 5;
        public int MaxWorkExperience { get; set; } = 5;
        public int NotificationCooldownMs { get; set; } = 10000;
    }

    public class Quote
    {
        [Key] public int Id { get; set; }
        [Required, MaxLength(300)] public string Text { get; set; } = string.Empty;
        [MaxLength(120)] public string Author { get; set; } = "Unknown";
    }

    public class DictionaryItem
    {
        [Key] public int Id { get; set; }
        [Required, MaxLength(50)] public string Group { get; set; } = string.Empty; // e.g., 'gradeYears'
        [Required, MaxLength(120)] public string Value { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
    }

    public class ActivityLog
    {
        [Key] public long ActivityId { get; set; }
        public string EntityType { get; set; } = string.Empty;
        public long? EntityId { get; set; }
        public string Action { get; set; } = string.Empty;
        public string ActorType { get; set; } = "system";
        public long? ActorId { get; set; }
        public string? DetailsJson { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
