using System.Collections.Generic;
using Microsoft.Extensions.Options;
using pdbMate.Core.Data.Nzbget;

namespace pdbMate.Core.Interfaces
{
    public interface INzbgetService
    {
        void setOptions(IOptions<NzbgetServiceOptions> optionsToSet);
        void CheckConnection();
        int GetVersion();
        List<NzbgetQueue> GetQueue();
        List<NzbgetHistory> GetHistory();
        bool AddDownload(string url);
    }
}