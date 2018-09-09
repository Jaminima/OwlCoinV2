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
        public static void Watching()
        {
            while (true)
            {
                if (DateTime.Now.Minute % 10 == 0)
                {
                    //new Thread(async() => await GiveOC()).Start();
                    GiveOC();
                }
                System.Threading.Thread.Sleep(60000);
            }
        }

        public static  Task GiveOC()
        {
            string ChannelID = UserHandler.UserFromUsername(Shared.ConfigHandler.Config["ChannelName"].ToString()).Matches[0].Id;
            List<String> Subs = GetSubs();
            foreach (string UserId in GetWatching())
            {
                if (Subs.Contains(UserId)) { Shared.Data.Accounts.GiveUser(UserId, Shared.IDType.Twitch, 300); }
                else { Shared.Data.Accounts.GiveUser(UserId, Shared.IDType.Twitch, 100); }
            }
            return null;
        }

        public static List<String> GetWatching()
        {
            List<String> Watcher = new List<string> { };
            WebRequest Req = WebRequest.Create("http://tmi.twitch.tv/group/user/"+ Shared.ConfigHandler.Config["ChannelName"] + "/chatters");
            Req.Method = "GET";
            WebResponse Res = Req.GetResponse();
            string D = new StreamReader(Res.GetResponseStream()).ReadToEnd();
            Newtonsoft.Json.Linq.JObject DJ = Newtonsoft.Json.Linq.JObject.Parse(D);
            foreach(string Username in DJ["chatters"]["moderators"]) { Watcher.Add(UserHandler.UserFromUsername(Username).Matches[0].Id); }
            foreach (string UserName in DJ["chatters"]["viewers"]) { Watcher.Add(UserHandler.UserFromUsername(UserName).Matches[0].Id); }
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
