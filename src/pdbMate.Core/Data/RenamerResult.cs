namespace pdbMate.Core.Data
{
    public class RenamerResult
    {
        public SourceFile Source { get; set; }
        public string Filename { get; set; }
        public string Foldername { get; set; }
        public string FileExtension { get; set; }
        public int VideoQualityId { get; set; }
        public Video Video { get; set; }
    }
}
