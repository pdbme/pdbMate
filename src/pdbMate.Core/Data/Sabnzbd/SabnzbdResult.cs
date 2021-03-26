namespace pdbMate.Core.Data.Sabnzbd
{
    public class SabnzbdResult
    {
        public string Status { get; set; }
        public string Error { get; set; }
        public string Version { get; set; }
        public SabnzbdHistory History { get; set; }
        public SabnzbdQueue Queue { get; set; }
    }
}
