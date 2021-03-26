using System.Collections.Generic;
using pdbMate.Core.Data.Nzbget;
using pdbMate.Core.Data.Sabnzbd;

namespace pdbMate.Core
{
    public interface ISabnzbdService
    {
        bool CheckConnection();
        string GetVersion();
        SabnzbdQueue GetQueue(int start, int limit);
        SabnzbdHistory GetHistory(int start, int limit);
    }
}