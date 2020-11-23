using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Discord_Bot.Commands
{
    public class VerifyCommand : ModuleBase<SocketCommandContext>
    {
        [Command("help")]
        [Summary("Sends a list of available commands and their summary to user's DMs.")]
        public Task GetCommandsList()
        {
            if (!(this.Context.Channel is IDMChannel))
            {
                await this.ReplyAsync($"{Context.User.Mention} Check your DMs! :smiley:");
            }

            List<CommandInfo> commands = this.CommandService.Commands.ToList();

            EmbedBuilder embed = new EmbedBuilder();

            embed.WithTitle("Command List");
            embed.WithColor(Color.Blue);
            embed.WithAuthor(author = > { author.WithName(this.Context.User.Username + this.Context.User.Discriminator).WithIconUrl(this.Context.User.GetAvatarUrl()) });

            int commandCount = 0;

            foreach (CommandInfo command in commands)
            {   
                commandCount++;
                string embedCommandSummary = command.Summary ?? "*No description available.*\n";
                embedBuilder.AddField($"/{command.Name}", embedCommandSummary);
            }

            try
            {
                if (commandCount <= 0)
                {
                    await this.Context.Message.ModifyAsync(m => m.Content = $"{this.Context.User.Mention} I don't have any commands! :frowning:");
                }
                else
                {
                    await this.Context.User.SendMessageAsync("", false, embed.Build());
                }
            }
            catch (Discord.Net.HttpException)
            {
                await this.Context.Message.ModifyAsync(m => m.Content = $"{this.Context.User.Mention} I couldn't send you my commands. To fix this issue head to **Server Settings** > **Privacy Settings** > **Allow Direct Messages from server members**.");
            }
        }
    }
}
