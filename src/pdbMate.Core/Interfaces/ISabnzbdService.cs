using Microsoft.Extensions.Options;
using pdbMate.Core.Data.Sabnzbd;

namespace pdbMate.Core.Interfaces
{
    public interface ISabnzbdService
    {
        void setOptions(IOptions<SabnzbdServiceOptions> optionsToSet);
        bool CheckConnection();
        string GetVersion();
        bool AddDownload(string url);
        SabnzbdQueue GetQueue(int start, int limit);
        SabnzbdHistory GetHistory(int start, int limit);
    }
}