using SurveyApp.Database.Entities;
using SurveyApp.Models;

namespace SurveyApp.Services
{
    public interface ISurveyService
    {
        Task<SurveyInvitation?> GetSurveyInvitationAsync(Guid token);
        Task<NpsResponse> CreateNpsResponseAsync(Guid surveyToken, int npsScore);
        Task<List<FollowUpQuestion>> GenerateAndSaveFollowUpQuestionsAsync(int npsResponseId);
        Task SaveQuestionResponseAsync(int npsResponseId, int questionId, string responseText);
        Task CompleteSurveyAsync(int npsResponseId);
        Task<List<User>> GetEligibleUsersForSurveyAsync();
        Task<List<SurveyInvitation>> CreateSurveyInvitationsAsync(List<User> users);
    }
}