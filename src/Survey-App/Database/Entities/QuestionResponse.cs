using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SurveyApp.Database.Entities
{
    [Table("QuestionResponses")]
    public class QuestionResponse
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int NpsResponseId { get; set; }

        public int FollowUpQuestionId { get; set; }

        [Required]
        public string ResponseText { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("NpsResponseId")]
        public virtual NpsResponse NpsResponse { get; set; } = null!;

        [ForeignKey("FollowUpQuestionId")]
        public virtual FollowUpQuestion FollowUpQuestion { get; set; } = null!;

        public virtual SentimentAnalysis? SentimentAnalysis { get; set; }
    }
}