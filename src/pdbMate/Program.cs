using System;
using System.Collections.Generic;
using System.IO;
using CommandLine;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using pdbMate.Core;
using Serilog;
// ReSharper disable ClassNeverInstantiated.Local
// ReSharper disable UnusedAutoPropertyAccessor.Local

namespace pdbMate
{
    public class Program
    {
        private static IConfigurationRoot _configuration;

        private class BaseOptions
        {
            [Option('v', "verbose", Required = false, HelpText = "Set output to verbose messages.")]
            public bool Verbose { get; set; }

            [Option('d', "dryrun", Required = false, HelpText = "Run without performing changes to the filesystem.")]
            public bool DryRun { get; set; }
        }

        [Verb("rename", HelpText = "Rename and sort video files.")]
        private class RenameOptions : BaseOptions
        {

        }

        [Verb("add", HelpText = "add test.")]
        private class AddOptions : BaseOptions
        {

        }

        [Verb("test", HelpText = "do a test.")]
        private class TestOptions : BaseOptions
        {

        }

        [Verb("download", HelpText = "Autodownload from usenet.")]
        private class DownloadOptions : BaseOptions
        {

        }

        [Verb("autopilot", HelpText = "Autopilot for usenet.")]
        private class AutopilotOptions : BaseOptions
        {

        }

        private static int Main(string[] args)
        {
            return Parser.Default.ParseArguments<AddOptions, RenameOptions, TestOptions, DownloadOptions, AutopilotOptions>(args)
                .MapResult(
                    (AddOptions opts) => RunApp(args, opts, AppAction.Add),
                    (RenameOptions opts) => RunApp(args, opts, AppAction.Rename),
                    (TestOptions opts) => RunApp(args, opts, AppAction.Test),
                    (DownloadOptions opts) => RunApp(args, opts, AppAction.Download),
                    (AutopilotOptions opts) => RunApp(args, opts, AppAction.Autopilot),
                    HandleParseError);
        }

        static int RunApp(string[] args, BaseOptions options, AppAction action)
        {
            _configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", false, true)
                .Build();

            var loggerConfig = new LoggerConfiguration()
                .ReadFrom.Configuration(_configuration);

            if (options.Verbose)
            {
                loggerConfig.MinimumLevel.Debug();
            }

            Log.Logger = loggerConfig.CreateLogger();

            try
            {
                Log.Debug("Starting up service");

                using var host = CreateHostBuilder(args).Build();
                using var serviceScope = host.Services.CreateScope();
                var provider = serviceScope.ServiceProvider;

                try
                {
                    var myApplication = provider.GetRequiredService<IApplication>();
                    if (options.DryRun)
                    {
                        Log.Information("DryRun is activated! Nothing will be made permanent.");
                    }

                    if (action == AppAction.Rename)
                    {
                        return myApplication.Rename(options.DryRun) ? (int)ExitCode.Success : (int) ExitCode.ApplicationError;
                    }
                    else if (action == AppAction.Add)
                    {
                        //myApplication.Add(options.DryRun);
                    }
                    else if (action == AppAction.Test)
                    {
                        return myApplication.Test(options.DryRun) ? (int)ExitCode.Success : (int)ExitCode.ApplicationError;
                    }
                    else if (action == AppAction.Download)
                    {
                        return myApplication.Download(options.DryRun) ? (int)ExitCode.Success : (int)ExitCode.ApplicationError;
                    }
                    else if (action == AppAction.Autopilot)
                    {
                        return myApplication.Download(options.DryRun) ? (int)ExitCode.Success : (int)ExitCode.ApplicationError;
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

        private static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((_, services) =>
                {
                    services.AddOptions();
                    services.AddPdbApiService(_configuration.GetSection("PdbApi"));
                    services.AddRenameService(_configuration.GetSection("Rename"));
                    services.AddNzbgetService(_configuration.GetSection("Nzbget"));
                    services.AddSabnzbdService(_configuration.GetSection("Sabnzbd"));
                    services.AddUsenetDownloadService(_configuration.GetSection("UsenetDownload"));

                    services.AddScoped<IApplication, Application>();
                }).UseSerilog();
    }

    internal enum AppAction
    {
        Rename = 0,
        Add = 1,
        Test = 2,
        Download = 3,
        Autopilot = 4
    }

    internal enum ExitCode
    {
        Success = 0,
        ErrorOnStartup = 1,
        ApplicationError = 2,
        ParseError = 3
    }
}
