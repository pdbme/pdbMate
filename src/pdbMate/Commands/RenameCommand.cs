using pdbMate.Core.Interfaces;
using pdbme.pdbInfrastructure.Logging.Commands;
using Spectre.Console.Cli;
using System.ComponentModel;

namespace pdbMate.Commands
{
    public class RenameCommand : Command<RenameCommand.Settings>
    {
        private readonly IRenameWorkflow renameWorkflow;

        public RenameCommand(IRenameWorkflow renameWorkflow)
        {
            this.renameWorkflow = renameWorkflow;
        }

        public class Settings : LogCommandSettings
        {
            [CommandOption("-d|--dryrun")]
            [DefaultValue(false)]
            public bool DryRun { get; init; }
        }

        public override int Execute(CommandContext context, Settings settings)
        {
            renameWorkflow.Rename(settings.DryRun);
            return 0;
        }
    }
}
