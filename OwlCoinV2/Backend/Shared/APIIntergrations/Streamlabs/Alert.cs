using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;

namespace OwlCoinV2.Backend.Shared.APIIntergrations.Streamlabs
{
    public static class Alert
    {
        public static bool SendRequest(string ImageUrl, string SoundUrl)
        {
            WebRequest Req = WebRequest.Create("https://streamlabs.com/api/v1.0/alerts");
            Req.Method = "POST";
            Req.ContentType = "application/x-www-form-urlencoded";
            byte[] PostData = Encoding.UTF8.GetBytes("access_token=" + GetAuthCode() +
                "&type=merch&duration=0&message= &image_href=https://upload.wikimedia.org/wikipedia/commons/thumb/0/0b/TransparentPlaceholder.svg/240px-TransparentPlaceholder.svg.png" /*+ ImageUrl*/ +
                "&sound_href=" + SoundUrl);
            Req.ContentLength = PostData.Length;
            Req.Timeout = 2000;
            Stream PostStream = Req.GetRequestStream();
            PostStream.Write(PostData, 0, PostData.Length);
            PostStream.Flush();
            PostStream.Close();
            WebResponse Res;
            try
            {
                Res = Req.GetResponse();
                string SData = new StreamReader(Res.GetResponseStream()).ReadToEnd();
                if (SData == null) { return false; }
                Newtonsoft.Json.Linq.JToken Data=Newtonsoft.Json.Linq.JToken.Parse(SData);
                if (Data == null) { return false; }
                //Console.WriteLine(Data);
                return true;
            }
            catch (WebException E)
            {
                Console.WriteLine(E.Message);
                return false;
            }
            return false;
        }

        public static string GetAuthCode()
        {
            WebRequest Req = WebRequest.Create("https://streamlabs.com/api/v1.0/token");
            Req.Method = "POST";
            Req.ContentType = "application/x-www-form-urlencoded";
            byte[] PostData = Encoding.UTF8.GetBytes("grant_type=refresh_token&client_id="+ Shared.ConfigHandler.LoginConfig["StreamLabs"]["ClientId"] +
                "&client_secret="+ Shared.ConfigHandler.LoginConfig["StreamLabs"]["ClientSecret"] +
                "&redirect_uri=https://www.google.co.uk/&refresh_token=" +Shared.ConfigHandler.LoginConfig["StreamLabs"]["RefreshToken"]);
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
                Shared.ConfigHandler.LoginConfig["StreamLabs"]["RefreshToken"] = D["refresh_token"];
                Shared.ConfigHandler.SaveConfig();
                return D["access_token"].ToString();
            }
            catch (WebException E)
            {
                Console.WriteLine(new StreamReader(E.Response.GetResponseStream()).ReadToEnd());
                return GetAuthCode();
            }
        }
    }
}
