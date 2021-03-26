using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace pdbMate.Core.Data.Sabnzbd
{
    public class SabnzbdQueueSlot
    {
        public int Index { get; set; }

        [JsonPropertyName("nzo_id")] 
        public string NzoId { get; set; }
        public string UnpackOpts { get; set; }
        public string Priority { get; set; }
        public string Script { get; set; }
        public string Filename { get; set; }
        public List<string> Labels { get; set; }
        public string Password { get; set; }
        public string Cat { get; set; }
        public string Mbleft { get; set; }
        public string Mb { get; set; }
        public string Size { get; set; }
        public string SizeLeft { get; set; }
        public string Percentage { get; set; }
        public string MbMissing { get; set; }
        public string Status { get; set; }
        public string Timeleft { get; set; }
        public string Eta { get; set; }
        [JsonPropertyName("avg_age")] 
        public string AverageAge { get; set; }

        [JsonPropertyName("has_rating")] 
        public bool HasRating { get; set; }
    }
}