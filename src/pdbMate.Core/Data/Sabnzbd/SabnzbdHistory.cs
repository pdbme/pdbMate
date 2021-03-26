using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace pdbMate.Core.Data.Sabnzbd
{
    public class SabnzbdHistory
    {
        [JsonPropertyName("total_size")]
        public string TotalSize { get; set; }

        [JsonPropertyName("month_size")]
        public string MonthSize { get; set; }

        [JsonPropertyName("week_size")]
        public string WeekSize { get; set; }

        [JsonPropertyName("day_size")]
        public string DaySize { get; set; }

        [JsonPropertyName("noofslots")]
        public int NumberOfSlots { get; set; }

        [JsonPropertyName("last_history_update")]
        public int LastHistoryUpdate { get; set; }

        public string Version { get; set; }

        public List<SabnzbdSlot> Slots { get; set; }
    }
}