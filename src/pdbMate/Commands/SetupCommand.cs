using pdbMate.SetupLogic;
using pdbme.pdbInfrastructure.Logging.Commands;
using Spectre.Console.Cli;
using System;

namespace pdbMate.Commands
{
    public class SetupCommand : Command<SetupCommand.Settings>
    {
        private readonly ISetup setup;

        public SetupCommand(ISetup setup)
        {
            this.setup = setup ?? throw new ArgumentNullException(nameof(setup));
        }

        public class Settings : LogCommandSettings
        {
        }

        public override int Execute(CommandContext context, Settings settings)
        {
            setup.RunSetup();
            return 0;
        }
    }
}
