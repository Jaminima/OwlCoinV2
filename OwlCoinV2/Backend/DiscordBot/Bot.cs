using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Discord.WebSocket;

namespace OwlCoinV2.Backend.DiscordBot
{
    public static class Bot
    {
        public static DiscordSocketClient DiscordClient;

        public async static void Start()
        {
            DiscordSocketConfig SocketConfig = new DiscordSocketConfig();
            SocketConfig.AlwaysDownloadUsers = true;
            DiscordClient = new DiscordSocketClient(SocketConfig);
            DiscordClient.MessageReceived += MessageHandler.HandleMessage;
            DiscordClient.GuildAvailable += Commands.NotificationHandler.GetGuild;
            await DiscordClient.LoginAsync(Discord.TokenType.Bot, Shared.ConfigHandler.LoginConfig["DiscordBot"]["Token"].ToString());
            await DiscordClient.StartAsync();
            await DiscordClient.SetGameAsync("!help");
            Console.WriteLine("DiscordBot Started");
            new Thread(() => Duels.Handler()).Start();
            new Thread(async () => await Commands.NotificationHandler.LiveEvent()).Start();
        }

    }
}
