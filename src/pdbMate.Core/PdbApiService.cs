using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using pdbMate.Core.Data;
using RestSharp;

namespace pdbMate.Core
{
    public class PdbApiService : IPdbApiService
    {
        private readonly ILogger<IPdbApiService> logger;
        private readonly PdbApiServiceOptions options;
        private readonly RestClient client;

        private List<Site> cachedSites;
        private List<UsenetIndexer> cachedIndexers;
        private readonly Dictionary<int,List<Video>> cachedVideos;

        public PdbApiService(ILogger<IPdbApiService> logger, IOptions<PdbApiServiceOptions> options)
        {
            this.logger = logger;
            this.options = options.Value;

            client = new RestClient(this.options.BaseUrl);
            client.AddDefaultHeader("X-PORNDB-APIKEY", this.options.Apikey);

            cachedVideos = new Dictionary<int, List<Video>>();
        }

        public List<VideoQuality> GetAllVideoQuality()
        {
            List<VideoQuality> videoQuality = new List<VideoQuality> {
                new VideoQuality() {
                    Id = 0, Name = "Unknown", SimplifiedName = "Unknown", BetterQualityOrdinal = 0
                },
                new VideoQuality()
                {
                    Id = 1, Name = "720p", SimplifiedName = "720p", BetterQualityOrdinal = 2
                },
                new VideoQuality()
                {
                    Id = 2, Name = "1080p", SimplifiedName = "1080p", BetterQualityOrdinal = 3
                },
                new VideoQuality()
                {
                    Id = 3, Name = "2160p", SimplifiedName = "2160p", BetterQualityOrdinal = 4
                },
                new VideoQuality()
                {
                    Id = 4, Name = "480p", SimplifiedName = "480p", BetterQualityOrdinal = 1
                }
            };

            return videoQuality;
        }

        public List<Site> GetSites()
        {
            if (cachedSites != null) return cachedSites;

            var request = new RestRequest("api/pornsites", Method.Get)
                .AddQueryParameter("limit", "100000");

            var response = client.GetAsync<List<Site>>(request).GetAwaiter().GetResult();
            cachedSites = response;

            return cachedSites;
        }

        public List<Video> GetVideosBySite(Site site)
        {
            if (cachedVideos.ContainsKey(site.Id))
            {
                return cachedVideos[site.Id];
            }

            var request = new RestRequest("api/pornvideos", Method.Get)
                .AddQueryParameter("limit", "100000")
                .AddQueryParameter("pornsite", site.Id.ToString());

            var response = client.GetAsync<List<PdbApiPornvideo>>(request).GetAwaiter().GetResult();
            var videosReturned = response;

            var requestActors = new RestRequest("api/actors", Method.Get)
                .AddQueryParameter("limit", "100000")
                .AddQueryParameter("pornsite", site.Id.ToString());

            var responseActors = client.GetAsync<List<Actor>>(requestActors).GetAwaiter().GetResult();
            var actorsReturned = responseActors;

            List<Video> videos = new List<Video>();
            foreach (PdbApiPornvideo videoReturned in videosReturned)
            {
                var actors = new List<Actor>();
                if (videoReturned.Pornstar1 > 0)
                {
                    var actorFound = actorsReturned.FirstOrDefault(x => x.Id == videoReturned.Pornstar1);
                    if (actorFound != null)
                    {
                        actors.Add(new Actor()
                        {
                            Id = actorFound.Id,
                            Actorname = actorFound.Actorname,
                            Gender = actorFound.Gender
                        });
                    }
                }

                if (videoReturned.Pornstar2 > 0)
                {
                    var actorFound = actorsReturned.FirstOrDefault(x => x.Id == videoReturned.Pornstar2);
                    if (actorFound != null)
                    {
                        actors.Add(new Actor()
                        {
                            Id = actorFound.Id,
                            Actorname = actorFound.Actorname,
                            Gender = actorFound.Gender
                        });
                    }
                }

                var video = new Video()
                {
                    Id = videoReturned.Id,
                    Site = new Site()
                    {
                        Id = site.Id,
                        Sitename = site.Sitename
                    },
                    Actors = actors,
                    Releasedate = videoReturned.ReleaseDate,
                    Title = videoReturned.VideoTitle,
                    FullTitle = videoReturned.FullTitle
                };
                videos.Add(video);
            }

            cachedVideos.Add(site.Id, videos);
            return videos;
        }

        public List<Actor> GetFavoriteActors()
        {
            var request = new RestRequest("api/myactors", Method.Get);

            var response= client.GetAsync<List<Actor>>(request).GetAwaiter().GetResult();
            return response;
        }

        public List<Site> GetFavoriteSites()
        {
            var request = new RestRequest("api/mysites", Method.Get);

            var response = client.GetAsync<List<Site>>(request).GetAwaiter().GetResult();
            return response;
        }
        
        public List<UsenetRelease> GetReleases(int page, int take, int? actor, int? site, string search)
        {
            var request = new RestRequest("api/usenet/releases", Method.Get)
                .AddQueryParameter("page", page.ToString())
                .AddQueryParameter("take", take.ToString());

            if (actor != null && actor != 0)
            {
                request.AddQueryParameter("actor", actor.Value.ToString());
            }

            if (!string.IsNullOrEmpty(search))
            {
                request.AddQueryParameter("search", search);
            }

            if (site != null && site != 0)
            {
                request.AddQueryParameter("site", site.Value.ToString());
            }

            var response = client.GetAsync<List<UsenetRelease>>(request).GetAwaiter().GetResult();
            return response;
        }

        public List<UsenetIndexer> GetMyIndexer()
        {
            if (cachedIndexers != null)
            {
                return cachedIndexers;
            }

            var request = new RestRequest("api/myindexer", Method.Get);
            var response = client.GetAsync<List<UsenetIndexer>>(request).GetAwaiter().GetResult();
            cachedIndexers = response;
            return cachedIndexers;
        }
    }

    internal class PdbApiPornvideo
    {
        public int Id { get; set; }
        public int Pornsite { get; set; }
        public string VideoTitle { get; set; }
        public string FullTitle { get; set; } // for Fulltext search
        public string FullReleaseTitle { get; set; } // for Fulltext search
        public DateTime Created { get; set; }
        public DateTime ReleaseDate { get; set; }
        public int Pornstar1 { get; set; }
        public int Pornstar2 { get; set; }
        public int SceneRelease { get; set; }
        public string PreviewImageUrl { get; set; }
        public string PreviewVideoUrl { get; set; }
    }
}
