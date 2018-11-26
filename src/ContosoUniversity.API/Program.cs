using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace ContosoUniversity.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostContext, config) =>
                {
                    var env = hostContext.HostingEnvironment;
                    config
                        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                        .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true)
                        .AddCommandLine(args)
                        .AddEnvironmentVariables();
                })
                .UseSerilog((hostingContext, loggerConfig) => ConfigureSerilog(loggerConfig, hostingContext))
                .UseStartup<Startup>();

        private static void ConfigureSerilog(LoggerConfiguration loggerConfig, WebHostBuilderContext hostingContext)
        {
            string path = "./logs/log.txt";
            string outputTemplate = "{UtcTimestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}][{SourceContext}] {Message:lj}{NewLine}{Exception}";

            loggerConfig
                .MinimumLevel.Information()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .Enrich.With(new UtcTimestampEnricher())
                .WriteTo.Console()
                .WriteTo.Async(sinkConfig =>
                {
                    sinkConfig.File(path,
                        outputTemplate: outputTemplate,
                        rollingInterval: RollingInterval.Day,
                        shared: true);
                })
                .ReadFrom.Configuration(hostingContext.Configuration);
        }

        public class UtcTimestampEnricher : ILogEventEnricher
        {
            public void Enrich(LogEvent logEvent, ILogEventPropertyFactory pf)
            {
                logEvent.AddPropertyIfAbsent(pf.CreateProperty("UtcTimestamp", logEvent.Timestamp.UtcDateTime));
            }
        }
    }
}
