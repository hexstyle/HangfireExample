using Hangfire;
using Hangfire.Console;
using Hangfire.Server;
using Hangfire.PostgreSql;
using HangfireExample;
using HangfireExample.Jobs;


if(Environment.GetEnvironmentVariable("POSTGRES_HOST") == null)
    DotNetEnv.Env.Load(".env.local");

var builder = WebApplication.CreateBuilder(args);

var connStringTemplate = builder.Configuration.GetConnectionString("HangfireConnection");

string connString = connStringTemplate
    .Replace("${POSTGRES_HOST}", Environment.GetEnvironmentVariable("POSTGRES_HOST") ?? "localhost")
    .Replace("${POSTGRES_DB}", Environment.GetEnvironmentVariable("POSTGRES_DB") ?? "hangfire_db")
    .Replace("${POSTGRES_USER}", Environment.GetEnvironmentVariable("POSTGRES_USER") ?? "postgres")
    .Replace("${POSTGRES_PASSWORD}", Environment.GetEnvironmentVariable("POSTGRES_PASSWORD") ?? "postgres");
var factory = new PostgresConnectionFactory(connString);

builder.Services.AddHangfire(config =>
{
    config.UsePostgreSqlStorage((options)=>
        options.UseConnectionFactory(factory)
        );
    config.UseConsole();
});
builder.Services.AddHangfireServer();
builder.Services.AddTransient<MyJob>();
builder.Services.AddTransient<MaintenanceJob>();
builder.Services.AddSingleton<IConnectionFactory>(factory);

var app = builder.Build();
app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = new[] { new AllowAllAuthorizationFilter() }
});

var scope = app.Services.CreateScope();
// Или повторяющаяся задача:
RecurringJob.AddOrUpdate<MyJob>(
    "my-job",
    job => job.Run((PerformContext)null),
    "*/5 * * * * *");
RecurringJob.AddOrUpdate<MaintenanceJob>(
    "maintenance-cleanup",
    job => job.CleanupAsync((PerformContext)null),
    Cron.Minutely);

app.MapGet("/", () => "Hangfire with PostgreSQL is running!");
app.Run();
