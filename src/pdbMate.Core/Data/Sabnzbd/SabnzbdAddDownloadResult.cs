using System.Text.Json.Serialization;

namespace pdbMate.Core.Data.Sabnzbd
{
    public class SabnzbdAddDownloadResult
    {
        [JsonPropertyName("status")]
        public bool Status { get; set; }
        public string Error { get; set; }
    }
}