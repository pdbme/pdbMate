using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace pdbMate.Core.Data.Sabnzbd
{
    public class SabnzbdSlot
    {
        public int Id { get; set; }
        public int Completed { get; set; }
        public string Name { get; set; }

        [JsonPropertyName("nzb_name")]
        public string NzbName { get; set; }

        public string Category { get; set; }

        [JsonPropertyName("pp")]
        public string PostProcessing { get; set; }
        public string Script { get; set; }
        public string Report { get; set; }
        public string Url { get; set; }
        public string Status { get; set; }

        [JsonPropertyName("nzo_id")]
        public string NzoId { get; set; }
        public string Storage { get; set; }
        public string Path { get; set; }

        [JsonPropertyName("script_log")]
        public string ScriptLog { get; set; }

        [JsonPropertyName("script_line")]
        public string ScriptLine { get; set; }

        [JsonPropertyName("download_time")]
        public int DownloadTime { get; set; }

        [JsonPropertyName("postproc_time")]
        public int PostProcessTime { get; set; }

        [JsonPropertyName("stage_log")]
        public List<SabnzbdSlotAction> StageLog { get; set; }
    }
}