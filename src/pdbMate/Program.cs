using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using pdbMate.Commands;
using pdbMate.Core;
using pdbMate.SetupLogic;
using pdbme.pdbInfrastructure.DependencyInjection.Services;
using pdbme.pdbInfrastructure.Logging.Services;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;
using Spectre.Console;
using Spectre.Console.Cli;

namespace pdbMate
{
    public class Program
    {
        private static IConfigurationRoot _configuration;

        private static int Main(string[] args)
        {
            AnsiConsole.Write(
                new FigletText("pdbMate")
                    .LeftAligned()
                    .Color(Color.Purple));

            bool appSettingsFound = File.Exists("appsettings.json");
            if (!appSettingsFound)
            {
                Console.Error.WriteLine("No appsettings.json found! Starting interactive setup...");
                args = new string[] { "setup" };
            }

            if (args.Length == 0)
            {
                args = GetArgsByInteractiveMode();
            }

            return RunWithArguments(args);
        }

        private static int RunWithArguments(string[] args)
        {
            var isVerbose = IsVerbose(args);
            var serviceCollections = ConfigureApp(isVerbose);
            var registrar = new TypeRegistrar(serviceCollections);

            var app = new CommandApp(registrar);
            app.Configure(config =>
            {
                config.PropagateExceptions();

                config.SetInterceptor(new LogInterceptor());

                config.SetExceptionHandler(ex =>
                {
                    Log.Logger.Error(ex.Message + " Stacktrace: " + ex.StackTrace);
                    return -1;
                });

                config.AddCommand<RenameCommand>("rename")
                    .WithDescription("Rename and sort video files in subfolders by sitename.")
                    .WithExample(new[] { "rename", "--dryrun" });
                config.AddCommand<SetupCommand>("setup")
                    .WithDescription("Interactive setup to configure pdbMate.")
                    .WithExample(new[] { "setup" });
                config.AddCommand<DownloadCommand>("download")
                    .WithDescription("Rename and sort video files in subfolders by sitename.")
                    .WithExample(new[] { "download", "--dryrun" })
                    .WithExample(new[] { "download", "--dryrun", "--client", "nzbget" });
                config.AddCommand<ChangeNamesCommand>("changenames")
                    .WithDescription("Apply different naming template to an existing folder (a folder with sorted videos - only videos with a pdbid in their filename will be renamed).")
                    .WithExample(new[] { "changenames", "--dryrun" });
                config.AddCommand<BackfillingCommand>("backfilling")
                    .WithDescription("Backfilling by favorite sites and actors (0 means all favorite actors or sites - otherwise the id of one site or actor).")
                    .WithExample(new[] { "backfilling", "--actor", "0" })
                    .WithExample(new[] { "backfilling", "--actor", "0", "--site", "0" })
                    .WithExample(new[] { "backfilling", "--site", "6" });
            });

            return app.Run(args);
        }

        private static ServiceCollection ConfigureApp(bool isVerbose)
        {
            _configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", true, false)
                .Build();

            var services = new ServiceCollection();
            services.AddOptions();

            services.AddLogging(opt =>
            {
                var loggerConfig = new LoggerConfiguration()
                    .MinimumLevel.Information()
                    .MinimumLevel.ControlledBy(LogInterceptor.LogLevel)
                    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                    .WriteTo.Console(theme: AnsiConsoleTheme.Code, outputTemplate: "[{Level:u3}] {Message:lj}{NewLine}{Exception}")
                    .WriteTo.File("log.txt", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 7, outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
                    .CreateLogger();
                opt.AddSerilog(loggerConfig);
            });

            /*
            services.AddLogging(opt =>
            {
                opt.SetMinimumLevel(isVerbose ? LogLevel.Debug : LogLevel.Information);
                opt.AddVerySimpleConsoleLogger();
            });
            */

            services.AddPdbApiService(_configuration.GetSection("PdbApi"));
            services.AddRenameService(_configuration.GetSection("Rename"));
            services.AddNzbgetService(_configuration.GetSection("Nzbget"));
            services.AddSabnzbdService(_configuration.GetSection("Sabnzbd"));
            services.AddUsenetDownloadService(_configuration.GetSection("UsenetDownload"));

            services.AddScoped<ISetup, Setup>();
            return services;
        }

        private static bool IsVerbose(string[] args)
        {
            foreach (string arg in args)
            {
                if(arg == "--verbose" || arg == "-v")
                {
                    return true;
                }
            }

            return false;
        }

        private static string[] GetArgsByInteractiveMode()
        {
            var argList = new List<string>();

            var action = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Which [green]action[/] do you want to execute?")
                    .AddChoices(new[] {
                        "rename", "download", "autopilot", "setup", "backfilling"
                    }));

            argList.Add(action);

            var isVerbose = AnsiConsole.Confirm("Run action in verbose logging mode?");
            if (isVerbose)
            {
                argList.Add("--verbose");
            }

            if (action == "rename" || action == "download" || action == "autopilot" || action == "backfilling")
            {
                var isDryRun = AnsiConsole.Confirm("Execute action as dryrun (does not modify anything on disk)?");
                if (isDryRun)
                {
                    argList.Add("--dryrun");
                }
            }

            if (action == "backfilling")
            {
                var doBackfillingForActors = AnsiConsole.Confirm("Do backfilling for all your favorite actors?");
                if (doBackfillingForActors)
                {
                    argList.Add("--actor");
                    argList.Add("0");
                }

                var doBackfillingForSites = AnsiConsole.Confirm("Do backfilling for all your favorite sites?");
                if (doBackfillingForSites)
                {
                    argList.Add("--site");
                    argList.Add("0");
                }

                var daysBack = AnsiConsole.Prompt(
                    new TextPrompt<int>($"Enter the maximum [green]number of days[/] you want to go back and backfill:")
                        .PromptStyle("green")
                        .ValidationErrorMessage("[red]That's not a valid number of days[/]")
                        .Validate(id =>
                        {
                            if (id == 0)
                            {
                                return ValidationResult.Error($"[red]Number of days should be more than 0[/]");
                            }

                            if (id > 1800)
                            {
                                return ValidationResult.Error($"[red]Number of days should be less than 1800[/]");
                            }

                            return ValidationResult.Success();
                        }));
                argList.Add("--daysback");
                argList.Add(daysBack.ToString());

                if (!doBackfillingForActors && !doBackfillingForSites)
                {
                    var siteId = AnsiConsole.Prompt(
                    new TextPrompt<int>($"Enter [green]site id[/] you want to backfill (enter 0 to skip and continue with an actor id):")
                        .PromptStyle("green")
                        .ValidationErrorMessage("[red]That's not a valid site id[/]")
                        .Validate(id =>
                        {
                            if (id < 0)
                            {
                                return ValidationResult.Error($"[red]Id {id} is not known[/]");
                            }

                            return ValidationResult.Success();
                        }));
                    if(siteId > 0)
                    {
                        argList.Add("--site");
                        argList.Add(siteId.ToString());
                    }
                    else
                    {
                        var actorId = AnsiConsole.Prompt(
                            new TextPrompt<int>($"Enter [green]actor id[/] you want to backfill:")
                                .PromptStyle("green")
                                .ValidationErrorMessage("[red]That's not a valid actor id[/]")
                                .Validate(id =>
                                {
                                    if (id == 0)
                                    {
                                        return ValidationResult.Error($"[red]Id {id} is not known[/]");
                                    }

                                    return ValidationResult.Success();
                                }));
                        argList.Add("--actor");
                        argList.Add(actorId.ToString());
                    }
                }
            }

            return argList.ToArray();
        }
    }
}
