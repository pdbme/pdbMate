using System.Collections.Generic;
using pdbMate.Core.Data;

namespace pdbMate.Core
{
    public interface IVideoMatching
    {
        Video GetVideoByName(string name);
        Video GetVideoByDate(List<Video> videos, string name);
        Video GetVideoByEpisode(List<Video> videos, string name);
    }
}