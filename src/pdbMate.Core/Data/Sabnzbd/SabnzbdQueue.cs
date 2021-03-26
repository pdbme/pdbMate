using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace pdbMate.Core.Data.Sabnzbd
{
    public class SabnzbdQueue
    {
        public bool Paused { get; set; }

        [JsonPropertyName("pause_int")]
        public string PausedInt { get; set; }

        [JsonPropertyName("paused_all")]
        public bool PausedAll { get; set; }

        public string Diskspace1 { get; set; }
        public string Diskspace2 { get; set; }

        [JsonPropertyName("diskspace1_norm")]
        public string Diskspace1Normalized { get; set; }
        [JsonPropertyName("diskspace2_norm")]
        public string Diskspace2Normalized { get; set; }
        public string Diskspacetotal1 { get; set; }
        public string Diskspacetotal2 { get; set; }
        public string Loadavg { get; set; }
        public string Speedlimit { get; set; }
        [JsonPropertyName("speedlimit_abs")]
        public string SpeedlimitAbsolute { get; set; }
        [JsonPropertyName("have_warnings")]
        public string HaveWarnings { get; set; }
        public string Finishaction { get; set; }
        public string Quota { get; set; }
        [JsonPropertyName("have_quota")]
        public bool HaveQuota { get; set; }
        [JsonPropertyName("left_quota")]
        public string LeftQuota { get; set; }
        [JsonPropertyName("cache_art")]
        public string CacheArt { get; set; }
        [JsonPropertyName("cache_size")]
        public string CacheSize { get; set; }
        [JsonPropertyName("cache_max")]
        public string CacheMax { get; set; }
        [JsonPropertyName("kbpersec")]
        public string Kbpersec { get; set; }
        public string Speed { get; set; }
        public string Mbleft { get; set; }
        public string Mb { get; set; }
        public string Sizeleft { get; set; }
        public string Size { get; set; }

        [JsonPropertyName("noofslots_total")]
        public int NumberOfSlotsTotal { get; set; }
        public string Status { get; set; }
        public string Timeleft { get; set; }
        public string Eta { get; set; }
        [JsonPropertyName("refresh_rate")]
        public string RefreshRate { get; set; }
        public List<string> Categories { get; set; }

        [JsonPropertyName("rating_enable")]
        public bool RatingEnable { get; set; }
        public int Start { get; set; }
        public int Limit { get; set; }

        public int Finish { get; set; }


        public string Version { get; set; }

        public List<SabnzbdQueueSlot> Slots { get; set; }
    }
}