using Microsoft.EntityFrameworkCore;
using SurveyApp.Database;
using SurveyApp.Database.Entities;
using SurveyApp.Models;

namespace SurveyApp.Services
{
    public class SurveyService : ISurveyService
    {
        private readonly DatabaseContext _context;
        private readonly IOpenAiService _openAiService;
        private readonly ILogger<SurveyService> _logger;

        public SurveyService(DatabaseContext context, IOpenAiService openAiService, ILogger<SurveyService> logger)
        {
            _context = context;
            _openAiService = openAiService;
            _logger = logger;
        }

        public async Task<SurveyInvitation?> GetSurveyInvitationAsync(Guid token)
        {
            return await _context.SurveyInvitations
                .Include(s => s.User)
                .Include(s => s.Ticket)
                .FirstOrDefaultAsync(s => s.InvitationToken == token && !s.IsExpired && !s.IsCompleted);
        }

        public async Task<NpsResponse> CreateNpsResponseAsync(Guid surveyToken, int npsScore)
        {
            var invitation = await GetSurveyInvitationAsync(surveyToken);
            if (invitation == null)
            {
                throw new InvalidOperationException("Invalid or expired survey token");
            }

            var npsResponse = new NpsResponse
            {
                SurveyToken = Guid.NewGuid(),
                UserId = invitation.UserId,
                TicketId = invitation.TicketId,
                NpsScore = npsScore,
                CreatedAt = DateTime.UtcNow
            };

            _context.NpsResponses.Add(npsResponse);
            
            // Mark invitation as opened
            invitation.OpenedAt = DateTime.UtcNow;
            
            await _context.SaveChangesAsync();
            return npsResponse;
        }

        public async Task<List<FollowUpQuestion>> GenerateAndSaveFollowUpQuestionsAsync(int npsResponseId)
        {
            var npsResponse = await _context.NpsResponses
                .Include(n => n.Ticket)
                .FirstOrDefaultAsync(n => n.Id == npsResponseId);

            if (npsResponse == null)
            {
                throw new InvalidOperationException("NPS Response not found");
            }

            try
            {
                var questions = await _openAiService.GenerateFollowUpQuestionsAsync(
                    npsResponse.Ticket.Description, 
                    npsResponse.NpsScore);

                var followUpQuestions = new List<FollowUpQuestion>();
                for (int i = 0; i < questions.Count; i++)
                {
                    var question = new FollowUpQuestion
                    {
                        NpsResponseId = npsResponseId,
                        QuestionText = questions[i],
                        QuestionOrder = i + 1,
                        CreatedAt = DateTime.UtcNow
                    };
                    followUpQuestions.Add(question);
                    _context.FollowUpQuestions.Add(question);
                }

                await _context.SaveChangesAsync();
                return followUpQuestions;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating follow-up questions for NPS Response {NpsResponseId}", npsResponseId);
                throw;
            }
        }

        public async Task SaveQuestionResponseAsync(int npsResponseId, int questionId, string responseText)
        {
            var questionResponse = new QuestionResponse
            {
                NpsResponseId = npsResponseId,
                FollowUpQuestionId = questionId,
                ResponseText = responseText,
                CreatedAt = DateTime.UtcNow
            };

            _context.QuestionResponses.Add(questionResponse);
            await _context.SaveChangesAsync();

            // Trigger sentiment analysis in background
            _ = Task.Run(async () =>
            {
                try
                {
                    var sentiment = await _openAiService.AnalyzeSentimentAsync(responseText);
                    var sentimentAnalysis = new SentimentAnalysis
                    {
                        QuestionResponseId = questionResponse.Id,
                        SentimentLabel = sentiment.Label,
                        ConfidenceScore = sentiment.Confidence,
                        AnalyzedAt = DateTime.UtcNow
                    };

                    _context.SentimentAnalyses.Add(sentimentAnalysis);
                    await _context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error analyzing sentiment for response {ResponseId}", questionResponse.Id);
                }
            });
        }

        public async Task CompleteSurveyAsync(int npsResponseId)
        {
            var npsResponse = await _context.NpsResponses.FindAsync(npsResponseId);
            if (npsResponse != null)
            {
                npsResponse.IsCompleted = true;
                npsResponse.CompletedAt = DateTime.UtcNow;

                // Also mark the invitation as completed
                var invitation = await _context.SurveyInvitations
                    .FirstOrDefaultAsync(s => s.UserId == npsResponse.UserId && s.TicketId == npsResponse.TicketId);
                if (invitation != null)
                {
                    invitation.IsCompleted = true;
                    invitation.CompletedAt = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<User>> GetEligibleUsersForSurveyAsync()
        {
            var lastMonth = DateTime.UtcNow.AddMonths(-1);
            var startOfLastMonth = new DateTime(lastMonth.Year, lastMonth.Month, 1);
            var endOfLastMonth = startOfLastMonth.AddMonths(1).AddDays(-1);

            return await _context.Users
                .Where(u => u.IsActive)
                .Where(u => u.Tickets.Any(t => 
                    t.Status == "Completed" && 
                    t.CompletedAt >= startOfLastMonth && 
                    t.CompletedAt <= endOfLastMonth))
                .OrderBy(u => Guid.NewGuid()) // Random order
                .Take(10)
                .ToListAsync();
        }

        public async Task<List<SurveyInvitation>> CreateSurveyInvitationsAsync(List<User> users)
        {
            var invitations = new List<SurveyInvitation>();
            var lastMonth = DateTime.UtcNow.AddMonths(-1);
            var startOfLastMonth = new DateTime(lastMonth.Year, lastMonth.Month, 1);
            var endOfLastMonth = startOfLastMonth.AddMonths(1).AddDays(-1);

            foreach (var user in users)
            {
                // Get the most recent completed ticket for this user
                var ticket = await _context.Tickets
                    .Where(t => t.AssignedToUserId == user.Id && 
                               t.Status == "Completed" && 
                               t.CompletedAt >= startOfLastMonth && 
                               t.CompletedAt <= endOfLastMonth)
                    .OrderByDescending(t => t.CompletedAt)
                    .FirstOrDefaultAsync();

                if (ticket != null)
                {
                    var invitation = new SurveyInvitation
                    {
                        UserId = user.Id,
                        TicketId = ticket.Id,
                        InvitationToken = Guid.NewGuid(),
                        SentAt = DateTime.UtcNow,
                        ExpiresAt = DateTime.UtcNow.AddDays(14) // 2 weeks to respond
                    };

                    invitations.Add(invitation);
                    _context.SurveyInvitations.Add(invitation);
                }
            }

            await _context.SaveChangesAsync();
            return invitations;
        }
    }
}