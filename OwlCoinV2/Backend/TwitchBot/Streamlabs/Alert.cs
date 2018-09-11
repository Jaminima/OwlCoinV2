using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;

namespace OwlCoinV2.Backend.TwitchBot.Streamlabs
{
    public static class Alert
    {
        public static bool SendRequest(string ImageUrl, string SoundUrl)
        {
            WebRequest Req = WebRequest.Create("https://streamlabs.com/api/v1.0/alerts");
            Req.Method = "POST";
            Req.ContentType = "application/x-www-form-urlencoded";
            byte[] PostData = Encoding.UTF8.GetBytes("access_token=" + GetAuthCode() +
                "&type=donation&message=Wow an amazing meme!&image_href=" + ImageUrl +
                "&sound_href=" + SoundUrl);
            //byte[] PostData = Encoding.UTF8.GetBytes("access_token=" + Shared.ConfigHandler.Config["StreamLabs"]["access_token"].ToString() + "&type=donation&image_href=" + ImageUrl + "&sound_href=" + SoundUrl);
            Req.ContentLength = PostData.Length;
            Stream PostStream = Req.GetRequestStream();
            PostStream.Write(PostData, 0, PostData.Length);
            PostStream.Flush();
            PostStream.Close();
            WebResponse Res;
            try
            {
                Res = Req.GetResponse();
                //Console.WriteLine(new StreamReader(Res.GetResponseStream()).ReadToEnd());
                return true;
            }
            catch (WebException E)
            {
                Console.WriteLine(new StreamReader(E.Response.GetResponseStream()).ReadToEnd());
            }
            return false;
        }

        public static string GetAuthCode()
        {
            WebRequest Req = WebRequest.Create("https://streamlabs.com/api/v1.0/token");
            Req.Method = "POST";
            Req.ContentType = "application/x-www-form-urlencoded";
            byte[] PostData = Encoding.UTF8.GetBytes("grant_type=refresh_token&client_id="+ Shared.ConfigHandler.Config["StreamLabs"]["ClientId"] +
                "&client_secret="+ Shared.ConfigHandler.Config["StreamLabs"]["ClientSecret"] +
                "&redirect_uri=https://www.google.co.uk/&refresh_token=" +Shared.ConfigHandler.Config["StreamLabs"]["RefreshToken"]);
            Req.ContentLength = PostData.Length;
            Stream PostStream = Req.GetRequestStream();
            PostStream.Write(PostData, 0, PostData.Length);
            PostStream.Flush();
            PostStream.Close();
            WebResponse Res;
            try
            {
                Res = Req.GetResponse();
                Newtonsoft.Json.Linq.JObject D=Newtonsoft.Json.Linq.JObject.Parse( new StreamReader(Res.GetResponseStream()).ReadToEnd());
                Shared.ConfigHandler.Config["StreamLabs"]["RefreshToken"] = D["refresh_token"];
                Shared.ConfigHandler.SaveConfig();
                return D["access_token"].ToString();
            }
            catch (WebException E)
            {
                Console.WriteLine(new StreamReader(E.Response.GetResponseStream()).ReadToEnd());
            }
            return "";
        }
    }
}
