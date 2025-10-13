namespace GuidanceOfficeAPI.Data
{
    using Microsoft.EntityFrameworkCore;
    using GuidanceOfficeAPI.Models;
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Student> Students { get; set; }
        public DbSet<ConsentForm> ConsentForms { get; set; }
        public DbSet<InventoryForm> InventoryForms { get; set; }
        public DbSet<Sibling> Siblings { get; set; }
        public DbSet<WorkExperience> WorkExperiences { get; set; }
        public DbSet<CareerPlanningForm> CareerPlanningForms { get; set; }
        public DbSet<MoodTracker> MoodTrackers { get; set; }
        public DbSet<GuidanceAppointment> GuidanceAppointments { get; set; }
        public DbSet<GuidancePass> GuidancePasses { get; set; }

        public DbSet<ExitInterviewForm> ExitInterviewForms { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ExitInterviewForm>()
                .Property(e => e.SpecificReasons)
                .HasConversion(
                    v => v,
                    v => v);

            modelBuilder.Entity<ExitInterviewForm>()
                .Property(e => e.ServiceResponsesJson)
                .HasConversion(
                    v => v,
                    v => v);

            // ✅ Add this unique index:
            modelBuilder.Entity<CareerPlanningForm>()
                .HasIndex(f => f.StudentId)
                .IsUnique();

            // Configure GuidanceAppointment
            modelBuilder.Entity<GuidanceAppointment>(entity =>
            {
                entity.HasKey(e => e.AppointmentId);
                entity.Property(e => e.StudentName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.ProgramSection).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Reason).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Date).IsRequired().HasMaxLength(10);
                entity.Property(e => e.Time).IsRequired().HasMaxLength(10);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(20).HasDefaultValue("pending");
                entity.Property(e => e.RejectionReason).HasMaxLength(500);
                entity.Property(e => e.CreatedAt).IsRequired().HasDefaultValueSql("GETDATE()");
            });

            // inside OnModelCreating(ModelBuilder modelBuilder), after base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<ProgramEntity>()
                .HasIndex(x => x.Code)
                .IsUnique();

            modelBuilder.Entity<SectionEntity>()
                .HasIndex(x => new { x.ProgramCode, x.Name })
                .IsUnique();

            modelBuilder.Entity<AppointmentReason>()
                .HasIndex(x => x.Code)
                .IsUnique();

            modelBuilder.Entity<ReferralCategory>()
                .HasIndex(x => x.Code)
                .IsUnique();

            modelBuilder.Entity<JournalEntry>()
                .HasIndex(j => new { j.StudentId, j.Date })
                .IsUnique();

            // seed singletons so GET returns a row
            modelBuilder.Entity<TimeSlotDefaults>().HasData(new TimeSlotDefaults
            {
                Id = 1,
                MaxAppointments = 3,
                DefaultTimesCsv = "9:00 AM, 10:00 AM, 1:00 PM",
                IsActive = true
            });
            modelBuilder.Entity<MoodThresholds>().HasData(new MoodThresholds
            {
                Id = 1,
                MildMax = 3,
                ModerateMax = 6,
                HighMin = 7,
                IsActive = true
            });

            modelBuilder.Entity<DictionaryItem>()
            .HasIndex(x => new { x.Group, x.Value }).IsUnique();

            modelBuilder.Entity<MobileConfig>().HasData(new MobileConfig { Id = 1 });

            modelBuilder.Entity<DictionaryItem>().HasData(
                // gradeYears
                new DictionaryItem { Id = 1, Group = "gradeYears", Value = "Grade 11", IsActive = true },
                new DictionaryItem { Id = 2, Group = "gradeYears", Value = "Grade 12", IsActive = true },
                new DictionaryItem { Id = 3, Group = "gradeYears", Value = "1st Year", IsActive = true },
                // genders
                new DictionaryItem { Id = 10, Group = "genders", Value = "Male", IsActive = true },
                new DictionaryItem { Id = 11, Group = "genders", Value = "Female", IsActive = true },
                // academicLevels
                new DictionaryItem { Id = 20, Group = "academicLevels", Value = "Junior High", IsActive = true },
                new DictionaryItem { Id = 21, Group = "academicLevels", Value = "Senior High", IsActive = true },
                // referredBy
                new DictionaryItem { Id = 30, Group = "referredBy", Value = "Student", IsActive = true },
                new DictionaryItem { Id = 31, Group = "referredBy", Value = "Parent", IsActive = true },
                // areasOfConcern
                new DictionaryItem { Id = 40, Group = "areasOfConcern", Value = "Academic", IsActive = true },
                new DictionaryItem { Id = 41, Group = "areasOfConcern", Value = "Behavioral", IsActive = true },
                // actionRequested
                new DictionaryItem { Id = 50, Group = "actionRequested", Value = "Counseling", IsActive = true },
                new DictionaryItem { Id = 51, Group = "actionRequested", Value = "Classroom Observation", IsActive = true },
                // referralPriorities
                new DictionaryItem { Id = 60, Group = "referralPriorities", Value = "Emergency", IsActive = true },
                new DictionaryItem { Id = 61, Group = "referralPriorities", Value = "ASAP", IsActive = true },
                new DictionaryItem { Id = 62, Group = "referralPriorities", Value = "Before Date", IsActive = true },
                // moodLevels
                new DictionaryItem { Id = 70, Group = "moodLevels", Value = "MILD", IsActive = true },
                new DictionaryItem { Id = 71, Group = "moodLevels", Value = "MODERATE", IsActive = true },
                new DictionaryItem { Id = 72, Group = "moodLevels", Value = "HIGH", IsActive = true }
            );

            modelBuilder.Entity<ActivityLog>()
            .HasIndex(x => new { x.EntityType, x.EntityId, x.CreatedAt });
            modelBuilder.Entity<ActivityLog>().HasIndex(x => x.CreatedAt);
            modelBuilder.Entity<ActivityLog>().HasIndex(x => new { x.ActorType, x.ActorId, x.CreatedAt });
            modelBuilder.Entity<ActivityLog>().HasIndex(x => new { x.Action, x.CreatedAt });
        }

        public DbSet<ReferralForm> ReferralForms { get; set; }
        public DbSet<EndorsementCustodyForm> EndorsementCustodyForms { get; set; }
        public DbSet<ConsultationForm> ConsultationForms { get; set; }
        public DbSet<Counselor> Counselors { get; set; }
        public DbSet<GuidanceNote> GuidanceNotes { get; set; }
        public DbSet<JournalEntry> JournalEntries { get; set; }

        public DbSet<AvailableTimeSlot> AvailableTimeSlots { get; set; }

        // Data/AppDbContext.cs (add DbSets + constraints)
        // inside AppDbContext class (DbSets)
        public DbSet<ProgramEntity> Programs { get; set; }
        public DbSet<SectionEntity> Sections { get; set; }
        public DbSet<AppointmentReason> AppointmentReasons { get; set; }
        public DbSet<ReferralCategory> ReferralCategories { get; set; }
        public DbSet<TimeSlotDefaults> TimeSlotDefaults { get; set; }
        public DbSet<MoodThresholds> MoodThresholds { get; set; }
        public DbSet<MobileConfig> MobileConfigs { get; set; }
        public DbSet<Quote> Quotes { get; set; }
        public DbSet<DictionaryItem> DictionaryItems { get; set; }
        public DbSet<ActivityLog> ActivityLogs { get; set; }
        public DbSet<CounselorSession> CounselorSessions { get; set; }
    }

}
