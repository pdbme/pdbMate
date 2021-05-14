using System;
using Microsoft.Extensions.Logging;

namespace pdbMate.Core
{
    public class Application : IApplication
    {
        private readonly ILogger<IApplication> logger;
        private readonly IRenameService renameService;
        private readonly INzbgetService nzbgetService;
        private readonly ISabnzbdService sabnzbdService;
        private readonly IFileOperatingService fileOperatingService;
        private readonly IUsenetDownloadService usenetDownloadService;

        public Application(ILogger<IApplication> logger, IRenameService renameService, IFileOperatingService fileOperatingService,
            INzbgetService nzbgetService, ISabnzbdService sabnzbdService, IUsenetDownloadService usenetDownloadService)
        {
            this.logger = logger;
            this.renameService = renameService;
            this.fileOperatingService = fileOperatingService;
            this.nzbgetService = nzbgetService;
            this.sabnzbdService = sabnzbdService;
            this.usenetDownloadService = usenetDownloadService;
        }

        public bool Rename(bool dryRun)
        {
            var files = renameService.GetFilesToProcess();

            fileOperatingService.RemoveUnwantedFiles(files, dryRun);

            var renamerResults = renameService.ProcessBatch(files);
            var knownVideoResults = renameService.GetKnownVideoQualityResults();

            renamerResults = renameService.CheckForDuplicatesInResults(renamerResults);
            renamerResults = renameService.CheckForDuplicates(renamerResults, knownVideoResults);
            
            fileOperatingService.WriteJsonFromObject(renamerResults, $"rename_{DateTime.Now:yyMMdd_hh_mm}.json");
            fileOperatingService.MoveFilesBasedOnRenameResults(renamerResults, renameService.GetTargetPath(), dryRun);
            fileOperatingService.DeleteDirectories(renameService.GetEmptyDirectories(), dryRun);

            return true;
        }

        public bool Test(bool dryRun)
        {
            //nzbgetService.CheckConnection();
            sabnzbdService.AddDownload("https://drunkenslug.com/getnzb/902d469ddc95da4750c5f34bcd786067245a1df0.nzb&i=93754&r=e9437ae410a9eb6becb7e33d7ba2bd42");

            /*
            nzbgetService.CheckConnection(); 
            var result = nzbgetService.GetQueue();
            logger.LogInformation($"Found in queue: {result.Count} entries.");
            var resultHistory = nzbgetService.GetHistory();
            logger.LogInformation($"Found in history: {resultHistory.Count} entries.");
            */

            /*
            sabnzbdService.CheckConnection();
            var resultSabnzbd = sabnzbdService.GetQueue(0, 100);
            logger.LogInformation($"Found in sabnzbd queue: {resultSabnzbd.Slots.Count} entries.");
            var resultSabnzbdHistory = sabnzbdService.GetHistory(0, 100);
            logger.LogInformation($"Found in sabnzbd history: {resultSabnzbdHistory?.Slots?.Count} entries.");
            */
            return true;
        }

        public bool Download(bool dryRun)
        {
            /*
            nzbgetService.CheckConnection(); 
            var result = nzbgetService.GetQueue();
            logger.LogInformation($"Found in queue: {result.Count} entries.");
            var resultHistory = nzbgetService.GetHistory();
            logger.LogInformation($"Found in history: {resultHistory.Count} entries.");
            */

            /*
            sabnzbdService.CheckConnection();
            var resultSabnzbd = sabnzbdService.GetQueue(0, 100);
            logger.LogInformation($"Found in sabnzbd queue: {resultSabnzbd.Slots.Count} entries.");
            var resultSabnzbdHistory = sabnzbdService.GetHistory(0, 100);
            logger.LogInformation($"Found in sabnzbd history: {resultSabnzbdHistory?.Slots?.Count} entries.");
            */

            usenetDownloadService.Execute(dryRun);

            return true;
        }
    }
}
