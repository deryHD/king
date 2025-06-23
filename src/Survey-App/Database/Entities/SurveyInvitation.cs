using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SurveyApp.Database.Entities
{
    [Table("SurveyInvitations")]
    public class SurveyInvitation
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int UserId { get; set; }

        public int TicketId { get; set; }

        [Required]
        public Guid InvitationToken { get; set; }

        public DateTime SentAt { get; set; } = DateTime.UtcNow;

        public DateTime? OpenedAt { get; set; }

        public DateTime? CompletedAt { get; set; }

        public DateTime ExpiresAt { get; set; }

        public bool IsExpired => DateTime.UtcNow > ExpiresAt;

        public bool IsCompleted { get; set; } = false;

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;

        [ForeignKey("TicketId")]
        public virtual Ticket Ticket { get; set; } = null!;
    }
}