namespace pdbMate.Core.Data.Nzbget
{
    public class NzbgetQueue
    {
        public int FirstId { get; set; }
        public int LastId { get; set; }
        public string Status { get; set; }
        public int NzbId { get; set; }
        public string NzbName { get; set; }
        public string NzbNicename { get; set; }
        public string Kind { get; set; }
        public string Url { get; set; }
        public string NzbFilename { get; set; }
        public string DestDir { get; set; }
        public string FinalDir { get; set; }
        public string Category { get; set; }
        public string UrlStatus { get; set; }
        public int FileSizeMb { get; set; }
        public int FileCount { get; set; }
        public bool Deleted { get; set; }
        public int TotalArticles { get; set; }
        public int SuccessArticles { get; set; }
        public int FailedArticles { get; set; }
        public int Health { get; set; }
        public int CriticalHealth { get; set; }
        public string DupeKey { get; set; }
        public int DupeScore { get; set; }
        public string DupeMode { get; set; }
    }
}
