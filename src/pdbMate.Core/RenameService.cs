using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using pdbMate.Core.Data;
using pdbMate.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace pdbMate.Core
{
    public class RenameService : IRenameService
    {
        private readonly ILogger<IRenameService> logger;
        private readonly IPdbApiService pdbApi;
        private readonly IVideoQualityProdiver videoQualityProdiver;
        private readonly RenameServiceOptions renameServiceOptions;

        public RenameService(ILogger<IRenameService> logger, IPdbApiService pdbApi, IVideoQualityProdiver videoQualityProdiver, IOptions<RenameServiceOptions> renameServiceOptions)
        {
            this.logger = logger;
            this.pdbApi = pdbApi;
            this.videoQualityProdiver = videoQualityProdiver;
            this.renameServiceOptions = renameServiceOptions.Value;
        }

        public string GetTargetPath()
        {
            return renameServiceOptions.TargetFolder;
        }

        public List<SourceFile> GetFilesToProcess()
        {
            logger.LogInformation("Search for files in folders specified in appsettings.json:");
            List<string> files = new List<string>();
            foreach (var sourceFolder in renameServiceOptions.SourceFolders)
            {
                
                if (Directory.Exists(sourceFolder))
                {
                    logger.LogInformation(sourceFolder);
                    files.AddRange(Directory.EnumerateFiles(sourceFolder, "*.*", SearchOption.AllDirectories).ToList());
                }
                else
                {
                    logger.LogWarning("Folder does not exists: " + sourceFolder);
                }
            }

            files = files.Where(file => !file.Contains("_UNPACK_")).ToList();

            return GetFileAttributes(files);
        }

        public List<string> GetEmptyDirectories()
        {
            logger.LogInformation("Search for empty directories in source folders specified in appsettings.json:");
            List<string> emptyDirectories = new List<string>();
            foreach (var sourceFolder in renameServiceOptions.SourceFolders)
            {
                var folders = Directory.GetDirectories(sourceFolder);
                foreach (var folder in folders)
                {
                    if (Directory.Exists(folder) && !Directory.EnumerateFileSystemEntries(folder).Any())
                    {
                        emptyDirectories.Add(folder);
                    }
                }

            }

            return emptyDirectories;
        }

        private List<SourceFile> GetFileAttributes(List<string> sourceFiles)
        {
            List<SourceFile> result = new List<SourceFile>();
            List<string> allowedExtension = new List<string>(){ ".mp4", ".mkv" };
            int minimumFilesizeInMegabyte = 150;

            foreach (var sourceFile in sourceFiles)
            {
                if (!File.Exists(sourceFile))
                {
                    continue;
                }

                var fi = new FileInfo(sourceFile);

                var resultFile = new SourceFile()
                {
                    Path = sourceFile,
                    Filename = Path.GetFileNameWithoutExtension(sourceFile),
                    Foldername = fi.Directory?.Name ?? "",
                    FileExtension = Path.GetExtension(sourceFile).ToLower(),
                    IsNoVideoExtension = false,
                    IsDuplicate = false,
                    IsToSmall = false
                };

                if (!allowedExtension.Contains(resultFile.FileExtension))
                {
                    resultFile.IsNoVideoExtension = true;
                }

                if (fi.Length < (minimumFilesizeInMegabyte * 1000 * 1000))
                {
                    resultFile.IsToSmall = true;
                }

                result.Add(resultFile);
            }

            return result;
        }

        public List<RenamerResult> ProcessBatch(List<SourceFile> sourceFiles)
        {
            List<RenamerResult> resultList = ProcessFiles(sourceFiles);
            foreach (var r in resultList)
            {
                r.FileExtension = r.Source.FileExtension;
                if (r.Video != null)
                {
                    r.Filename = ReplaceTemplatePlaceholders(renameServiceOptions.FilenameTemplate, r);
                    r.Foldername = ReplaceTemplatePlaceholders(renameServiceOptions.FolderTemplate, r);
                }
            }

            return resultList;
        }

        public List<RenamerResult> ProcessFiles(List<SourceFile> sourceFiles)
        {
            List<RenamerResult> resultList = new List<RenamerResult>();
            foreach (var sourceFile in sourceFiles)
            {
                var v = new RenamerResult()
                {
                    Source = sourceFile,
                    Video = null,
                    VideoQualityId = 0
                };

                if (!sourceFile.IsNoVideoExtension && !sourceFile.IsToSmall)
                {
                    var titleAndSite = GetRelevantTitleAndSite(sourceFile.Filename, sourceFile.Foldername);
                    if (titleAndSite != null)
                    {
                        var title = titleAndSite.Item1;
                        var site = titleAndSite.Item2;

                        logger.LogDebug($"Next GetVideoByFilename for {title} - Site: {site.Sitename}");
                        Video video = GetVideoByFilename(title, site);
                        logger.LogDebug($"Result GetVideoByFilename: {video?.Title ?? ""}");
                        if (video != null)
                        {
                            v.Video = video;
                            v.VideoQualityId = videoQualityProdiver.GetByName(StringExtractor.ExtractQuality(title))?.Id ?? 0;
                        }
                    }
                }

                resultList.Add(v);
            }

            return resultList;
        }

        public List<KnownVideoQualityResult> GetKnownVideoQualityResults()
        {
            List<KnownVideoQualityResult> result = new List<KnownVideoQualityResult>();
            string targetFolder = renameServiceOptions.TargetFolder;
            if (!Directory.Exists(targetFolder))
            {
                logger.LogError($"Target directory {targetFolder} does not exist.");
                return null;
            }

            var files = Directory.EnumerateFiles(targetFolder, "*.*", SearchOption.AllDirectories).ToList();
            foreach (var file in files)
            {
                Match match = Regex.Match(file, @"P([0-9]{7})", RegexOptions.IgnoreCase);

                if (match.Success)
                {
                    string pdbIdString = match.Groups[1].Value;
                    bool parsed = int.TryParse(pdbIdString, out var pdbId);
                    if (parsed)
                    {
                        FileInfo fi = new FileInfo(file);
                        result.Add(new KnownVideoQualityResult()
                        {
                            VideoId = pdbId,
                            VideoQualityId = videoQualityProdiver.GetByName(StringExtractor.ExtractQuality(fi.Name))?.Id ?? 0
                        });
                    }
                }
            }

            return result;
        }

        public List<RenamerResult> CheckForDuplicates(List<RenamerResult> renamerResults, List<KnownVideoQualityResult> knownVideoQualityResults)
        {
            foreach (var renamerResult in renamerResults)
            {
                if (renamerResult.Video != null && knownVideoQualityResults.Any(x => x.VideoId == renamerResult.Video.Id && x.VideoQualityId == renamerResult.VideoQualityId))
                {
                    renamerResult.Source.IsDuplicate = true;
                }
            }

            return renamerResults;
        }

        public List<RenamerResult> CheckForDuplicatesInResults(List<RenamerResult> renamerResults)
        {
            List<KnownVideoQualityResult> knownVideoQualityResultsInResult = new List<KnownVideoQualityResult>();
            foreach (var renamerResult in renamerResults)
            {
                if (renamerResult.Video != null && knownVideoQualityResultsInResult.Any(x => x.VideoId == renamerResult.Video.Id && x.VideoQualityId == renamerResult.VideoQualityId))
                {
                    renamerResult.Source.IsDuplicate = true;
                }
                else
                {
                    if (renamerResult.Video != null)
                    {
                        knownVideoQualityResultsInResult.Add(new KnownVideoQualityResult() { VideoId = renamerResult.Video.Id, VideoQualityId = renamerResult.VideoQualityId });
                    }
                }
            }

            return renamerResults;
        }

        private Tuple<string, Site> GetRelevantTitleAndSite(string filenameWithoutExtension, string directoryName)
        {
            var site = GetSiteByFirstword(filenameWithoutExtension, directoryName);
            if (site == null)
            {
                return null;
            }

            bool firstWordInFolder = directoryName.ToLower().Contains(site.Sitename.ToLower());
            string title = filenameWithoutExtension;
            if (firstWordInFolder)
            {
                title = directoryName;
            }

            return new Tuple<string, Site>(title, site);
        }

        private Video GetVideoByFilename(string title, Site site)
        {
            DateTime? releaseDate = StringExtractor.ExtractDate(title);
            string episode = StringExtractor.ExtractEpisode(title);

            List<Video> videos = GetVideosBySiteWithCached(site);

            if (releaseDate != null)
            {
                var foundMatchingVideos = videos.Where(x => x.Releasedate == releaseDate.Value).ToList();
                logger.LogDebug("Found videos (by ReleaseDate): " + foundMatchingVideos.Count + " Date: " + releaseDate.Value.ToString("yyyyMMdd"));
                if (!foundMatchingVideos.Any() || foundMatchingVideos.Count >= 6) return null;
                foreach (var foundMatchingVideo in foundMatchingVideos)
                {
                    logger.LogDebug("Video: " + foundMatchingVideo.Title + " matching: " + title + " ?");

                    if (foundMatchingVideo.Title.Contains(" "))
                    {
                        string firstWord = foundMatchingVideo.Title.Substring(0,
                            foundMatchingVideo.Title.IndexOf(" ", StringComparison.InvariantCulture));
                        if (title.Contains(firstWord))
                        {
                            return foundMatchingVideo;
                        }
                    }

                    string foundTitle = foundMatchingVideo.Title.Replace(" ", ".").ToLower();
                    string titleSimplified = title.ToLower().Replace(" ", ".");

                    if (titleSimplified.Contains(foundTitle))
                    {
                        return foundMatchingVideo;
                    }
                }

                return null;
            }

            if (!string.IsNullOrEmpty(episode))
            {
                var foundMatchingVideos = videos.Where(x => x.FullTitle.Contains("E" + episode)).ToList();
                logger.LogDebug("Found videos (by Episode): " + foundMatchingVideos.Count + " Episode: E" + episode);
                if (foundMatchingVideos.Any() && foundMatchingVideos.Count < 6)
                {
                    foreach (var foundMatchingVideo in foundMatchingVideos)
                    {
                        var firstwordLength = foundMatchingVideo.Title.IndexOf(" ", StringComparison.InvariantCulture);
                        if (firstwordLength > 0)
                        {
                            var firstWord = foundMatchingVideo.Title.Substring(0, firstwordLength);
                            if (title.Contains(firstWord))
                            {
                                return foundMatchingVideo;
                            }
                        }
                    }
                }
            }

            return null;
        }

        private Site GetSiteByFirstword(string filenameWithoutExtension, string directoryName)
        {
            List<Site> sites = GetSites();

            List<string> separatorArray = new List<string> { ".", " ", "-"};

            Site foundSite = null;
            foreach (var separator in separatorArray)
            {
                var site = GetSiteByFilenameWithFirstWordSeparator(sites, separator, directoryName);
                if (site != null)
                {
                    foundSite = site;
                    break;
                }
            }

            if (foundSite == null)
            {
                foreach (var separator in separatorArray)
                {
                    var site = GetSiteByFilenameWithFirstWordSeparator(sites, separator, filenameWithoutExtension);
                    if (site != null)
                    {
                        foundSite = site;
                        break;
                    }
                }
            }

            return foundSite;
        }

        private Site GetSiteByFilenameWithFirstWordSeparator(List<Site> sites, string separator, string filenameWithoutExtension)
        {
            if (filenameWithoutExtension.Contains(separator))
            {
                var firstWordBySeparator = filenameWithoutExtension.Substring(0, filenameWithoutExtension.IndexOf(separator, StringComparison.Ordinal));
                if (!string.IsNullOrWhiteSpace(firstWordBySeparator) && firstWordBySeparator.Length >= 3)
                {
                    var siteFound = sites.FirstOrDefault(x => string.Equals(x.Sitename, firstWordBySeparator, StringComparison.CurrentCultureIgnoreCase));
                    if (siteFound != null)
                    {
                        return siteFound;
                    }
                }
            }

            return null;
        }

        private List<Site> GetSites()
        {
            return pdbApi.GetSites();
        }

        private readonly Dictionary<int, List<Video>> cachedVideosBySite = new Dictionary<int, List<Video>>();
        private List<Video> GetVideosBySiteWithCached(Site site)
        {
            if (cachedVideosBySite.ContainsKey(site.Id))
            {
                return cachedVideosBySite[site.Id];
            }

            List<Video> videos = pdbApi.GetVideosBySite(site);
            cachedVideosBySite.Add(site.Id, videos);
            return videos;
        }

        public string ReplaceTemplatePlaceholders(string template, RenamerResult v)
        {
            string r = template;
            r = r.Replace("{Video.Site.Sitename}", v.Video.Site.Sitename);
            r = r.Replace("{Video.Site.Id}", v.Video.Site.Id.ToString());
            r = r.Replace("{Video.Id}", v.Video.Id.ToString());
            r = r.Replace("{Video.Title}", v.Video.Title);
            r = r.Replace("{Video.Releasedate}", v.Video.Releasedate.ToString("yyyy-MM-dd"));
            r = r.Replace("{Video.ReleasedateShort}", v.Video.Releasedate.ToString("yy-MM-dd"));
            r = r.Replace("{VideoQuality.SimplifiedName}", videoQualityProdiver.GetById(v.VideoQualityId).SimplifiedName);
            r = r.Replace("{VideoQuality.Name}", videoQualityProdiver.GetById(v.VideoQualityId).Name);
            r = r.Replace("{FileExtension}", v.FileExtension);
            r = r.Replace("{PdbId}", $"P{v.Video.Id:D7}");
            r = r.Replace("{OriginalFilename}", v.Source.Filename);

            return r;
        }
    }
}
