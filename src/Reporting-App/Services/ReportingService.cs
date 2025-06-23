using Microsoft.EntityFrameworkCore;
using SurveyApp.Database;
using SurveyApp.Database.Entities;
using System.Globalization;
using System.Text;

namespace Reporting_App.Services
{
    public class ReportingService : IReportingService
    {
        private readonly DatabaseContext _context;
        private readonly ILogger<ReportingService> _logger;

        public ReportingService(DatabaseContext context, ILogger<ReportingService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<double> GetAverageNpsScoreAsync(DateTime? startDate = null, DateTime? endDate = null, string? team = null)
        {
            var query = _context.NpsResponses
                .Include(n => n.User)
                .Where(n => n.IsCompleted);

            if (startDate.HasValue)
                query = query.Where(n => n.CreatedAt >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(n => n.CreatedAt <= endDate.Value);

            if (!string.IsNullOrEmpty(team))
                query = query.Where(n => n.User.Team == team);

            var scores = await query.Select(n => n.NpsScore).ToListAsync();
            return scores.Any() ? scores.Average() : 0;
        }

        public async Task<List<MonthlyNpsData>> GetMonthlyNpsTrendAsync(int months = 12)
        {
            var startDate = DateTime.UtcNow.AddMonths(-months);
            
            var monthlyData = await _context.NpsResponses
                .Where(n => n.IsCompleted && n.CreatedAt >= startDate)
                .GroupBy(n => new { n.CreatedAt.Year, n.CreatedAt.Month })
                .Select(g => new MonthlyNpsData
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    AverageScore = g.Average(n => n.NpsScore),
                    ResponseCount = g.Count()
                })
                .OrderBy(m => m.Year)
                .ThenBy(m => m.Month)
                .ToListAsync();

            // Add month names
            foreach (var data in monthlyData)
            {
                data.MonthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(data.Month);
            }

            return monthlyData;
        }

        public async Task<List<TeamNpsData>> GetNpsScoresByTeamAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _context.NpsResponses
                .Include(n => n.User)
                .Where(n => n.IsCompleted && !string.IsNullOrEmpty(n.User.Team));

            if (startDate.HasValue)
                query = query.Where(n => n.CreatedAt >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(n => n.CreatedAt <= endDate.Value);

            var teamData = await query
                .GroupBy(n => n.User.Team)
                .Select(g => new TeamNpsData
                {
                    TeamName = g.Key!,
                    AverageScore = g.Average(n => n.NpsScore),
                    ResponseCount = g.Count(),
                    PromoterCount = g.Count(n => n.NpsScore >= 9),
                    PassiveCount = g.Count(n => n.NpsScore >= 7 && n.NpsScore <= 8),
                    DetractorCount = g.Count(n => n.NpsScore <= 6)
                })
                .OrderByDescending(t => t.AverageScore)
                .ToListAsync();

            return teamData;
        }

        public async Task<List<SentimentDistribution>> GetSentimentDistributionAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _context.SentimentAnalyses
                .Include(s => s.QuestionResponse)
                .ThenInclude(q => q.NpsResponse)
                .Where(s => s.QuestionResponse.NpsResponse.IsCompleted);

            if (startDate.HasValue)
                query = query.Where(s => s.AnalyzedAt >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(s => s.AnalyzedAt <= endDate.Value);

            var sentimentCounts = await query
                .GroupBy(s => s.SentimentLabel)
                .Select(g => new { Label = g.Key, Count = g.Count() })
                .ToListAsync();

            var totalCount = sentimentCounts.Sum(s => s.Count);
            
            return sentimentCounts.Select(s => new SentimentDistribution
            {
                SentimentLabel = s.Label,
                Count = s.Count,
                Percentage = totalCount > 0 ? (double)s.Count / totalCount * 100 : 0
            }).OrderByDescending(s => s.Count).ToList();
        }

        public async Task<List<string>> GetAvailableTeamsAsync()
        {
            return await _context.Users
                .Where(u => !string.IsNullOrEmpty(u.Team))
                .Select(u => u.Team!)
                .Distinct()
                .OrderBy(t => t)
                .ToListAsync();
        }

        public async Task<NpsDashboardData> GetDashboardDataAsync(DateTime? startDate = null, DateTime? endDate = null, string? team = null)
        {
            var query = _context.NpsResponses
                .Include(n => n.User)
                .Where(n => n.IsCompleted);

            if (startDate.HasValue)
                query = query.Where(n => n.CreatedAt >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(n => n.CreatedAt <= endDate.Value);

            if (!string.IsNullOrEmpty(team))
                query = query.Where(n => n.User.Team == team);

            var responses = await query.ToListAsync();
            var totalInvitations = await _context.SurveyInvitations
                .CountAsync(s => (!startDate.HasValue || s.SentAt >= startDate.Value) &&
                                (!endDate.HasValue || s.SentAt <= endDate.Value));

            var promoters = responses.Count(r => r.NpsScore >= 9);
            var passives = responses.Count(r => r.NpsScore >= 7 && r.NpsScore <= 8);
            var detractors = responses.Count(r => r.NpsScore <= 6);

            return new NpsDashboardData
            {
                AverageNpsScore = responses.Any() ? responses.Average(r => r.NpsScore) : 0,
                TotalResponses = responses.Count,
                PromoterCount = promoters,
                PassiveCount = passives,
                DetractorCount = detractors,
                ResponseRate = totalInvitations > 0 ? (double)responses.Count / totalInvitations * 100 : 0,
                MonthlyTrend = await GetMonthlyNpsTrendAsync(12),
                TeamData = await GetNpsScoresByTeamAsync(startDate, endDate),
                SentimentData = await GetSentimentDistributionAsync(startDate, endDate)
            };
        }

        public async Task<byte[]> ExportNpsDataToCsvAsync(DateTime? startDate = null, DateTime? endDate = null, string? team = null)
        {
            var query = _context.NpsResponses
                .Include(n => n.User)
                .Include(n => n.Ticket)
                .Include(n => n.QuestionResponses)
                .ThenInclude(q => q.SentimentAnalysis)
                .Where(n => n.IsCompleted);

            if (startDate.HasValue)
                query = query.Where(n => n.CreatedAt >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(n => n.CreatedAt <= endDate.Value);

            if (!string.IsNullOrEmpty(team))
                query = query.Where(n => n.User.Team == team);

            var data = await query.ToListAsync();

            var csv = new StringBuilder();
            csv.AppendLine("Date,User,Team,Ticket,NPS Score,Category,Sentiment,Confidence");

            foreach (var response in data)
            {
                var category = response.NpsScore >= 9 ? "Promoter" :
                              response.NpsScore >= 7 ? "Passive" : "Detractor";

                var sentiment = response.QuestionResponses
                    .SelectMany(q => q.SentimentAnalysis != null ? new[] { q.SentimentAnalysis } : new SentimentAnalysis[0])
                    .FirstOrDefault();

                csv.AppendLine($"{response.CreatedAt:yyyy-MM-dd}," +
                              $"{response.User.FirstName} {response.User.LastName}," +
                              $"{response.User.Team ?? ""}," +
                              $"{response.Ticket.JiraId}," +
                              $"{response.NpsScore}," +
                              $"{category}," +
                              $"{sentiment?.SentimentLabel ?? ""}," +
                              $"{sentiment?.ConfidenceScore ?? 0:F2}");
            }

            return Encoding.UTF8.GetBytes(csv.ToString());
        }
    }
}