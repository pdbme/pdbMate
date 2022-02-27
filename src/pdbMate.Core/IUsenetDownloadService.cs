using pdbMate.Core.Data;

namespace pdbMate.Core
{
    public interface IUsenetDownloadService
    {
        void Execute(bool dryRun, DownloadClient client);
    }
}