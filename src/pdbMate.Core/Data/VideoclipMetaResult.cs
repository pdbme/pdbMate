namespace pdbMate.Core.Data
{
    public class VideoclipMetaResult
    {
        public SourceFile Source { get; set; }
        public Video Video { get; set; }
        public int VideoQualityId { get; set; }
        public string FileExtension { get; set; }
    }
}