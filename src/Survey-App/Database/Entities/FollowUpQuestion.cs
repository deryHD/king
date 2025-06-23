using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SurveyApp.Database.Entities
{
    [Table("FollowUpQuestions")]
    public class FollowUpQuestion
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int NpsResponseId { get; set; }

        [Required]
        public string QuestionText { get; set; } = string.Empty;

        public int QuestionOrder { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("NpsResponseId")]
        public virtual NpsResponse NpsResponse { get; set; } = null!;

        public virtual ICollection<QuestionResponse> QuestionResponses { get; set; } = new List<QuestionResponse>();
    }
}