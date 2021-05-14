using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using pdbMate.Core.Data;

namespace pdbMate.Core
{
    public class VideoMatching : IVideoMatching
    {
        private readonly ILogger<VideoMatching> logger;
        private readonly IPdbApiService pdbApi;
        private readonly Dictionary<string, Video> interalCache;
        public VideoMatching(ILogger<VideoMatching> logger, IPdbApiService pdbApi)
        {
            this.logger = logger;
            this.pdbApi = pdbApi;

            interalCache = new Dictionary<string, Video>();
        }

        public Video GetVideoByName(string name)
        {
            if (interalCache.ContainsKey(name))
            {
                return interalCache[name];
            }

            var site = FindSiteByFirstWord(StringExtractor.GetSeparator(name), name);
            if (site == null)
            {
                return null;
            }

            var videos = pdbApi.GetVideosBySite(site);

            var byDate = GetVideoByDate(videos, name);
            if (byDate != null)
            {
                interalCache.Add(name, byDate);
                return byDate;
            }

            var byEpisode = GetVideoByEpisode(videos, name);
            if (byEpisode != null)
            {
                interalCache.Add(name, byEpisode);
                return byEpisode;
            }

            var byNormalization = GetVideoByNormalization(videos, name);
            interalCache.Add(name, byNormalization);
            return byNormalization;
        }

        public Video GetVideoByDate(List<Video> videos, string name)
        {
            var dateExtracted = StringExtractor.ExtractDate(name);
            if (dateExtracted != null)
            {
                int foundVideosByDateCount = videos.Count(x => x.Releasedate == dateExtracted);
                if (foundVideosByDateCount == 1)
                {
                    return videos.Single(x => x.Releasedate == dateExtracted);
                }

                var videosFound = videos.Where(x => x.Releasedate == dateExtracted).ToList();
                var videoFoundResult = GetVideoByNormalization(videosFound, name);
                if (videoFoundResult != null)
                {
                    return videoFoundResult;
                }
            }

            return null;
        }

        public Video GetVideoByEpisode(List<Video> videos, string name)
        {
            var episodeExtracted = StringExtractor.ExtractEpisode(name);
            if (episodeExtracted != null)
            {
                int foundVideosByEpisodeCount = videos.Count(x => x.FullTitle.Contains(" " + episodeExtracted));
                if (foundVideosByEpisodeCount == 1)
                {
                    return videos.Single(x => x.FullTitle.Contains(" " + episodeExtracted));
                }

                var videosFound = videos.Where(x => x.FullTitle.Contains(" " + episodeExtracted)).ToList();
                var videoFoundResult = GetVideoByNormalization(videosFound, name);
                if (videoFoundResult != null)
                {
                    return videoFoundResult;
                }
            }

            return null;
        }

        private Video GetVideoByNormalization(List<Video> videos, string name)
        {
            var prefixString = StringExtractor.ExtractPrefixBeforeQualityAndReleasegroup(name);
            if (!string.IsNullOrEmpty(prefixString))
            {
                var n = StringNormalizer.Normalize(prefixString);
                var foundByNormalizationVideos = new List<Video>();
                foreach (var videoFound in videos)
                {
                    var nFound = StringNormalizer.Normalize(videoFound.FullTitle);

                    if (n.Equals(nFound))
                    {
                        foundByNormalizationVideos.Add(videoFound);
                    }
                }

                if (foundByNormalizationVideos.Count == 1)
                {
                    return foundByNormalizationVideos[0];
                }
            }

            return null;
        }

        private Site FindSiteByFirstWord(string separator, string name)
        {
            var sites = pdbApi.GetSites();
            var firstWordBySeparator = name.Substring(0, name.IndexOf(separator, StringComparison.Ordinal));
            if (!string.IsNullOrWhiteSpace(firstWordBySeparator) && firstWordBySeparator.Length >= 3)
            {
                var siteFound = sites.FirstOrDefault(x => string.Equals(x.Sitename, firstWordBySeparator, StringComparison.CurrentCultureIgnoreCase));
                if (siteFound != null)
                {
                    return siteFound;
                }
            }

            return null;
        }

    }
}
