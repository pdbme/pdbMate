using System;
using Microsoft.Extensions.Logging;

namespace pdbMate.Core
{
    public class Application : IApplication
    {
        private readonly ILogger<IApplication> logger;
        private readonly IRenameService renameService;
        private readonly IFileOperatingService fileOperatingService;

        public Application(ILogger<IApplication> logger, IRenameService renameService, IFileOperatingService fileOperatingService)
        {
            this.logger = logger;
            this.renameService = renameService;
            this.fileOperatingService = fileOperatingService;
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
    }
}
