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

        public static Newtonsoft.Json.Linq.JToken RequestSong(string Url)
        {
            WebRequest Req = WebRequest.Create("https://api.nightbot.tv/1/song_requests/queue");
            Req.Method = "POST";
            Req.Headers.Add("Authorization", "Bearer " + GetAuthToken());
            byte[] PostData = Encoding.UTF8.GetBytes("q="+Url);
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
                return JD;
            }
            catch (WebException E)
            {
                return Newtonsoft.Json.Linq.JToken.Parse(new StreamReader(E.Response.GetResponseStream()).ReadToEnd());
            }
        }

    }
}
