using System;
using System.Collections.Generic;

namespace pdbMate.Core.Data
{
    public class Video
    {
        public int Id { get; set; }
        public Site Site { get; set; }
        public string Title { get; set; }
        public string FullTitle { get; set; }
        public DateTime Releasedate { get; set; }
        public List<Actor> Actors { get; set; }
    }
}
