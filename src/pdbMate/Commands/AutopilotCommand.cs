using pdbMate.Core.Data;
using pdbMate.Core.Interfaces;
using Spectre.Console.Cli;
using System.ComponentModel;

namespace pdbMate.Commands
{
    public class AutopilotCommand : Command<AutopilotCommand.Settings>
    {
        private readonly IRenameWorkflow renameWorkflow;
        private readonly IUsenetDownloadService usenetDownloadService;

        public AutopilotCommand(IRenameWorkflow renameWorkflow, IUsenetDownloadService usenetDownloadService)
        {
            this.renameWorkflow = renameWorkflow;
            this.usenetDownloadService = usenetDownloadService;
        }

        public class Settings : CommandSettings
        {
            [CommandOption("-d|--dryrun")]
            [DefaultValue(false)]
            public bool DryRun { get; set; }

            [CommandOption("-c|--client")]
            public DownloadClient? Client { get; init; }
        }

        public override int Execute(CommandContext context, Settings settings)
        {
            renameWorkflow.Rename(settings.DryRun);
            usenetDownloadService.Execute(settings.DryRun, settings.Client, null, null, 0);
            return 0;
        }
    }
}
