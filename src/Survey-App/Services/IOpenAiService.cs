using SurveyApp.Models;

namespace SurveyApp.Services
{
    public interface IOpenAiService
    {
        Task<List<string>> GenerateFollowUpQuestionsAsync(string ticketDescription, int npsScore);
        Task<SentimentResult> AnalyzeSentimentAsync(string responseText);
    }
}