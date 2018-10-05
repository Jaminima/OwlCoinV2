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
            int Items = Shared.ConfigHandler.Config["AutoMessages"]["Messages"].Count();
            new Thread(() => StartLoop(LoopStartDelay,Items)).Start();
        }

        static void StartLoop(int StartupDealy,int Items)
        {
            for (int i=0;i<Items;i++)
            {
                new Thread(() => MessageLoop(i)).Start();
                System.Threading.Thread.Sleep(StartupDealy * 60000);
            }
        }

        static void MessageLoop(int i)
        {
            while (true)
            {
                if (Commands.Drops.IsLive())
                { MessageHandler.SendMessage(Shared.ConfigHandler.Config["ChannelName"].ToString(), Shared.ConfigHandler.Config["AutoMessages"]["Messages"][i]["Text"].ToString(), null,null); }
                System.Threading.Thread.Sleep(int.Parse(Shared.ConfigHandler.Config["AutoMessages"]["Messages"][i]["Delay"].ToString()) * 60000);
            }
        }
    }
}
