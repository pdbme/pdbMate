﻿using pdbMate.Core.Interfaces;
using pdbme.pdbInfrastructure.Logging.Commands;
using Spectre.Console.Cli;
using System.ComponentModel;

namespace pdbMate.Commands
{
    public class ChangeNamesCommand : Command<ChangeNamesCommand.Settings>
    {
        private readonly IChangeNamingTemplateService changeNamingTemplateService;

        public ChangeNamesCommand(IChangeNamingTemplateService changeNamingTemplateService)
        {
            this.changeNamingTemplateService = changeNamingTemplateService;
        }

        public class Settings : LogCommandSettings
        {
            [CommandOption("-d|--dryrun")]
            [DefaultValue(false)]
            public bool DryRun { get; init; }
        }

        public override int Execute(CommandContext context, Settings settings)
        {
            changeNamingTemplateService.RenameToNewTemplate(settings.DryRun);
            return 0;
        }
    }
}
