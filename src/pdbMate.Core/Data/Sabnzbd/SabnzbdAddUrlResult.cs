using System.Collections.Generic;

namespace pdbMate.Core.Data.Sabnzbd
{
    public class SabnzbdAddUrlResult
    {
        public bool Status { get; set; }
        public List<string> Nzo_ids { get; set; }
    }
}
