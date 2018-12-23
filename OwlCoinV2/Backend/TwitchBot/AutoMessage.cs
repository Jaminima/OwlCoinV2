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
        static Dictionary<int, DateTime> MessageHistory = new Dictionary<int, DateTime> { };
        static DateTime LastMessage;
        public static void MessageSender()
        {
            if (Commands.Drops.IsLive())
            {
                Newtonsoft.Json.Linq.JToken MessageSet = Shared.ConfigHandler.Config["AutoMessages"]["Messages"];
                int MinDelay = int.Parse(Shared.ConfigHandler.Config["AutoMessages"]["MinDelay"].ToString());
                if (((TimeSpan)(DateTime.Now - LastMessage)).TotalSeconds > MinDelay)
                {
                    for (int i = 0; i < MessageSet.Count(); i++)
                    {
                        if (MessageHistory.ContainsKey(i))
                        {
                            int Delay = int.Parse(MessageSet[i]["Delay"].ToString());
                            if (((TimeSpan)(DateTime.Now - MessageHistory[i])).TotalMinutes > Delay)
                            {
                                MessageHistory[i] = DateTime.Now;
                                MessageHandler.SendMessage(Shared.ConfigHandler.Config["ChannelName"].ToString(), MessageSet[i]["Text"].ToString(), null, null);
                                LastMessage = DateTime.Now;
                                break;
                            }
                        }
                        else
                        {
                            MessageHistory.Add(i, DateTime.Now);
                            MessageHandler.SendMessage(Shared.ConfigHandler.Config["ChannelName"].ToString(), MessageSet[i]["Text"].ToString(), null, null);
                            LastMessage = DateTime.Now;
                            break;
                        }
                    }
                }
            }
        }
    }
}
