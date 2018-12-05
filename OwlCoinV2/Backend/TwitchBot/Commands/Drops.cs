using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net;
using System.IO;

namespace OwlCoinV2.Backend.TwitchBot.Commands
{
    public static class Drops
    {
        public static List<String> RaffleParticipant = new List<string> { };
        public static void Handler()
        {
            int RaffleLastRunMin = -1;
            int WatchingGiveOCLastRunMin = -1;
            int DonationLastRunMin = -1;
            while (true)
            {
                if (DateTime.Now.Minute % 15 == 0 && RaffleLastRunMin!=DateTime.Now.Minute && IsLive())
                {
                    Raffles++;
                    RaffleLastRunMin = DateTime.Now.Minute;
                    if (Raffles % 4 == 0) { new Thread(() => RaffleStart(true)).Start(); } else { new Thread(() => RaffleStart(false)).Start(); }
                }
                if (DateTime.Now.Minute != DonationLastRunMin)
                {
                    Shared.APIIntergrations.Streamlabs.Donations.CheckForNewDonation();
                    DonationLastRunMin = DateTime.Now.Minute;
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
            try
            {
                MessageHandler.SendMessage(Shared.ConfigHandler.Config["ChannelName"].ToString(),Shared.ConfigHandler.Config["EventMessages"]["Raffle"]["Start"].ToString(), null, null,PayOutAmount);
                System.Threading.Thread.Sleep(15000);
                MessageHandler.SendMessage(Shared.ConfigHandler.Config["ChannelName"].ToString(), Shared.ConfigHandler.Config["EventMessages"]["Raffle"]["45Sec"].ToString(), null, null, PayOutAmount);
                System.Threading.Thread.Sleep(15000);
                MessageHandler.SendMessage(Shared.ConfigHandler.Config["ChannelName"].ToString(), Shared.ConfigHandler.Config["EventMessages"]["Raffle"]["30Sec"].ToString(), null, null, PayOutAmount);
                System.Threading.Thread.Sleep(15000);
                MessageHandler.SendMessage(Shared.ConfigHandler.Config["ChannelName"].ToString(), Shared.ConfigHandler.Config["EventMessages"]["Raffle"]["15Sec"].ToString(), null, null, PayOutAmount);
                System.Threading.Thread.Sleep(15000);
            }
            catch (Exception E) { Console.WriteLine(E); return null; }
            if (RaffleParticipant.Count == 0)
            {
                MessageHandler.SendMessage(Shared.ConfigHandler.Config["ChannelName"].ToString(), Shared.ConfigHandler.Config["EventMessages"]["Raffle"]["NoEntry"].ToString(), null, null, PayOutAmount);
            }
            else if (!IsMega||RaffleParticipant.Count==1)
            {
                string Winner = RaffleParticipant[Rnd.Next(0, RaffleParticipant.Count - 1)];
                PayOut(Winner, PayOutAmount);
            }
            else
            {
                int UsersCount = Rnd.Next(1,2+1);
                string Winner="";
                List<String> Winners = new List<string> { "" };
                for (int i = 0; i < UsersCount; i++)
                {
                    while (Winners.Contains(Winner))
                    { Winner = RaffleParticipant[Rnd.Next(0, RaffleParticipant.Count)]; }
                    Winners.Add(Winner);
                    PayOut(Winner, (int)Math.Floor((decimal)PayOutAmount / UsersCount));
                }
            }
            
            return Task.CompletedTask;
        }

        public static void PayOut(string Winner,int Amount)
        {
            Shared.Data.Accounts.GiveUser(Winner, Shared.IDType.Twitch, Amount);
            MessageHandler.SendMessage(Shared.ConfigHandler.Config["ChannelName"].ToString(), Shared.ConfigHandler.Config["EventMessages"]["Raffle"]["Win"].ToString(), null, UserHandler.UserFromUserID(Winner).Name, Amount);
        }

        public static void WatchingGiveOC()
        {
            string ChannelID = UserHandler.UserFromUsername(Shared.ConfigHandler.Config["ChannelName"].ToString()).Matches[0].Id;
            if (!IsLive()) { return; }
            List<String> Subs = GetSubs();
            foreach (string UserId in GetWatching())
            {
                if (!Shared.ConfigHandler.Config["Rewards"]["Twitch"]["Exceptions"].Contains(UserId))
                {
                    if (Subs.Contains(UserId)) { Shared.Data.Accounts.GiveUser(UserId, Shared.IDType.Twitch, int.Parse(Shared.ConfigHandler.Config["Rewards"]["Twitch"]["Watching"]["Subscriber"].ToString())); }
                    else { Shared.Data.Accounts.GiveUser(UserId, Shared.IDType.Twitch, int.Parse(Shared.ConfigHandler.Config["Rewards"]["Twitch"]["Watching"]["Viewer"].ToString())); }
                }
            }
        }

        public static List<String> GetWatching()
        {
            try
            {
                List<String> Watcher = new List<string> { };
                WebRequest Req = WebRequest.Create("http://tmi.twitch.tv/group/user/" + Shared.ConfigHandler.Config["ChannelName"] + "/chatters");
                Req.Method = "GET";
                WebResponse Res = Req.GetResponse();
                string D = new StreamReader(Res.GetResponseStream()).ReadToEnd();
                Newtonsoft.Json.Linq.JObject DJ = Newtonsoft.Json.Linq.JObject.Parse(D);
                foreach (string Username in DJ["chatters"]["moderators"]) { try { Watcher.Add(UserHandler.UserFromUsername(Username).Matches[0].Id); } catch { } }
                foreach (string UserName in DJ["chatters"]["viewers"]) { try { Watcher.Add(UserHandler.UserFromUsername(UserName).Matches[0].Id); } catch { } }
                return Watcher;
            }catch (Exception E) { Console.WriteLine(E); return GetWatching(); }
        }

        public static List<String> GetSubs()
        {
            List<String> Subs = new List<string> { };
            int Offset = 0;
            while (Subs.Count % 25 == 0)
            {
                string D="";
                while (D == "") { try { D = GetSubData(Offset); } catch (Exception E) { Console.WriteLine(E); D = GetSubData(Offset); } }
                foreach (Newtonsoft.Json.Linq.JToken Item in Newtonsoft.Json.Linq.JObject.Parse(D)["subscriptions"])
                {
                    Subs.Add(Item["user"]["_id"].ToString());
                }
                Offset += 25;
            }
            return Subs;
        }

        static string GetSubData(int Offset)
        {
            WebRequest Req = WebRequest.Create("https://api.twitch.tv/kraken/channels/" + Shared.ConfigHandler.Config["ChannelName"] + "/subscriptions?offset=" + Offset);
            Req.Method = "GET";
            Req.Headers.Add("Client-ID", Shared.ConfigHandler.LoginConfig["TwitchBot"]["ClientId"].ToString());
            Req.Headers.Add("Authorization", "OAuth " + Shared.ConfigHandler.LoginConfig["TwitchBot"]["AccessToken"].ToString());
            WebResponse Res = Req.GetResponse();
            string D = new StreamReader(Res.GetResponseStream()).ReadToEnd();
            return D;
        }

        public static bool IsLive()
        {
            try
            {
                WebRequest Req = WebRequest.Create("https://api.twitch.tv/helix/streams?user_login=" + Shared.ConfigHandler.Config["ChannelName"]);
                Req.Method = "GET";
                Req.Headers.Add("Client-ID", Shared.ConfigHandler.LoginConfig["TwitchBot"]["ClientId"].ToString());
                Req.Headers.Add("Authorization", "OAuth " + Shared.ConfigHandler.LoginConfig["TwitchBot"]["AccessToken"].ToString());
                WebResponse Res = Req.GetResponse();
                string D = new StreamReader(Res.GetResponseStream()).ReadToEnd();
                Newtonsoft.Json.Linq.JObject JD = Newtonsoft.Json.Linq.JObject.Parse(D);
                if (JD["data"].Count() != 0)
                {
                    if (JD["data"][0]["type"].ToString() == "live")
                    {
                        return true;
                    }
                }
                return false;
            }
            catch (Exception E) {
                Console.WriteLine(E);
                return false;
            }
        }

    }
}
