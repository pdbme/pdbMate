using pdbMate.Core.Data;

namespace pdbMate.Core.Interfaces
{
    public interface IUsenetDownloadService
    {
        void Execute(bool dryRun, DownloadClient? client, int? backFillingActors, int? backFillingSites, int daysBack);
    }
}