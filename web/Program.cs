using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Core;

namespace web
{
    public class Program
    {
        public static IConfiguration CurrentConfiguration { get; } = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
            .Build();

        public static int Main(string[] args)
        {
            var loggerConfig = new LoggerConfiguration()
                .MinimumLevel.Information()
                .MinimumLevel.Override("Microsoft", new LoggingLevelSwitch(Serilog.Events.LogEventLevel.Warning))
                .MinimumLevel.Override("System", new LoggingLevelSwitch(Serilog.Events.LogEventLevel.Warning))
                .WriteTo.LiterateConsole();

            var fileLogPath = Environment.GetEnvironmentVariable("APP_LOG_FILE_PATH");

            int fileLogFileCount = 0;
            int.TryParse(Environment.GetEnvironmentVariable("APP_LOG_FILE_COUNT"), out fileLogFileCount);

            if(!string.IsNullOrEmpty(fileLogPath))
                loggerConfig = loggerConfig.WriteTo.File(fileLogPath, rollingInterval: RollingInterval.Day, outputTemplate:"{Timestamp:o} [{Level:u4}] {Message:lj}{NewLine}{Exception}", retainedFileCountLimit: (fileLogFileCount > 0 ? (int?)fileLogFileCount : null));

            Log.Logger = loggerConfig
                .Enrich.FromLogContext()
                .Enrich.WithThreadId()
                .CreateLogger();

            try
            {
                Log.Information("Starting web host");
                BuildWebHost(args).Run();
                return 0;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Host terminated unexpectedly");
                return 1;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .UseSerilog()
                .Build();
    }
}
