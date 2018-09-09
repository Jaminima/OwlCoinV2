using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwitchLib.Client;
using TwitchLib.Client.Models;
using System.Threading;

namespace OwlCoinV2.Backend.Twitch
{
    public static class Bot
    {
        public static TwitchClient TwitchC;
        static Thread TwitchThread;
        public static void Start()
        {
            TwitchThread = new Thread(()=>StartUp());
            TwitchThread.Start();
        }

        public static void StartUp()
        {
            ConnectionCredentials credentials = new ConnectionCredentials("OwlCoinbot", "oauth:xtvql2bd5sl31u08u4ldjy06cpxov5");
            TwitchC = new TwitchClient();
            TwitchC.Initialize(credentials, Shared.ConfigHandler.Config["ChannelName"].ToString());
            TwitchC.OnMessageReceived += MessageHandler.HandleMessage;
            TwitchC.Connect();
            Console.WriteLine("TwitchBot Started");
        }

    }
}
