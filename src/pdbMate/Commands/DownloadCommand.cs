using pdbMate.Core.Data;
using pdbMate.Core.Interfaces;
using Spectre.Console.Cli;
using System;
using System.ComponentModel;

namespace pdbMate.Commands
{
    public class DownloadCommand : Command<DownloadCommand.Settings>
    {
        private readonly IUsenetDownloadService usenetDownloadService;

        public DownloadCommand(IUsenetDownloadService usenetDownloadService)
        {
            this.usenetDownloadService = usenetDownloadService ?? throw new ArgumentNullException(nameof(usenetDownloadService));
        }

        public class Settings : CommandSettings
        {
            [CommandOption("-d|--dryrun")]
            [DefaultValue(false)]
            public bool DryRun { get; init; }

            [CommandOption("-c|--client")]
            public DownloadClient? Client { get; init; }
        }

        public override int Execute(CommandContext context, Settings settings)
        {
            usenetDownloadService.Execute(settings.DryRun, settings.Client, null, null, 0);
            return 0;
        }
    }
}
