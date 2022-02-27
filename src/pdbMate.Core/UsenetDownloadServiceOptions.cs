using System.Collections.Generic;

namespace pdbMate.Core
{
    public class UsenetDownloadServiceOptions
    {
        public List<string> AllowedQualities { get; set; }
        public bool KeepOnlyHighestQuality { get; set; }
        public bool DownloadFavoriteActors { get; set; }
        public bool DownloadFavoriteSites { get; set; }
    }
}
