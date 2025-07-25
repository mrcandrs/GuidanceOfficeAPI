﻿using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

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


    public class InitialAssessmentForm
    {
        [Key]
        public int AssessmentId { get; set; }

        public int? StudentId { get; set; }
        public int Score { get; set; }
        public string MoodLevel { get; set; }
        public DateTime AssessmentDate { get; set; }

        [ForeignKey("StudentId")]
        public Student Student { get; set; }
    }

    public class MoodTracker
    {
        [Key]
        public int MoodId { get; set; }

        public int StudentId { get; set; }
        public string MoodLevel { get; set; }
        public DateTime EntryDate { get; set; }

        [ForeignKey("StudentId")]
        public Student Student { get; set; }
    }

    public class AppointmentRequest
    {
        [Key]
        public int AppointmentId { get; set; }

        public int StudentId { get; set; }
        public DateTime PreferredDate { get; set; }
        public TimeSpan PreferredTime { get; set; }
        public bool ReasonAcademics { get; set; }
        public bool ReasonSocial { get; set; }
        public bool ReasonPersonal { get; set; }
        public bool ReasonCareer { get; set; }
        public bool ReasonOthers { get; set; }
        public string OtherReasonText { get; set; }
        public string Status { get; set; }
        public DateTime SubmittedAt { get; set; }

        [ForeignKey("StudentId")]
        public Student Student { get; set; }
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
        public AppointmentRequest Appointment { get; set; }

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
        public string EndorsementDetails { get; set; }
        public DateTime Date { get; set; }

        [ForeignKey("StudentId")]
        public Student Student { get; set; }

        [ForeignKey("CounselorId")]
        public Counselor Counselor { get; set; }
    }

    public class ConsultationForm
    {
        [Key]
        public int ConsultationId { get; set; }
        public int StudentId { get; set; }
        public int CounselorId { get; set; }
        public DateTime Date { get; set; }
        public string Topic { get; set; }
        public string ActionTaken { get; set; }

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

        public int StudentId { get; set; }
        public int CounselorId { get; set; }
        public string NoteText { get; set; }
        public DateTime CreatedAt { get; set; }

        [ForeignKey("StudentId")]
        public Student Student { get; set; }

        [ForeignKey("CounselorId")]
        public Counselor Counselor { get; set; }
    }

    public class MoodSupport
    {
        [Key]
        public int SupportId { get; set; }

        public string MoodLevel { get; set; }
        public string SuggestionText { get; set; }
    }

    public class MotivationalQuote
    {
        [Key]
        public int QuoteId { get; set; }
        public string QuoteText { get; set; }
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
}
