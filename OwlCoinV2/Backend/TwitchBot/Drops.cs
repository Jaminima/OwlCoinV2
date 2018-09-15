using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net;
using System.IO;

namespace OwlCoinV2.Backend.TwitchBot
{
    public static class Drops
    {
        public static List<String> RaffleParticipant = new List<string> { };
        public static void Handler()
        {
            int RaffleLastRunMin = -1;
            int WatchingGiveOCLastRunMin = -1;
            while (true)
            {
                if (DateTime.Now.Minute % 15 == 0 && RaffleLastRunMin!=DateTime.Now.Minute)
                {
                    Raffles++;
                    RaffleLastRunMin = DateTime.Now.Minute;
                    if (Raffles % 4 == 0) { new Thread(() => RaffleStart(true)).Start(); } else { new Thread(() => RaffleStart(false)).Start(); }
                }

                if (DateTime.Now.Minute % 10 == 0&&WatchingGiveOCLastRunMin!=DateTime.Now.Minute)
                {
                    WatchingGiveOCLastRunMin = DateTime.Now.Minute;
                    new Thread(() => WatchingGiveOC()).Start();
                }
                System.Threading.Thread.Sleep(2000);
            }
        }
        static Random Rnd = new Random();
        static int Raffles = 0;
        public static Task RaffleStart(bool IsMega)
        {
            int PayOutAmount=1000;
            if (IsMega) { PayOutAmount = 5000; }
            RaffleParticipant = new List<string> { };
            Bot.TwitchC.SendMessage(Shared.ConfigHandler.Config["ChannelName"].ToString(), "/me PogChamp a Raffle has begun for "+PayOutAmount+" Owlcoin PogChamp it will end in 60 Seconds. Enter by typing \"oc!join\" OpieOP");
            System.Threading.Thread.Sleep(15000);
            Bot.TwitchC.SendMessage(Shared.ConfigHandler.Config["ChannelName"].ToString(), "/me The Raffle for " + PayOutAmount + " Owlcoin will end in 45 Seconds. Enter by typing \"oc!join\" FeelsGoodMan");
            System.Threading.Thread.Sleep(15000);
            Bot.TwitchC.SendMessage(Shared.ConfigHandler.Config["ChannelName"].ToString(), "/me The Raffle for " + PayOutAmount + " Owlcoin will end in 30 Seconds. Enter by typing \"oc!join\" FeelsGoodMan");
            System.Threading.Thread.Sleep(15000);
            Bot.TwitchC.SendMessage(Shared.ConfigHandler.Config["ChannelName"].ToString(), "/me The Raffle for " + PayOutAmount + " Owlcoin will end in 15 Seconds. Enter by typing \"oc!join\" FeelsGoodMan");
            System.Threading.Thread.Sleep(15000);
            if (RaffleParticipant.Count == 0)
            {
                Bot.TwitchC.SendMessage(Shared.ConfigHandler.Config["ChannelName"].ToString(), "No one entered the raffle LUL");
            }
            else if (!IsMega||RaffleParticipant.Count==1)
            {
                string Winner = RaffleParticipant[Rnd.Next(0, RaffleParticipant.Count - 1)];
                PayOut(Winner, PayOutAmount);
            }
            else
            {
                int UsersCount = Rnd.Next(1,2);
                string Winner="";
                List<String> Winners = new List<string> { "" };
                for (int i = 0; i < UsersCount; i++)
                {
                    while (Winners.Contains(Winner))
                    { Winner = RaffleParticipant[Rnd.Next(0, RaffleParticipant.Count - 1)]; }
                    Winners.Add(Winner);
                    PayOut(Winner, (int)Math.Floor((decimal)PayOutAmount / UsersCount));
                }
            }
            
            return Task.CompletedTask;
        }

        public static void PayOut(string Winner,int Amount)
        {
            Shared.Data.Accounts.GiveUser(Winner, Shared.IDType.Twitch, Amount);
            Bot.TwitchC.SendMessage(Shared.ConfigHandler.Config["ChannelName"].ToString(), "The Raffle has ended and @" + UserHandler.UserFromUserID(Winner).Name + " won " + Amount + " Owlcoin FeelsGoodMan");
        }

        public static void WatchingGiveOC()
        {
            string ChannelID = UserHandler.UserFromUsername(Shared.ConfigHandler.Config["ChannelName"].ToString()).Matches[0].Id;
            List<String> Subs = GetSubs();
            foreach (string UserId in GetWatching())
            {
                if (Subs.Contains(UserId)) { Shared.Data.Accounts.GiveUser(UserId, Shared.IDType.Twitch, 300); }
                else { Shared.Data.Accounts.GiveUser(UserId, Shared.IDType.Twitch, 100); }
            }
        }

        public static List<String> GetWatching()
        {
            List<String> Watcher = new List<string> { };
            WebRequest Req = WebRequest.Create("http://tmi.twitch.tv/group/user/"+ Shared.ConfigHandler.Config["ChannelName"] + "/chatters");
            Req.Method = "GET";
            WebResponse Res = Req.GetResponse();
            string D = new StreamReader(Res.GetResponseStream()).ReadToEnd();
            Newtonsoft.Json.Linq.JObject DJ = Newtonsoft.Json.Linq.JObject.Parse(D);
            foreach(string Username in DJ["chatters"]["moderators"]) { try { Watcher.Add(UserHandler.UserFromUsername(Username).Matches[0].Id); } catch { } }
            foreach (string UserName in DJ["chatters"]["viewers"]) { try { Watcher.Add(UserHandler.UserFromUsername(UserName).Matches[0].Id); } catch { } }
            return Watcher;
        }

        public static List<String> GetSubs()
        {
            List<String> Subs = new List<string> { };
            int Offset = 0;
            while (Subs.Count % 25 == 0)
            {
                WebRequest Req = WebRequest.Create("https://api.twitch.tv/kraken/channels/" + Shared.ConfigHandler.Config["ChannelName"] + "/subscriptions?offset="+Offset);
                Req.Method = "GET";
                Req.Headers.Add("Client-ID", Shared.ConfigHandler.Config["TwitchBot"]["ClientId"].ToString());
                Req.Headers.Add("Authorization", "OAuth " + Shared.ConfigHandler.Config["TwitchBot"]["AccessToken"].ToString());
                WebResponse Res = Req.GetResponse();
                string D = new StreamReader(Res.GetResponseStream()).ReadToEnd();
                foreach (Newtonsoft.Json.Linq.JToken Item in Newtonsoft.Json.Linq.JObject.Parse(D)["subscriptions"])
                {
                    Subs.Add(Item["user"]["_id"].ToString());
                }
                Offset += 25;
            }
            return Subs;
        }

    }
}
