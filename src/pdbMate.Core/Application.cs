using System;
using Microsoft.Extensions.Logging;
using pdbMate.Core.Data;

namespace pdbMate.Core
{
    public class Application : IApplication
    {
        private readonly ILogger<IApplication> logger;
        private readonly IRenameService renameService;
        private readonly IFileOperatingService fileOperatingService;
        private readonly IUsenetDownloadService usenetDownloadService;
        private readonly IChangeNamingTemplateService changeNamingTemplateService;

        public Application(ILogger<IApplication> logger, IRenameService renameService, IFileOperatingService fileOperatingService,
            IUsenetDownloadService usenetDownloadService, IChangeNamingTemplateService changeNamingTemplateService)
        {
            this.logger = logger;
            this.renameService = renameService;
            this.fileOperatingService = fileOperatingService;
            this.usenetDownloadService = usenetDownloadService;
            this.changeNamingTemplateService = changeNamingTemplateService;
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

        public bool Download(bool dryRun, string client)
        {
            var downloadClient = GetClientFromString(client);

            usenetDownloadService.Execute(dryRun, downloadClient);

            return true;
        }

        public bool Autopilot(bool dryRun, string client)
        {
            var downloadClient = GetClientFromString(client);

            Rename(dryRun);
            usenetDownloadService.Execute(dryRun, downloadClient);

            return true;
        }

        private DownloadClient GetClientFromString(string s)
        {
            s = s.Trim().ToLower();
            if (s.Equals("nzbget"))
            {
                return DownloadClient.Nzbget;
            }

            if (s.Equals("sabnzbd"))
            {
                return DownloadClient.Sabnzbd;
            }

            throw new ApplicationException($"Please specify a valid download client - given: {s} - expected: sabnzbd or nzbget");
        }

        public bool ChangeNamingTemplate(bool dryRun)
        {
            changeNamingTemplateService.RenameToNewTemplate(dryRun);
            return true;
        }
    }
}
