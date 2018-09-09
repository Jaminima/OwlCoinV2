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
            TwitchA.Settings.ClientId = "obav1xhsi7jl5dx7a2ydqhxbvc4nt1";
            TwitchA.Settings.AccessToken = "22xhm0r9g10jq06gbpbunhzfsitqvc";

            ConnectionCredentials credentials = new ConnectionCredentials("OwlCoinbot", "oauth:xtvql2bd5sl31u08u4ldjy06cpxov5");
            TwitchC = new TwitchClient();
            TwitchC.Initialize(credentials, Shared.ConfigHandler.Config["ChannelName"].ToString());
            TwitchC.OnMessageReceived += MessageHandler.HandleMessage;
            TwitchC.OnUserJoined += UserHandler.HandleUserJoin;
            TwitchC.OnUserLeft += UserHandler.HandleUserLeft;
            TwitchC.Connect();
            Console.WriteLine("TwitchBot Started");
        }

    }
}
