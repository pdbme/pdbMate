namespace pdbMate.Core
{
    public interface IApplication
    {
        bool Rename(bool dryRun);
        bool Test(bool dryRun);
        bool Download(bool dryRun);
    }
}
