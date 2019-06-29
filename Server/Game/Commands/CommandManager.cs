using log4net;
using Platform_Racing_3_Server.Game.Commands.Match;
using Platform_Racing_3_Server.Game.Commands.Misc;
using Platform_Racing_3_Server.Game.Commands.User;
using Platform_Racing_3_Server_API.Game.Commands;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Platform_Racing_3_Server.Game.Commands
{
    internal class CommandManager
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private Dictionary<string, ICommand> Commands;

        internal CommandManager()
        {
            this.Commands = new Dictionary<string, ICommand>()
            {
                { "hello", new HelloCommand() },
                { "shutdown", new ShutdownCommand() },
                { "givepart", new GivePartCommand() },
                { "givehat", new GiveHatCommand() },
                { "broadcast", new BroadcastCommand() },
                { "kick", new KickCommand() },
                { "alert", new AlertCommand() },
                { "addhat", new AddHatCommand() },
                { "fakeprize", new FakePrizeCommand() },
                { "spawnaliens", new SpawnAliensCommand() },
                { "teleport", new TeleportCommand() },
                { "tournament", new TournamentCommand() },
                { "broadcaster", new BroadcasterCommand() },
                { "givebonusexp", new GiveBonusExpCommand() },
                { "help", new HelpCommand() },
            };
        }

        internal bool Execte(ICommandExecutor executor, string label, ReadOnlySpan<string> args)
        {
            if (this.Commands.TryGetValue(label, out ICommand command))
            {
                if (command.Permission == null || executor.HasPermission(command.Permission))
                {
                    try
                    {
                        command.OnCommand(executor, label, args);
                    }
                    catch(Exception ex)
                    {
                        CommandManager.Logger.Error(ex);

                        executor.SendMessage("Critical error while executing the command!");
                    }
                }
                else
                {
                    executor.SendMessage("You are lacking the permissions to use this command!");
                }

                return true;
            }

            return false;
        }
    }
}
