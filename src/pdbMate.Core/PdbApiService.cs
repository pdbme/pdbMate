using System;
using System.Collections.Generic;
using System.IO;
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

        public PdbApiService(ILogger<IPdbApiService> logger, IOptions<PdbApiServiceOptions> options)
        {
            this.logger = logger;
            this.options = options.Value;

            client = new RestClient(this.options.BaseUrl);
            client.AddDefaultHeader("X-PORNDB-APIKEY", this.options.Apikey);
        }

        public List<VideoQuality> GetAllVideoQuality()
        {
            List<VideoQuality> videoQuality = new List<VideoQuality> {
                new VideoQuality() {
                    Id = 0, Name = "Unknown", SimplifiedName = "Unknown"
                },
                new VideoQuality()
                {
                    Id = 1, Name = "720p", SimplifiedName = "720p"
                },
                new VideoQuality()
                {
                    Id = 2, Name = "1080p", SimplifiedName = "1080p"
                },
                new VideoQuality()
                {
                    Id = 3, Name = "2160p", SimplifiedName = "2160p"
                },
                new VideoQuality()
                {
                    Id = 4, Name = "480p", SimplifiedName = "480p"
                }
            };

            return videoQuality;
        }

        public List<Site> GetSites()
        {

            var request = new RestRequest("api/pornsites", DataFormat.Json)
                .AddQueryParameter("limit", "100000");

            var response = client.Get<List<Site>>(request);
            return response.Data;
        }

        public List<Video> GetVideosBySite(Site site)
        {

            var request = new RestRequest("api/pornvideos", DataFormat.Json)
                .AddQueryParameter("limit", "100000")
                .AddQueryParameter("pornsite", site.Id.ToString());

            var response = client.Get<List<PdbApiPornvideo>>(request);
            var videosReturned = response.Data;

            var requestActors = new RestRequest("api/actors", DataFormat.Json)
                .AddQueryParameter("limit", "100000")
                .AddQueryParameter("pornsite", site.Id.ToString());

            var responseActors = client.Get<List<PdbApiPornstar>>(requestActors);
            var actorsReturned = responseActors.Data;

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
                            Actorname = actorFound.Starname,
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
                            Actorname = actorFound.Starname,
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

            return videos;
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

    internal class PdbApiPornstar
    {
        public int Id { get; set; }
        public string Starname { get; set; }
        public int Gender { get; set; }
    }
}
