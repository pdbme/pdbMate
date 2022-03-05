using pdbMate.Core.Data;
using System.Collections.Generic;

namespace pdbMate.ApplicationSetupLogic
{
    public class AppSettingsResult
    {
        public AppSettingsRename Rename { get; set; } = new AppSettingsRename();
        public AppSettingsPdbApi PdbApi { get; set; } = new AppSettingsPdbApi();
        public AppSettingsNzbget Nzbget { get; set; } = new AppSettingsNzbget();
        public AppSettingsSabnzbd Sabnzbd { get; set; } = new AppSettingsSabnzbd();
        public AppSettingsUsenetDownload UsenetDownload { get; set; } = new AppSettingsUsenetDownload();
    }

    public class AppSettingsRename
    {
        public string FilenameTemplate { get; set; } = "{Video.ReleasedateShort} - {Video.Title} - {VideoQuality.SimplifiedName} - {PdbId}{FileExtension}";
        public string FolderTemplate { get; set; } = "{Video.Site.Sitename}";
        public List<string> SourceFolders { get; set; } = new List<string>();
        public string TargetFolder { get; set; }
    }

    public class AppSettingsPdbApi
    {
        public string BaseUrl { get; set; } = "https://api.porndb.me/";
        public string ApiKey { get; set; }
    }

    public class AppSettingsNzbget
    {
        public string Hostname { get; set; } = "127.0.0.1";
        public int Port { get; set; } = 6789;
        public string Username { get; set; } = "";
        public string Password { get; set; } = "";
        public bool UseHttps { get; set; } = false;
    }

    public class AppSettingsSabnzbd
    {
        public string Url { get; set; } = "http://127.0.0.1:8080/sabnzbd/api";
        public string ApiKey { get; set; } = "";
    }

    public class AppSettingsUsenetDownload
    {
        public List<string> AllowedQualities { get; set; } = new List<string>() { "1080p", "2160p"};
        public bool KeepOnlyHighestQuality { get; set; } = true;
        public bool DownloadFavoriteActors { get; set; } = true;
        public bool DownloadFavoriteSites { get; set; } = true;
        public DownloadClient ActiveClient { get; set; } = DownloadClient.Sabnzbd;
    }
}