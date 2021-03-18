using System.Collections.Generic;
using pdbMate.Core.Data;

namespace pdbMate.Core
{
    public interface IPdbApiService
    {
        List<VideoQuality> GetAllVideoQuality();
        List<Site> GetSites();
        List<Video> GetVideosBySite(Site site);
    }
}