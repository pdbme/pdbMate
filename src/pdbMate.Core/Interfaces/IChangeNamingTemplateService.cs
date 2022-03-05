namespace pdbMate.Core.Interfaces
{
    public interface IChangeNamingTemplateService
    {
        void RenameToNewTemplate(bool dryRun);
    }
}