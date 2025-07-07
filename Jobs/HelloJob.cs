using Quartz;
namespace Saigor.Jobs
{
    public class HelloJob : IJob
    {
        public Task Execute(IJobExecutionContext context)
        {
            Console.WriteLine($"[HelloJob] Executado em: {DateTime.Now}");
            return Task.CompletedTask;
        }
    }
}




