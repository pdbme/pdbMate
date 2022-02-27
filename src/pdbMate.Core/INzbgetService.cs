using System;
using System.Collections.Generic;
using pdbMate.Core.Data;
using pdbMate.Core.Data.Nzbget;

namespace pdbMate.Core
{
    public interface INzbgetService
    {
        void CheckConnection();
        int GetVersion();
        List<NzbgetQueue> GetQueue();
        List<NzbgetHistory> GetHistory();
        bool AddDownload(string url);
    }
}