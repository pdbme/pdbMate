namespace pdbMate.Core
{
    public interface IApplication
    {
        bool Rename(bool dryRun);
        bool ChangeNamingTemplate(bool dryRun);
        bool Download(bool dryRun, string client);
        bool Autopilot(bool dryRun, string client);
    }
}
