namespace SurveyApp.Models
{
    public class SentimentResult
    {
        public string Label { get; set; } = string.Empty;
        public double Confidence { get; set; }
    }
}