using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;

namespace OwlCoinV2.Backend.TwitchBot.Nightbot
{
    public static class Requests
    {
        public static string GetAuthToken()
        {
            Shared.ConfigHandler.LoadConfig();
            WebRequest Req = WebRequest.Create("https://api.nightbot.tv/oauth2/token");
            byte[] PostData = Encoding.UTF8.GetBytes("client_id=" + Shared.ConfigHandler.Config["NightBot"]["ClientId"] +
                "&client_secret=" + Shared.ConfigHandler.Config["NightBot"]["ClientSecret"] +
                "&grant_type=refresh_token&redirect_uri=https://www.twitch.tv/harbonator&refresh_token=" + Shared.ConfigHandler.Config["NightBot"]["RefreshToken"]);
            Req.Method = "POST";
            Req.ContentType = "application/x-www-form-urlencoded";
            Req.ContentLength = PostData.Length;
            Stream PostStream = Req.GetRequestStream();
            PostStream.Write(PostData, 0, PostData.Length);
            PostStream.Flush();
            PostStream.Close();
            try
            {
                WebResponse Res = Req.GetResponse();
                string D = new StreamReader(Res.GetResponseStream()).ReadToEnd();
                Newtonsoft.Json.Linq.JObject JD = Newtonsoft.Json.Linq.JObject.Parse(D);
                Shared.ConfigHandler.Config["NightBot"]["RefreshToken"] = JD["refresh_token"];
                Shared.ConfigHandler.SaveConfig();
                return JD["access_token"].ToString();
            }
            catch (WebException E)
            {
                Console.WriteLine(new StreamReader(E.Response.GetResponseStream()).ReadToEnd());
                return GetAuthToken();
            }
        }

        public static Newtonsoft.Json.Linq.JToken GenericExecute(string URL,string Method)
        {
            return GenericExecute(URL, "",Method);
        }

        public static Newtonsoft.Json.Linq.JToken GenericExecute(string URL,string Data,string Method)
        {
            WebRequest Req = WebRequest.Create(URL);
            Req.Method = Method;
            Req.Headers.Add("Authorization", "Bearer " + GetAuthToken());
            Req.ContentType = "application/x-www-form-urlencoded";
            if (Data != "")
            {
                byte[] PostData = Encoding.UTF8.GetBytes(Data);
                Req.ContentLength = PostData.Length;
                Stream PostStream = Req.GetRequestStream();
                PostStream.Write(PostData, 0, PostData.Length);
                PostStream.Flush();
                PostStream.Close();
            }
            try
            {
                WebResponse Res = Req.GetResponse();
                string D = new StreamReader(Res.GetResponseStream()).ReadToEnd();
                Newtonsoft.Json.Linq.JObject JD = Newtonsoft.Json.Linq.JObject.Parse(D);
                return JD;
            }
            catch (WebException E)
            {
                return Newtonsoft.Json.Linq.JToken.Parse(new StreamReader(E.Response.GetResponseStream()).ReadToEnd());
            }
        }
        static int PrevVolume = 10;
        public static Newtonsoft.Json.Linq.JToken PauseSong()
        {
            PrevVolume = int.Parse(GenericExecute("https://api.nightbot.tv/1/song_requests", "GET")["settings"]["volume"].ToString());
            return GenericExecute("https://api.nightbot.tv/1/song_requests", "volume=0","PUT");
        }

        public static Newtonsoft.Json.Linq.JToken PlaySong()
        {
            return GenericExecute("https://api.nightbot.tv/1/song_requests", "volume="+PrevVolume,"PUT");
        }

        public static Newtonsoft.Json.Linq.JToken SkipSong()
        {
            return GenericExecute("https://api.nightbot.tv/1/song_requests/queue/skip","POST");
        }

        public static Newtonsoft.Json.Linq.JToken SetVolume(int Volume)
        {
            return GenericExecute("https://api.nightbot.tv/1/song_requests", "volume=" + Volume, "PUT");
        }

        public static Newtonsoft.Json.Linq.JToken RequestSong(string Url)
        {
            return GenericExecute("https://api.nightbot.tv/1/song_requests/queue", "q=" + Url,"POST");
        }

    }
}
