using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace pdbMate.Core.Data.Nzbget
{
    public class NzbgetRequestAddDownload
    {
        [JsonPropertyName("method")]
        public string Method { get; set; }

        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("params")]
        public object[] Params { get; set; }
    }
}