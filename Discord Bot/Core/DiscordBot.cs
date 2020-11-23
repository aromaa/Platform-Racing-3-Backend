using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Discord_Bot.Config;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Platform_Racing_3_Common.Redis;
using Platform_Racing_3_Common.Server;
using StackExchange.Redis;

namespace Discord_Bot.Core
{
    internal class DiscordBot
    {
        private DiscordBotConfig Config;

        private CommandService CommandService;

        private DiscordSocketClient Client;

        private IServiceProvider Services;

        internal DiscordBot(DiscordBotConfig config)
        {
            this.Config = config;

            this.CommandService = new CommandService();

            this.Client = new DiscordSocketClient();
            this.Client.MessageReceived += this.HandleIncomingMessage;
        }

        internal async Task LoadCommandsAsync()
        {
            await this.CommandService.AddModulesAsync(assembly: Assembly.GetEntryAssembly(), this.BuildServeProvider());
        }

        private IServiceProvider BuildServeProvider()
        {
            if (this.Services == null)
            {
                ServerManager serverManager = new ServerManager();
                serverManager.LoadServersAsync().GetAwaiter().GetResult();

                this.Services = new ServiceCollection().AddSingleton(serverManager).BuildServiceProvider();
            }

            return this.Services;
        }

        internal async Task SetupDiscordBot()
        {
            try
            {
                await this.Client.LoginAsync(TokenType.Bot, this.Config.BotToken);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Could not login: {e}");
            }

            await this.Client.StartAsync();
            await this.Client.SetGameAsync("pr3hub.com | /help");
        }

        private async Task HandleIncomingMessage(SocketMessage message)
        {
            if (message is SocketUserMessage userMessage)
            {
                if (userMessage.Author.IsBot)
                {
                    return;
                }

                int commandIndex = 0;

                if (!userMessage.HasCharPrefix('/', ref commandIndex) && !userMessage.HasMentionPrefix(this.Client.CurrentUser, ref commandIndex))
                {
                    return;
                }

                SocketCommandContext context = new SocketCommandContext(this.Client, userMessage);

                await this.CommandService.ExecuteAsync(context, commandIndex, this.Services);
            }
        }
    }
}
