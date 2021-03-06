﻿using System.Collections.Generic;
using pdbMate.Core.Data;

namespace pdbMate.Core
{
    public interface IPdbApiService
    {
        List<VideoQuality> GetAllVideoQuality();
        List<Site> GetSites();
        List<Video> GetVideosBySite(Site site);
        List<Actor> GetFavoriteActors();
        List<Site> GetFavoriteSites();
        List<UsenetRelease> GetReleases(int page, int take, int? actor, int? site, string search);
        List<UsenetIndexer> GetMyIndexer();
    }
}