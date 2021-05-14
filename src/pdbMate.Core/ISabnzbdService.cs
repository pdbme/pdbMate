using System.Collections.Generic;
using pdbMate.Core.Data.Nzbget;
using pdbMate.Core.Data.Sabnzbd;

namespace pdbMate.Core
{
    public interface ISabnzbdService
    {
        bool IsActive();
        bool CheckConnection();
        string GetVersion();
        string AddDownload(string url);
        SabnzbdQueue GetQueue(int start, int limit);
        SabnzbdHistory GetHistory(int start, int limit);
    }
}