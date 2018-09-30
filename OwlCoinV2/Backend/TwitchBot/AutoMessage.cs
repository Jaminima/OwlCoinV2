using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace OwlCoinV2.Backend.TwitchBot
{
    public static class AutoMessage
    {
        public static void Start()
        {
            int LoopStartDelay = int.Parse(Shared.ConfigHandler.Config["AutoMessages"]["LoopStartDelay"].ToString());
            new Thread(() => StartLoop(LoopStartDelay)).Start();
        }

        static void StartLoop(int StartupDealy)
        {
            foreach (Newtonsoft.Json.Linq.JObject Message in Shared.ConfigHandler.Config["AutoMessages"]["Messages"])
            {
                new Thread(() => MessageLoop(Message["Text"].ToString(), int.Parse(Message["Delay"].ToString()))).Start();
                System.Threading.Thread.Sleep(StartupDealy * 60000);
            }
        }

        static void MessageLoop(string Message,int Delay)
        {
            while (true)
            {
                if (Commands.Drops.IsLive())
                { MessageHandler.SendMessage(Shared.ConfigHandler.Config["ChannelName"].ToString(), Message, null); }
                System.Threading.Thread.Sleep(Delay * 60000);
            }
        }
    }
}
