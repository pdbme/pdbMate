using System;

namespace pdbMate.Core.Data
{
    public class UsenetRelease
    {
        public int Id { get; set; }
        public int Indexer { get; set; }
        public int Video { get; set; }
        public int Site { get; set; }
        public string Guid { get; set; }
        public string Title { get; set; }
        public int Filesize { get; set; }
        public int Actor1 { get; set; }
        public int Actor2 { get; set; }
        public DateTime Created { get; set; }
        public VideoQuality VideoQuality { get; set; }
    }
}