using Platform_Racing_3_Server.Game.Client;
using Platform_Racing_3_Server_API.Game.Commands;
using System;
using System.Collections.Generic;
using System.Text;

namespace Platform_Racing_3_Server.Game.Commands.Misc
{
    internal class HelpCommand : ICommand
    {
        public string Permission => null;

        public void OnCommand(ICommandExecutor executor, string label, ReadOnlySpan<string> args)
        {
            executor.SendMessage("<b><u>Developer / Administrator / Manager</u></b>"

                + "\n\n- /addhat [hat id]"
                + "\n- /broadcaster"
                + "\n- /broadcast"
                + "\n- /fakeprize [type] [prize id]"
                + "\n- /spawnaliens [alien amount]"
                + "\n- /teleport [x] [y]"
                + "\n- /alert [username] [message]"
                + "\n- /kick [username] [reason]"
                + "\n- /shutdown"
                + "\n- /tournament"
                + "\n- /givebonusexp [username] [amount]"
                + "\n- /givehat [username] [hat id]"
                + "\n- /givepart [username] [part id]"

                + "\n\n<b><u>Everyone</u></b>"

                + "\n\n- /help"
                + "\n- /hello"
                + "\n- /setworkers [worker amount]");
        }
    }
}
