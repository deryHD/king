using SurveyApp.Database.Entities;

namespace Reporting_App.Services
{
    public interface IReportingService
    {
        Task<double> GetAverageNpsScoreAsync(DateTime? startDate = null, DateTime? endDate = null, string? team = null);
        Task<List<MonthlyNpsData>> GetMonthlyNpsTrendAsync(int months = 12);
        Task<List<TeamNpsData>> GetNpsScoresByTeamAsync(DateTime? startDate = null, DateTime? endDate = null);
        Task<List<SentimentDistribution>> GetSentimentDistributionAsync(DateTime? startDate = null, DateTime? endDate = null);
        Task<List<string>> GetAvailableTeamsAsync();
        Task<NpsDashboardData> GetDashboardDataAsync(DateTime? startDate = null, DateTime? endDate = null, string? team = null);
        Task<byte[]> ExportNpsDataToCsvAsync(DateTime? startDate = null, DateTime? endDate = null, string? team = null);
    }

    public class MonthlyNpsData
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public string MonthName { get; set; } = string.Empty;
        public double AverageScore { get; set; }
        public int ResponseCount { get; set; }
    }

    public class TeamNpsData
    {
        public string TeamName { get; set; } = string.Empty;
        public double AverageScore { get; set; }
        public int ResponseCount { get; set; }
        public int PromoterCount { get; set; }
        public int PassiveCount { get; set; }
        public int DetractorCount { get; set; }
    }

    public class SentimentDistribution
    {
        public string SentimentLabel { get; set; } = string.Empty;
        public int Count { get; set; }
        public double Percentage { get; set; }
    }

    public class NpsDashboardData
    {
        public double AverageNpsScore { get; set; }
        public int TotalResponses { get; set; }
        public int PromoterCount { get; set; }
        public int PassiveCount { get; set; }
        public int DetractorCount { get; set; }
        public double ResponseRate { get; set; }
        public List<MonthlyNpsData> MonthlyTrend { get; set; } = new();
        public List<TeamNpsData> TeamData { get; set; } = new();
        public List<SentimentDistribution> SentimentData { get; set; } = new();
    }
}