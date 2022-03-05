using Microsoft.Extensions.Options;
using pdbMate.ApplicationSetupLogic;
using pdbMate.Core;
using pdbMate.Core.Data;
using pdbMate.Core.Interfaces;
using Spectre.Console;
using System;
using System.IO;
using System.Text.Json;

namespace pdbMate.SetupLogic
{
    public class Setup : ISetup
    {
        private AppSettingsResult appSettings;
        private readonly IPdbApiService pdbApiService;
        private readonly ISabnzbdService sabnzbdService;
        private readonly INzbgetService nzbgetService;

        public Setup(IPdbApiService pdbApiService, ISabnzbdService sabnzbdService, INzbgetService nzbgetService)
        {
            this.pdbApiService = pdbApiService;
            this.sabnzbdService = sabnzbdService;
            this.nzbgetService = nzbgetService;
        }

        public bool RunSetup()
        {
            appSettings = new AppSettingsResult();

            ReadApiKey();
            bool useForDownload = ReadActiveServices();
            if (useForDownload)
            {
                ReadDownloadClient();
            }
            ReadRenaming();

            return SaveToDisk();
        }

        private void ReadRenaming()
        {
            var useForRenaming = AnsiConsole.Confirm("Do you want to use pdbMate to rename your downloads (sort them into folders based on sitename and rename files into a humanfriendly format - very useful for media players like plex)?");
            if (!useForRenaming)
            {
                return;
            }

            var useStandardFormat = AnsiConsole.Confirm("Use default format for folder and filename renaming (Folder: {Video.Site.Sitename} File: {Video.ReleasedateShort} - {Video.Title} - {VideoQuality.SimplifiedName} - {PdbId}{FileExtension})?");
            if (!useStandardFormat)
            {
                appSettings.Rename.FilenameTemplate = AnsiConsole.Prompt(
                new TextPrompt<string>($"Enter your desired [green]filename template[/] for renaming:")
                    .PromptStyle("green")
                    .ValidationErrorMessage("[red]That's not a valid filename template[/]")
                    .Validate(filenameTemplate =>
                    {
                        if (!filenameTemplate.Contains("{FileExtension}"))
                        {
                            return ValidationResult.Error("[red]Please include at least the {FileExtension}[/]");
                        }

                        return ValidationResult.Success();
                    }));
                appSettings.Rename.FolderTemplate = AnsiConsole.Prompt(
                    new TextPrompt<string>($"Enter your desired [green]folder template[/] for renaming:")
                        .PromptStyle("green"));
            }

            bool doYouWantToAddAnother = false;
            do
            {
                var sourceFolder = AnsiConsole.Prompt(
                new TextPrompt<string>($"Add [green]folderpath[/] as source for renaming:")
                    .PromptStyle("green")
                    .ValidationErrorMessage("[red]That's not a valid folderpath[/]")
                    .Validate(folderpath =>
                    {
                        if (!Directory.Exists(folderpath))
                        {
                            return ValidationResult.Error("[red]This directory does not exist[/]");
                        }

                        return ValidationResult.Success();
                    }));

                appSettings.Rename.SourceFolders.Add(sourceFolder);
                doYouWantToAddAnother = AnsiConsole.Confirm("Do you want to add another source folder for renaming?");
            }
            while (doYouWantToAddAnother);

            appSettings.Rename.TargetFolder = AnsiConsole.Prompt(
            new TextPrompt<string>($"In which [green]target path[/] should renamed files be moved (every site will become its own subfolder if the default folder template setting wasn't changed):")
                .PromptStyle("green")
                .ValidationErrorMessage("[red]That's not a valid target path[/]")
                .Validate(targetFolder =>
                {
                    if (!Directory.Exists(targetFolder))
                    {
                        return ValidationResult.Error("[red]This directory does not exist[/]");
                    }

                    return ValidationResult.Success();
                }));
        }
        private void ReadApiKey()
        {
            var apikey = AnsiConsole.Prompt(
                new TextPrompt<string>($"Enter your [green]apikey[/] for {appSettings.PdbApi.BaseUrl}:")
                    .PromptStyle("green")
                    .ValidationErrorMessage("[red]That's not a valid apikey[/]")
                    .Validate(apikey =>
                    {
                        if (apikey.Length != 32)
                        {
                            return ValidationResult.Error("[red]The apikey must be 32 characters long[/]");
                        }

                        if (!IsApiKeyValid(apikey))
                        {
                            return ValidationResult.Error($"[red]Failed to connect to {appSettings.PdbApi.BaseUrl} with your apikey[/]");
                        }

                        return ValidationResult.Success();
                    }));

            appSettings.PdbApi.ApiKey = apikey;
            pdbApiService.setOptions(Options.Create(new PdbApiServiceOptions()
            {
                BaseUrl = appSettings.PdbApi.BaseUrl,
                Apikey = appSettings.PdbApi.ApiKey
            }));

            AnsiConsole.MarkupLine("[green]Successfully[/] connected with your apikey");
        }

        private bool IsApiKeyValid(string apiKey)
        {
            try
            {
                pdbApiService.setOptions(Options.Create(new PdbApiServiceOptions()
                {
                    BaseUrl = appSettings.PdbApi.BaseUrl,
                    Apikey = apiKey
                }));
                var r = pdbApiService.GetMyIndexer();
                return true;
            }
            catch
            {
                return false;
            }
        }

        private bool ReadActiveServices()
        {
            var useForDownload = AnsiConsole.Confirm("Do you want to use pdbMate to add downloads to your usenet client?");
            if (useForDownload)
            {
                var allowedQualities = AnsiConsole.Prompt(
                    new MultiSelectionPrompt<string>()
                        .Title("What [green]video quality[/] do you want to download?")
                        .InstructionsText(
                            "[grey](Press [blue]<space>[/] to toggle a quality, " +
                            "[green]<enter>[/] to accept)[/]")
                        .AddChoices(new[] {
                            "1080p", "2160p"
                        }));

                var keepOnlyHighestQuality = AnsiConsole.Confirm("When downloading from usenet, only keep the highest quality of the same release?");
                var downloadFavoriteActors = AnsiConsole.Confirm("Add downloads based on your favorite actors?");
                var downloadFavoriteSites = AnsiConsole.Confirm("Add downloads based on your favorite sites?");

                if (!downloadFavoriteActors && !downloadFavoriteSites)
                {
                    AnsiConsole.MarkupLine($"[red]You do not want to add downloads based on your [bold]favorite actors OR sites[/] - pdbMate would not add any downloads - let's start from the beginning.[/]");
                    ReadActiveServices();
                }

                appSettings.UsenetDownload.AllowedQualities = allowedQualities;
                appSettings.UsenetDownload.KeepOnlyHighestQuality = keepOnlyHighestQuality;
                appSettings.UsenetDownload.DownloadFavoriteSites = downloadFavoriteSites;
                appSettings.UsenetDownload.DownloadFavoriteActors = downloadFavoriteActors;
            }

            return useForDownload;
        }

        private void ReadDownloadClient()
        {
            var client = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Which [green]download client[/] do you want to use?")
                    .AddChoices(new[] {
                        "sabnzbd", "nzbget"
                    }));

            if (client == "sabnzbd")
            {
                ReadSabnzbd();
                appSettings.UsenetDownload.ActiveClient = DownloadClient.Sabnzbd;
                return;
            }

            ReadNzbget();
            appSettings.UsenetDownload.ActiveClient = DownloadClient.Nzbget;
        }

        private void ReadSabnzbd()
        {
            var hostname = AnsiConsole.Prompt(
                new TextPrompt<string>($"Enter [green]hostname[/] for sabnzbd (e.g. localhost):")
                    .PromptStyle("green")
                    .ValidationErrorMessage("[red]That's not a valid hostname[/]")
                    .Validate(hostname =>
                    {
                        if (hostname.Contains(':'))
                        {
                            return ValidationResult.Error("[red]Do not provide http or the port in the hostname[/]");
                        }

                        return ValidationResult.Success();
                    }));

            var useSsl = AnsiConsole.Confirm("Using SSL to connect?");
            hostname = $"http{(useSsl ? "s" : "")}://" + hostname;

            var port = AnsiConsole.Prompt(
                new TextPrompt<int>($"Enter [green]port[/] for sabnzbd (e.g. 8080):")
                    .PromptStyle("green")
                    .ValidationErrorMessage("[red]That's not a valid port[/]")
                    .Validate(port =>
                    {
                        if (port == 0)
                        {
                            return ValidationResult.Error("[red]Port 0 is not a valid port[/]");
                        }

                        return ValidationResult.Success();
                    }));

            hostname = hostname + ":" + port + "/sabnzbd/api";

            var apikey = AnsiConsole.Prompt(
                new TextPrompt<string>($"Enter [green]apikey[/] for sabnzbd:")
                    .PromptStyle("green")
                    .ValidationErrorMessage("[red]That's not a valid apikey[/]")
                    .Validate(apikey =>
                    {
                        if (apikey.Length != 32)
                        {
                            return ValidationResult.Error("[red]The apikey must be 32 characters long[/]");
                        }

                        return ValidationResult.Success();
                    }));

            if (!IsConnectionSabnzbdSuccessful(hostname, apikey))
            {
                AnsiConsole.MarkupLine($"[red]Cound not connect to sabnzbd on url: {hostname} - let's start from the beginning.[/]");
                ReadDownloadClient();
            }

            appSettings.Sabnzbd.Url = hostname;
            appSettings.Sabnzbd.ApiKey = apikey;
        }

        private void ReadNzbget()
        {
            var hostname = AnsiConsole.Prompt(
            new TextPrompt<string>($"Enter [green]hostname[/] for nzbget (e.g. localhost):")
                .PromptStyle("green")
                .ValidationErrorMessage("[red]That's not a valid hostname[/]")
                .Validate(hostname =>
                {
                    if (hostname.Contains(':'))
                    {
                        return ValidationResult.Error("[red]Do not provide http or the port in the hostname[/]");
                    }

                    return ValidationResult.Success();
                }));

            var useSsl = AnsiConsole.Confirm("Using SSL to connect?");

            var port = AnsiConsole.Prompt(
            new TextPrompt<int>($"Enter [green]port[/] for sabnzbd (e.g. 8080):")
                .PromptStyle("green")
                .ValidationErrorMessage("[red]That's not a valid port[/]")
                .Validate(port =>
                {
                    if (port == 0)
                    {
                        return ValidationResult.Error("[red]Port 0 is not a valid port[/]");
                    }

                    return ValidationResult.Success();
                }));

            var useAuth = AnsiConsole.Confirm("Do you need to specify username and password for accessing nzbget?");

            var username = "";
            var password = "";
            if (useAuth)
            {
                username = AnsiConsole.Prompt(new TextPrompt<string>($"Enter [green]username[/] for nzbget:")
                    .PromptStyle("green").AllowEmpty());

                password = AnsiConsole.Prompt(new TextPrompt<string>($"Enter [green]password[/] for nzbget:")
                    .PromptStyle("green").AllowEmpty());
            }

            if (!IsConnectionNzbgetSuccessful(hostname, port, username, password, useSsl))
            {
                AnsiConsole.MarkupLine($"[red]Cound not connect to nzbget on url: {hostname} - let's start from the beginning.[/]");
                ReadDownloadClient();
            }

            appSettings.Nzbget.Username = username;
            appSettings.Nzbget.Password = password;
            appSettings.Nzbget.Hostname = hostname;
            appSettings.Nzbget.Port = port;
            appSettings.Nzbget.UseHttps = useSsl;
        }

        private bool IsConnectionNzbgetSuccessful(string hostname, int port, string username, string password, bool useSsl)
        {
            nzbgetService.setOptions(Options.Create(new NzbgetServiceOptions()
            {
                Hostname = hostname,
                Port = port,
                Username = username,
                Password = password,
                UseHttps = useSsl
            }));

            try
            {
                return nzbgetService.GetVersion() > 0;
            }
            catch
            {
                return false;
            }
        }

        private bool IsConnectionSabnzbdSuccessful(string hostname, string apikey)
        {
            sabnzbdService.setOptions(Options.Create(new SabnzbdServiceOptions()
            {
                Url = hostname,
                ApiKey = apikey
            }));

            try
            {
                return !string.IsNullOrEmpty(sabnzbdService.GetVersion());
            }
            catch
            {
                return false;
            }
        }

        private bool SaveToDisk()
        {
            string s = JsonSerializer.Serialize(appSettings, new JsonSerializerOptions()
            {
                WriteIndented = true
            });

            File.WriteAllText("appsettings.json", s);

            if (File.Exists("appsettings.json"))
            {
                Console.WriteLine("Successfully written your settings to appsettings.json. Next start of pdbMate will run in autopilot mode with sabnzbd client.");
            }

            return true;
        }
    }
}
