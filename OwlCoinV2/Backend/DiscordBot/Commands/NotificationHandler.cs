﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;
using System.Threading;

namespace OwlCoinV2.Backend.DiscordBot.Commands
{
    public static class NotificationHandler
    {
        static List<SocketGuildUser> UserList=new List<SocketGuildUser> { };
        public static async Task GetGuild(SocketGuild e)
        {
            foreach (SocketGuildUser User in e.Users.ToList()) { UserList.Add(User); }
        }

        static bool StreamLive = false;
        public static async Task LiveEvent()
        {
            System.Threading.Thread.Sleep(5000);
            while (true)
            {
                Shared.ConfigHandler.LoadConfig();
                bool IsLive = TwitchBot.Commands.Drops.IsLive();
                if (StreamLive != IsLive && UserList.Count != 0)
                {
                    StreamLive = IsLive;
                    TimeSpan TSPan = (TimeSpan)(DateTime.Now - DateTime.Parse(Shared.ConfigHandler.Config["Notifications"]["LastLive"].ToString()));
                    if (StreamLive&&(int)Math.Floor(TSPan.TotalMinutes)>=int.Parse(Shared.ConfigHandler.Config["Notifications"]["MinimumDownTime"].ToString()))
                    {
                        foreach (Newtonsoft.Json.Linq.JToken User in Shared.ConfigHandler.Config["Notifications"]["DiscordUsers"])
                        {
                            foreach (SocketGuildUser SocUser in UserList)
                            {
                                if (SocUser.Id.ToString() == User.ToString())
                                {
                                    try
                                    {
                                        Discord.IDMChannel DM = await SocUser.GetOrCreateDMChannelAsync();
                                        await DM.SendMessageAsync(Shared.ConfigHandler.Config["Notifications"]["DiscordMessage"].ToString());
                                    }
                                    catch { }
                                    break;
                                }
                            }
                        }
                    }
                }
                if (IsLive)
                {
                    Shared.ConfigHandler.Config["Notifications"]["LastLive"] = DateTime.Now.ToString();
                    Shared.ConfigHandler.SaveConfig();
                }
                System.Threading.Thread.Sleep(60000);
            }
        }
    }
}
