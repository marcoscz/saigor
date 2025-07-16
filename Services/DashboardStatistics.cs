using Saigor.Models;

namespace Saigor.Services
{
    public class DashboardStatistics
    {
        public int TotalJobs { get; set; }
        public int TotalTarefas { get; set; }
        public int TotalLogs { get; set; }
        public int ActiveJobs { get; set; }
        public double SuccessRate { get; set; }
        public Dictionary<JobStatus, int> JobsByStatus { get; set; } = new();
        public Dictionary<string, int> LogsByStatus { get; set; } = new();
    }
} 