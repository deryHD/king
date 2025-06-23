using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SurveyApp.Database.Entities
{
    [Table("NpsResponses")]
    public class NpsResponse
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public Guid SurveyToken { get; set; }

        public int UserId { get; set; }

        public int TicketId { get; set; }

        [Range(0, 10)]
        public int NpsScore { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? CompletedAt { get; set; }

        public bool IsCompleted { get; set; } = false;

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;

        [ForeignKey("TicketId")]
        public virtual Ticket Ticket { get; set; } = null!;

        public virtual ICollection<FollowUpQuestion> FollowUpQuestions { get; set; } = new List<FollowUpQuestion>();
        public virtual ICollection<QuestionResponse> QuestionResponses { get; set; } = new List<QuestionResponse>();
    }
}