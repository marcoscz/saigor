using Saigor.Models;

namespace Saigor.Services
{
    public interface IJobSchedulerService
    {
        Task ScheduleJobAsync(JobModel job);
        Task UnscheduleJobAsync(int jobId);
        Task<bool> IsJobScheduledAsync(int jobId);
        Task<bool> StartJobAsync(string jobName);
        Task<bool> StopJobAsync(string jobName);
    }
} 