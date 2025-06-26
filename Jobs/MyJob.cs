using Hangfire.Server;
using Hangfire.Console;
namespace HangfireExample.Jobs;

public class MyJob
{
    private readonly ILogger<MyJob> _logger;

    public MyJob(ILogger<MyJob> logger)
    {
        _logger = logger;
    }

    public void Run(PerformContext context)
    {
        context.WriteLine( $"Job started at {DateTime.Now}");
        Thread.Sleep(2000);
        context.WriteLine($"Job finished at {DateTime.Now}");
    }
}
