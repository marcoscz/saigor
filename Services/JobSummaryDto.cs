using Saigor.Models;

namespace Saigor.Services
{
    public class JobSummaryDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Command { get; set; } = string.Empty;
        public string Schedule { get; set; } = string.Empty;
        public JobStatus Status { get; set; }
    }
} 