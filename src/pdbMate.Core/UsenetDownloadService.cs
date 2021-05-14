using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using pdbMate.Core.Data;

namespace pdbMate.Core
{
    public class UsenetDownloadService : IUsenetDownloadService
    {
        private readonly ILogger<IUsenetDownloadService> logger;
        private readonly UsenetDownloadServiceOptions options;
        private readonly IRenameService renameService;
        private readonly IPdbApiService pdbApiService;
        private readonly IVideoQualityProdiver videoQualityProdiver;
        private readonly ISabnzbdService sabnzbdService;
        private readonly INzbgetService nzbgetService;
        private readonly IVideoMatching videoMatching;

        public UsenetDownloadService(ILogger<IUsenetDownloadService> logger, IOptions<UsenetDownloadServiceOptions> options, 
            IRenameService renameService, IPdbApiService pdbApiService, IVideoQualityProdiver videoQualityProdiver,
            ISabnzbdService sabnzbdService, INzbgetService nzbgetService, IVideoMatching videoMatching)
        {
            this.logger = logger;
            this.options = options.Value;
            this.renameService = renameService;
            this.pdbApiService = pdbApiService;
            this.videoQualityProdiver = videoQualityProdiver;
            this.sabnzbdService = sabnzbdService;
            this.nzbgetService = nzbgetService;
            this.videoMatching = videoMatching;
        }

        public void Execute(bool dryRun)
        {
            var knownVideosOnDisk = renameService.GetKnownVideoQualityResults();
            var favoriteSites = options.DownloadFavoriteSites ? pdbApiService.GetFavoriteSites() : null;
            var favoriteActors = options.DownloadFavoriteActors ? pdbApiService.GetFavoriteActors() : null;
            var releases = pdbApiService.GetReleases(1, 1000, 0, 0, "");
            var myIndexers = pdbApiService.GetMyIndexer();

            if(releases == null || !releases.Any())
            {
                logger.LogError("No releases found");
                return;
            }

            var resultList = new List<UsenetRelease>();
            foreach (var release in releases)
            {
                release.VideoQuality = videoQualityProdiver.GetByName(StringExtractor.ExtractQuality(release.Title));
                bool matchActor = favoriteActors != null && options.DownloadFavoriteActors && favoriteActors.Any(x => x.Id == release.Actor1 || x.Id == release.Actor2);
                bool matchSite = favoriteSites != null && options.DownloadFavoriteSites && favoriteSites.Any(x => x.Id == release.Site);

                if (myIndexers.Any(x => x.Id == release.Indexer) && (matchActor || matchSite) && !IsDuplicate(release, knownVideosOnDisk))
                {
                    logger.LogDebug($"Add {release.Title} to resultlist.");
                    resultList.Add(release);
                }
            }
            logger.LogInformation($"{resultList.Count} releases found.");

            if (options.KeepOnlyHighestQuality)
            {
                resultList = KeepOnlyHighestQuality(resultList);
                // TODO: remove releases that are knownVideosOnDisk with higher quality:

                logger.LogInformation($"{resultList.Count} releases after Filter: keep only highest quality release.");
            }

            var excludeReleases = new List<UsenetRelease>();
            excludeReleases.AddRange(GetReleasesToExcludeFromSabnzbd(resultList));
            excludeReleases.AddRange(GetReleasesToExcludeFromNzbget(resultList));

            var excludeReleasesIds = excludeReleases.Select(x => x.Id);
            resultList.RemoveAll(x => excludeReleasesIds.Contains(x.Id));
            logger.LogInformation($"{resultList.Count} releases after Filter: Exclude active downloads Downloads - exlcuded {excludeReleases.Count}.");

            resultList = RemoveDuplicates(resultList);
            logger.LogInformation($"{resultList.Count} releases after Filter: remove duplicates.");

            logger.LogInformation($"-- List releases to add");
            

            if (options.AddNzbTo.Equals("nzbget"))
            {
                foreach (var result in resultList)
                {
                    logger.LogInformation($"Add {result.Title} to download list (Id: {result.Id}, Indexer: {result.Indexer}, PID: {result.Video}).");
                    if (!dryRun)
                    {
                        var donwloadLink = GetDownloadLink(result);
                        logger.LogInformation($"Add to nzbget: {donwloadLink}");
                        nzbgetService.AddDownload(donwloadLink);
                    }
                }
            } 
            else if (options.AddNzbTo.Equals("sabnzbd"))
            {
                foreach (var result in resultList)
                {
                    logger.LogInformation($"Add {result.Title} to download list (Id: {result.Id}, Indexer: {result.Indexer}).");
                    if (!dryRun)
                    {
                        var donwloadLink = GetDownloadLink(result);
                        logger.LogInformation($"Add to sabnzbd: {donwloadLink}");
                        sabnzbdService.AddDownload(donwloadLink);
                    }
                }
            }
            else
            {
                throw new ApplicationException($"No valid entry for AddNzbTo, given: {options.AddNzbTo} - expected: sabnzbd or nzbget");
            }
            logger.LogInformation($"-----------------------");
        }

        private string GetDownloadLink(UsenetRelease release)
        {
            string url = "";
            var indexerId = release.Indexer;
            var indexer = pdbApiService.GetMyIndexer().SingleOrDefault(x => x.Id == indexerId);
            if (indexer != null)
            {
                url = indexer.Downloadurl.Replace("[USERID]", indexer.Apiuserid).Replace("[GUID]", release.Guid)
                    .Replace("[APIKEY]", indexer.Apikey);
            }

            return url;
        }

        private List<UsenetRelease> RemoveDuplicates(List<UsenetRelease> releases)
        {
            var result = new List<UsenetRelease>();
            var known = new List<KnownVideoQualityResult>();
            foreach (var r in releases)
            {
                if (known.Any(x => x.VideoId == r.Video && x.VideoQualityId == r.VideoQuality.Id))
                {
                    continue;
                }

                known.Add(new KnownVideoQualityResult()
                {
                    VideoId = r.Video,
                    VideoQualityId = r.VideoQuality.Id
                });
                result.Add(r);
            }

            return result;
        }


        private List<UsenetRelease> GetReleasesToExcludeFromSabnzbd(List<UsenetRelease> releases)
        {
            // All releases found in queue must be excluded from releases list
            List<UsenetRelease> excludeReleases = new List<UsenetRelease>();
            if (!sabnzbdService.IsActive())
            {
                return excludeReleases;
            }

            var slots = sabnzbdService.GetQueue(0, 1000).Slots;
            foreach (var slot in slots)
            {
                var videoFound = videoMatching.GetVideoByName(slot.Filename);
                if (videoFound != null)
                {
                    var quality = videoQualityProdiver.GetByName(StringExtractor.ExtractQuality(slot.Filename));

                    var releasesFound = releases
                        .Where(x => x.Video == videoFound.Id && x.VideoQuality.Id == quality.Id).ToList();
                    if (releasesFound.Any())
                    {
                        excludeReleases.AddRange(releasesFound);
                    }
                }
            }

            var slotsHistory = sabnzbdService.GetHistory(0, 1000).Slots;
            foreach (var slot in slotsHistory)
            {
                var videoFound = videoMatching.GetVideoByName(slot.Name);
                if (videoFound != null)
                {
                    var quality = videoQualityProdiver.GetByName(StringExtractor.ExtractQuality(slot.Name));

                    var releasesFound = releases
                        .Where(x => x.Video == videoFound.Id && x.VideoQuality.Id == quality.Id).ToList();
                    if (releasesFound.Any())
                    {
                        excludeReleases.AddRange(releasesFound);
                    }
                }
            }

            return excludeReleases;
        }

        private List<UsenetRelease> GetReleasesToExcludeFromNzbget(List<UsenetRelease> releases)
        {
            // All releases found in queue must be excluded from releases list
            List<UsenetRelease> excludeReleases = new List<UsenetRelease>();
            if (!nzbgetService.IsActive())
            {
                return excludeReleases;
            }

            var queueEntries = nzbgetService.GetQueue();
            if (queueEntries == null || queueEntries.Count == 0)
            {
                return excludeReleases;
            }

            foreach (var queueEntry in queueEntries)
            {
                var videoFound = videoMatching.GetVideoByName(queueEntry.NzbName);
                if (videoFound != null)
                {
                    var quality = videoQualityProdiver.GetByName(StringExtractor.ExtractQuality(queueEntry.NzbName));

                    var releasesFound = releases
                        .Where(x => x.Video == videoFound.Id && x.VideoQuality.Id == quality.Id).ToList();
                    if (releasesFound.Any())
                    {
                        excludeReleases.AddRange(releasesFound);
                    }
                }
            }

            var historyEntries = nzbgetService.GetHistory();
            if (historyEntries == null || historyEntries.Count == 0)
            {
                return excludeReleases;
            }

            foreach (var historyEntry in historyEntries)
            {
                var videoFound = videoMatching.GetVideoByName(historyEntry.NzbName);
                if (videoFound != null)
                {
                    var quality = videoQualityProdiver.GetByName(StringExtractor.ExtractQuality(historyEntry.NzbName));

                    var releasesFound = releases
                        .Where(x => x.Video == videoFound.Id && x.VideoQuality.Id == quality.Id).ToList();
                    if (releasesFound.Any())
                    {
                        excludeReleases.AddRange(releasesFound);
                    }
                }
            }

            return excludeReleases;
        }

        private bool IsQualityAllowed(UsenetRelease release)
        {
            if (release.VideoQuality == null)
            {
                return false; // releases without quality information will be ignored
            }

            if (!options.AllowedQualities.Contains(release.VideoQuality.SimplifiedName))
            {
                return false; // quality not allowed
            }

            return true;
        }

        private bool IsDuplicate(UsenetRelease release, List<KnownVideoQualityResult> knownVideosOnDisk)
        {
            if (!IsQualityAllowed(release))
            {
                return true; // handle as duplicate if quality is not allowed.
            }

            if (knownVideosOnDisk.Any(x => x.VideoId == release.Video && x.VideoQualityId == release.VideoQuality.Id))
            {
                return true; // release in exactly the quality is on disk
            }

            if (options.KeepOnlyHighestQuality)
            {
                var betterQualities = GetBetterQualitiesThan(release.VideoQuality.Id);
                if (betterQualities != null && betterQualities.Any() && knownVideosOnDisk.Any(x => x.VideoId == release.Video && betterQualities.Contains(x.VideoQualityId)))
                {
                    return true; // release in a higher quality is on disk
                }
            }

            return false;
        }

        private List<int> GetBetterQualitiesThan(int videoQualityId)
        {
            var videoQualities = pdbApiService.GetAllVideoQuality();
            var betterThanThis = videoQualities.Single(x => x.Id == videoQualityId);
            var betterQualities =
                videoQualities.Where(x => x.BetterQualityOrdinal > betterThanThis.BetterQualityOrdinal);
            return betterQualities.Select(x => x.Id).ToList();

        }

        private List<UsenetRelease> KeepOnlyHighestQuality(List<UsenetRelease> releases)
        {
            var highestQualities = GetHighestQualities(releases);

            List<UsenetRelease> filteredReleases = new List<UsenetRelease>();
            foreach (var release in releases)
            {
                var releaseFoundInMultiQualityRelease = highestQualities.SingleOrDefault(x => x.VideoId == release.Id);
                if (releaseFoundInMultiQualityRelease != null)
                {
                    if (release.VideoQuality.Id == releaseFoundInMultiQualityRelease.VideoQualityId)
                    {
                        filteredReleases.Add(release); // multiple releases found, but this is the highest quality
                    }
                }
                else
                {
                    filteredReleases.Add(release); // only one release available
                }
            }

            return filteredReleases;
        }

        private List<KnownVideoQualityResult> GetHighestQualities(List<UsenetRelease> releases)
        {
            List<KnownVideoQualityResult> bestQualities = new List<KnownVideoQualityResult>();
            var releasesWithMultipleQualities = releases.GroupBy(x => x.Video).Where(grp => grp.Count() > 1);
            foreach (var group in releasesWithMultipleQualities)
            {
                var bestQualityRelease = group.OrderBy(x => x.VideoQuality.BetterQualityOrdinal).FirstOrDefault();
                if (bestQualityRelease != null)
                {
                    bestQualities.Add(new KnownVideoQualityResult()
                    {
                        VideoId = bestQualityRelease.Video,
                        VideoQualityId = bestQualityRelease.VideoQuality.Id
                    });
                }
            }

            return bestQualities;
        }
    }
}
