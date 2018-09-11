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
        static DiscordSocketClient DiscordClient;

        public async static void Start()
        {
            DiscordClient = new DiscordSocketClient();
            DiscordClient.MessageReceived += MessageHandler.HandleMessage;
            await DiscordClient.LoginAsync(Discord.TokenType.Bot, Shared.ConfigHandler.Config["DiscordBot"]["Token"].ToString());
            await DiscordClient.StartAsync();
            Console.WriteLine("DiscordBot Started");
            new Thread(() => Duels.Handler()).Start();
        }

    }
}
