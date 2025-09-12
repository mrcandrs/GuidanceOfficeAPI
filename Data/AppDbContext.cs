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
        }


        public DbSet<ReferralForm> ReferralForms { get; set; }
        public DbSet<EndorsementCustodyForm> EndorsementCustodyForms { get; set; }
        public DbSet<ConsultationForm> ConsultationForms { get; set; }
        public DbSet<Counselor> Counselors { get; set; }
        public DbSet<GuidanceNote> GuidanceNotes { get; set; }
        public DbSet<JournalEntry> JournalEntries { get; set; }

        public DbSet<AvailableTimeSlot> AvailableTimeSlots { get; set; }
    }

}
