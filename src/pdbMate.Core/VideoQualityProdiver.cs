using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using pdbMate.Core.Data;
using pdbMate.Core.Interfaces;

namespace pdbMate.Core
{
    public class VideoQualityProdiver : IVideoQualityProdiver
    {
        private readonly ILogger<IVideoQualityProdiver> logger;
        private readonly IPdbApiService pdbApi;
        private readonly Dictionary<int, VideoQuality> interalCache;
        public VideoQualityProdiver(ILogger<IVideoQualityProdiver> logger, IPdbApiService pdbApi)
        {
            this.logger = logger;
            this.pdbApi = pdbApi;

            interalCache = new Dictionary<int, VideoQuality>();
        }

        public VideoQuality GetById(int id)
        {
            interalCache.TryGetValue(id, out VideoQuality result);
            if (result == null)
            {
                FillCache();
                interalCache.TryGetValue(id, out result);
            }

            return result;
        }

        public VideoQuality GetByName(string name)
        {
            if (!interalCache.Any())
            {
                FillCache();
            }

            if (!interalCache.Any()) return null;
            foreach (var (_, value) in interalCache)
            {
                if (value.Name.Equals(name))
                {
                    return value;
                }
            }

            return null;
        }

        private void FillCache()
        {
            var videoQualities = pdbApi.GetAllVideoQuality();
            foreach (var videoQuality in videoQualities)
            {
                interalCache.TryAdd(videoQuality.Id, videoQuality);
            }
        }
    }
}
