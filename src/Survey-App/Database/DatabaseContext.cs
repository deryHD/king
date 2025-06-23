using Microsoft.EntityFrameworkCore;
using SurveyApp.Database.Entities;

namespace SurveyApp.Database
{
    public class DatabaseContext : DbContext
    {
        public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options) { }

        // DbSets
        public DbSet<User> Users { get; set; }
        public DbSet<Ticket> Tickets { get; set; }
        public DbSet<NpsResponse> NpsResponses { get; set; }
        public DbSet<FollowUpQuestion> FollowUpQuestions { get; set; }
        public DbSet<QuestionResponse> QuestionResponses { get; set; }
        public DbSet<SentimentAnalysis> SentimentAnalyses { get; set; }
        public DbSet<SurveyInvitation> SurveyInvitations { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User configuration
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasIndex(e => e.Email).IsUnique();
                entity.Property(e => e.Email).IsRequired();
            });

            // Ticket configuration
            modelBuilder.Entity<Ticket>(entity =>
            {
                entity.HasIndex(e => e.JiraId).IsUnique();
                entity.HasOne(t => t.AssignedToUser)
                      .WithMany(u => u.Tickets)
                      .HasForeignKey(t => t.AssignedToUserId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // NpsResponse configuration
            modelBuilder.Entity<NpsResponse>(entity =>
            {
                entity.HasIndex(e => e.SurveyToken).IsUnique();
                entity.HasOne(n => n.User)
                      .WithMany(u => u.NpsResponses)
                      .HasForeignKey(n => n.UserId)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(n => n.Ticket)
                      .WithMany(t => t.NpsResponses)
                      .HasForeignKey(n => n.TicketId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // FollowUpQuestion configuration
            modelBuilder.Entity<FollowUpQuestion>(entity =>
            {
                entity.HasOne(f => f.NpsResponse)
                      .WithMany(n => n.FollowUpQuestions)
                      .HasForeignKey(f => f.NpsResponseId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // QuestionResponse configuration
            modelBuilder.Entity<QuestionResponse>(entity =>
            {
                entity.HasOne(q => q.NpsResponse)
                      .WithMany(n => n.QuestionResponses)
                      .HasForeignKey(q => q.NpsResponseId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(q => q.FollowUpQuestion)
                      .WithMany(f => f.QuestionResponses)
                      .HasForeignKey(q => q.FollowUpQuestionId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // SentimentAnalysis configuration
            modelBuilder.Entity<SentimentAnalysis>(entity =>
            {
                entity.HasOne(s => s.QuestionResponse)
                      .WithOne(q => q.SentimentAnalysis)
                      .HasForeignKey<SentimentAnalysis>(s => s.QuestionResponseId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // SurveyInvitation configuration
            modelBuilder.Entity<SurveyInvitation>(entity =>
            {
                entity.HasIndex(e => e.InvitationToken).IsUnique();
                entity.HasOne(s => s.User)
                      .WithMany()
                      .HasForeignKey(s => s.UserId)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(s => s.Ticket)
                      .WithMany()
                      .HasForeignKey(s => s.TicketId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}