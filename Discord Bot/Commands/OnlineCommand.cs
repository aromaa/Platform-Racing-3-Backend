using Discord.Commands;
using Discord.Commands.Builders;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Platform_Racing_3_Common.Server;

namespace Discord_Bot.Commands
{
    public class OnlineCommand : ModuleBase<SocketCommandContext>
    {
        private ServerManager ServerManager;

        public OnlineCommand(ServerManager serverManager)
        {
            this.ServerManager = serverManager;
        }

        [Command("pr3online")]
        [Summary("Returns server status and active player count.")]
        public Task GetOnlinePlayersCount()
        {
            EmbedBuilder embed = new EmbedBuilder();
            
            embed.WithTitle("Server Status");
            embed.WithColor(Color.Blue);
            embed.WithAuthor(author = > { author.WithName(this.Context.User.Username + this.Context.User.Discriminator).WithIconUrl(this.Context.User.GetAvatarUrl()) });
            
            StringBuilder stringBuilder = new StringBuilder();
            
            int serverCount = 0;

            foreach (ServerDetails server in this.ServerManager.GetServers())
            {
                serverCount++;
                stringBuilder.Append($"**{server.Name}**");
                stringBuilder.Append(" - ");
                stringBuilder.Append(server.Status);
                stringBuilder.AppendLine();
            }
            
            embed.AddField(new EmbedFieldBuilder()
            {
                Value = stringBuilder.ToString()
            });
            
            if (serverCount <= 0)
            {
                await this.ReplyAsync($"{this.Context.User.Mention} There are no servers! :frowning:");
            }
            else
            {
                await this.Context.Channel.SendMessageAsync("", false, embed.Build());
            }
        }
    }
}
