using pdbMate.Core.Data;
using pdbMate.Core.Interfaces;
using pdbme.pdbInfrastructure.Logging.Commands;
using Spectre.Console.Cli;
using System;
using System.ComponentModel;

namespace pdbMate.Commands
{
    public class BackfillingCommand : Command<BackfillingCommand.Settings>
    {
        private readonly IUsenetDownloadService usenetDownloadService;

        public BackfillingCommand(IUsenetDownloadService usenetDownloadService)
        {
            this.usenetDownloadService = usenetDownloadService ?? throw new ArgumentNullException(nameof(usenetDownloadService));
        }

        public class Settings : LogCommandSettings
        {
            [CommandOption("-d|--dryrun")]
            [DefaultValue(false)]
            public bool DryRun { get; init; }

            [CommandOption("-c|--client")]
            public DownloadClient? Client { get; init; }

            [CommandOption("--actor")]
            public int? Actor { get; init; }

            [CommandOption("--site")]
            public int? Site { get; init; }

            [CommandOption("--daysback")]
            [DefaultValue(30)]
            public int DaysBack { get; init; }
        }

        public override int Execute(CommandContext context, Settings settings)
        {
            usenetDownloadService.Execute(settings.DryRun, settings.Client, settings.Actor, settings.Site, settings.DaysBack);
            return 0;
        }
    }
}
