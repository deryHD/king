using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SurveyApp.Database.Entities
{
    [Table("SentimentAnalyses")]
    public class SentimentAnalysis
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int QuestionResponseId { get; set; }

        [Required, StringLength(50)]
        public string SentimentLabel { get; set; } = string.Empty;

        [Range(0.0, 1.0)]
        public double ConfidenceScore { get; set; }

        public DateTime AnalyzedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("QuestionResponseId")]
        public virtual QuestionResponse QuestionResponse { get; set; } = null!;
    }
}