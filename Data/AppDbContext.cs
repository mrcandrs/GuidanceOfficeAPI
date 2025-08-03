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
        public DbSet<InitialAssessmentForm> InitialAssessmentForms { get; set; }
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
        }


        public DbSet<ReferralForm> ReferralForms { get; set; }
        public DbSet<EndorsementCustodyForm> EndorsementCustodyForms { get; set; }
        public DbSet<ConsultationForm> ConsultationForms { get; set; }
        public DbSet<Counselor> Counselors { get; set; }
        public DbSet<GuidanceNote> GuidanceNotes { get; set; }
        public DbSet<MoodSupport> MoodSupports { get; set; }
        public DbSet<MotivationalQuote> MotivationalQuotes { get; set; }
        public DbSet<JournalEntry> JournalEntries { get; set; }
    }

}
