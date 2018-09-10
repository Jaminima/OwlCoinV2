using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwitchLib.Client;
using TwitchLib.Client.Models;
using TwitchLib.Api;
using System.Threading;

namespace OwlCoinV2.Backend.TwitchBot
{
    public static class Bot
    {
        public static TwitchClient TwitchC;
        public static TwitchAPI TwitchA;
        static Thread TwitchThread;
        public static void Start()
        {
            TwitchThread = new Thread(()=>StartUp());
            TwitchThread.Start();
        }

        public static void StartUp()
        {
            TwitchA = new TwitchAPI();
            TwitchA.Settings.ClientId = Shared.ConfigHandler.Config["TwitchBot"]["ClientId"].ToString();
            TwitchA.Settings.AccessToken = Shared.ConfigHandler.Config["TwitchBot"]["AccessToken"].ToString();

            ConnectionCredentials credentials = new ConnectionCredentials(Shared.ConfigHandler.Config["TwitchBot"]["Username"].ToString(), Shared.ConfigHandler.Config["TwitchBot"]["0AuthToken"].ToString());
            TwitchC = new TwitchClient();
            TwitchC.Initialize(credentials, Shared.ConfigHandler.Config["ChannelName"].ToString());
            TwitchC.OnMessageReceived += MessageHandler.HandleMessage;
            TwitchC.OnMessageReceived += ChatLogger.AddToLog;
            TwitchC.Connect();
            Console.WriteLine("TwitchBot Started");
            System.Threading.Thread.Sleep(500);
            new Thread(() => Drops.Handler()).Start();
        }

    }
}
