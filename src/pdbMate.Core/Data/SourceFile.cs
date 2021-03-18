namespace pdbMate.Core.Data
{
    public class SourceFile
    {
        public string Path { get; set; }
        public string FileExtension { get; set; }
        public string Filename { get; set; }
        public string Foldername { get; set; }
        public int Filesize { get; set; }
        public bool IsDuplicate { get; set; }
        public bool IsToSmall { get; set; }
        public bool IsNoVideoExtension { get; set; }
    }
}
