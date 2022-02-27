using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using pdbMate.Core.Data;

namespace pdbMate.Core
{
    public class ChangeNamingTemplateService : IChangeNamingTemplateService
    {
        private readonly ILogger<ChangeNamingTemplateService> logger;
        private readonly IPdbApiService pdbApi;
        private readonly IRenameService renameService;
        private readonly IVideoQualityProdiver videoQualityProdiver;
        private readonly RenameServiceOptions renameServiceOptions;

        public ChangeNamingTemplateService(ILogger<ChangeNamingTemplateService> logger, IPdbApiService pdbApi, IRenameService renameService, IVideoQualityProdiver videoQualityProdiver, IOptions<RenameServiceOptions> renameServiceOptions)
        {
            this.logger = logger;
            this.pdbApi = pdbApi;
            this.renameService = renameService;
            this.videoQualityProdiver = videoQualityProdiver;
            this.renameServiceOptions = renameServiceOptions.Value;
        }

        public void RenameToNewTemplate(bool dryRun)
        {
            var sites = pdbApi.GetSites();

            string targetFolder = renameServiceOptions.TargetFolder;
            var folders = Directory.EnumerateDirectories(targetFolder, "*", SearchOption.TopDirectoryOnly).ToList();
            foreach (string folder in folders)
            {
                var di = new DirectoryInfo(folder);
                var site = sites.SingleOrDefault(x => x.Sitename == di.Name);
                if(site == null)
                {
                    logger.LogInformation($"Site for folder {di.Name} not found.");
                    continue;
                }

                var videos = pdbApi.GetVideosBySite(site);
                ProcessBySite(site, videos, folder, dryRun);
            }
        }

        private void ProcessBySite(Site site, List<Video> videos, string searchFolder, bool dryRun)
        {
            if (!Directory.Exists(searchFolder))
            {
                logger.LogError($"Directory {searchFolder} does not exist.");
                return;
            }

            var files = Directory.EnumerateFiles(searchFolder, "*.*", SearchOption.TopDirectoryOnly).ToList();
            foreach (var file in files)
            {
                Match match = Regex.Match(file, @"P([0-9]{7})", RegexOptions.IgnoreCase);

                if (!match.Success)
                {
                    continue;
                }

                int videoQualityId = 0;
                bool parsed = int.TryParse(match.Groups[1].Value, out var pdbId);
                if (!parsed)
                {
                    continue;
                }

                FileInfo fi = new FileInfo(file);
                videoQualityId = videoQualityProdiver.GetByName(StringExtractor.ExtractQuality(fi.Name))?.Id ?? 0;

                var video = videos.SingleOrDefault(x => x.Id == pdbId);
                if(video == null)
                {
                    logger.LogInformation($"Video info for id {pdbId} not found (in videos for site {site.Sitename}).");
                    continue;
                }

                string newFilename = renameService.ReplaceTemplatePlaceholders(renameServiceOptions.FilenameTemplate, new RenamerResult()
                {
                    Video = video,
                    VideoQualityId = videoQualityId,
                    FileExtension = fi.Extension,
                    Filename = fi.Name,
                    Foldername = searchFolder,
                    Source = new SourceFile()
                    {
                        Filename = fi.Name,
                        FileExtension = fi.Extension
                    }
                });

                logger.LogInformation($"Rename {fi.Name} to {newFilename}");
                if (!dryRun)
                {
                    File.Move(Path.Combine(searchFolder, fi.Name), Path.Combine(searchFolder, newFilename));
                }
            }
        }
    }
}