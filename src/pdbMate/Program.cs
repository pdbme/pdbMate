using System;
using System.Collections.Generic;
using System.IO;
using CommandLine;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using pdbMate.CommandLineOptions;
using pdbMate.Core;
using Serilog;

namespace pdbMate
{
    public class Program
    {
        private static IConfigurationRoot _configuration;

        private static int Main(string[] args)
        {
            return Parser.Default.ParseArguments<RenameOptions, DownloadOptions, AutopilotOptions, ChangeNamingTemplateOptions>(args)
                .MapResult(
                    (RenameOptions opts) => RunApp(args, opts, AppAction.Rename),
                    (DownloadOptions opts) => RunApp(args, opts, AppAction.Download),
                    (AutopilotOptions opts) => RunApp(args, opts, AppAction.Autopilot),
                    (ChangeNamingTemplateOptions opts) => RunApp(args, opts, AppAction.ChangeNamingTemplate),
                    HandleParseError);
        }

        static int RunApp(string[] args, BaseOptions options, AppAction action)
        {
            _configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", false, true)
                .Build();

            var host = Host.CreateDefaultBuilder(args).ConfigureServices(services => {
                services.AddOptions();
                services.AddPdbApiService(_configuration.GetSection("PdbApi"));
                services.AddRenameService(_configuration.GetSection("Rename"));
                services.AddNzbgetService(_configuration.GetSection("Nzbget"));
                services.AddSabnzbdService(_configuration.GetSection("Sabnzbd"));
                services.AddUsenetDownloadService(_configuration.GetSection("UsenetDownload"));

                services.AddScoped<IApplication, Application>();
            }).UseSerilog().Build();

            var loggerConfig = new LoggerConfiguration();
            if (options.Verbose)
            {
                loggerConfig = loggerConfig.MinimumLevel.Debug();
            }
            else
            {
                loggerConfig = loggerConfig.MinimumLevel.Error();
            }

            loggerConfig = loggerConfig.WriteTo.Console();
            loggerConfig = loggerConfig.WriteTo.File("pdbmate.log", rollingInterval: RollingInterval.Day, fileSizeLimitBytes: 1000000, retainedFileCountLimit: 3);
            loggerConfig = loggerConfig.Enrich.FromLogContext();
            loggerConfig = loggerConfig.Enrich.WithProperty("Application", "pdbMate");
            loggerConfig = loggerConfig.MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning);
            loggerConfig = loggerConfig.MinimumLevel.Override("System", Serilog.Events.LogEventLevel.Warning);

            Log.Logger = loggerConfig.CreateLogger();

            try
            {
                Log.Debug("Starting up service");

                try
                {
                    var myApplication = host.Services.GetRequiredService<IApplication>();
                    if (options.DryRun)
                    {
                        Log.Information("DryRun is activated! Nothing will be made permanent.");
                    }

                    if (action == AppAction.Rename)
                    {
                        return myApplication.Rename(options.DryRun) ? (int)ExitCode.Success : (int) ExitCode.ApplicationError;
                    }
                    else if (action == AppAction.Download)
                    {
                        return myApplication.Download(options.DryRun, options.Client) ? (int)ExitCode.Success : (int)ExitCode.ApplicationError;
                    }
                    else if (action == AppAction.Autopilot)
                    {
                        return myApplication.Autopilot(options.DryRun, options.Client) ? (int)ExitCode.Success : (int)ExitCode.ApplicationError;
                    }else if (action == AppAction.ChangeNamingTemplate)
                    {
                        return myApplication.ChangeNamingTemplate(options.DryRun) ? (int)ExitCode.Success : (int)ExitCode.ApplicationError;
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Exception when executing Application.");
                    return (int)ExitCode.ApplicationError;
                }
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "There was a problem with the service");
            }
            finally
            {
                Log.CloseAndFlush();
            }

            return (int)ExitCode.ErrorOnStartup;
        }

        private static int HandleParseError(IEnumerable<Error> errs)
        {
            //handle errors
            return (int) ExitCode.ParseError;
        }
    }
}
