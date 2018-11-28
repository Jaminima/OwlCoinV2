using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;

namespace OwlCoinV2.Backend.Shared
{
    public static class WebRequests
    {
        public static Newtonsoft.Json.Linq.JToken GET(string URL)
        {
            if (CachedObjectExists(URL, null, null)) { return GetCachedResponse(URL, null, null); }
            WebRequest Request = WebRequest.Create(ConfigHandler.LoginConfig["API"]["URL"].ToString() + ":" + ConfigHandler.LoginConfig["API"]["Port"] + URL);
            Request.Method = "GET";
            WebResponse Response = Request.GetResponse();
            string D = new StreamReader(Response.GetResponseStream()).ReadToEnd();
            Newtonsoft.Json.Linq.JToken JD = Newtonsoft.Json.Linq.JToken.Parse(D);
            CacheResponse(URL, null, null, JD);
            return JD;
        }
        
        public static Newtonsoft.Json.Linq.JToken POST(string URL, Dictionary<string, string> Headers = null,string sPostData="")
        { return POST(URL, Headers, sPostData, true); }

        public static Newtonsoft.Json.Linq.JToken POST(string URL, Dictionary<string, string> Headers, string sPostData,bool AuthToken = false,bool Caching=true)
        {
            if (Caching) { if (CachedObjectExists(URL, Headers, sPostData)) { return GetCachedResponse(URL, Headers, sPostData); } }
            WebRequest Request = WebRequest.Create(ConfigHandler.LoginConfig["API"]["URL"].ToString() + ":" + ConfigHandler.LoginConfig["API"]["Port"] + URL);
            Request.Method = "POST";
            byte[] PostData = Encoding.UTF8.GetBytes(sPostData);
            if (Headers != null) { foreach (KeyValuePair<string, string> Pair in Headers) { Request.Headers.Add(Pair.Key, Pair.Value); } }
            if (AuthToken) { Request.Headers.Add("AuthorizationToken", GetAuthToken()); }
            Request.ContentLength = PostData.Length;
            Stream PostStream = Request.GetRequestStream();
            PostStream.Write(PostData, 0, PostData.Length);
            PostStream.Flush();
            PostStream.Close();
            WebResponse Response = Request.GetResponse();
            string D = new StreamReader(Response.GetResponseStream()).ReadToEnd();
            Newtonsoft.Json.Linq.JToken JD = Newtonsoft.Json.Linq.JToken.Parse(D);
            if (Caching) { CacheResponse(URL, Headers, sPostData, JD); }
            return JD;
        }

        public static string GetAuthToken()
        {
            Dictionary<string, string> Headers = new Dictionary<string, string> { };
            Headers.Add("RefreshToken", ConfigHandler.LoginConfig["API"]["RefreshToken"].ToString());
            Newtonsoft.Json.Linq.JToken AuthData = POST("/auth/token",Headers,"",false,false);
            if (AuthData["Status"].ToString() != "200") { Console.WriteLine("RefreshToken Invalid!!!!!!!"); return null; }
            ConfigHandler.LoginConfig["API"]["RefreshToken"] = AuthData["Data"]["RefreshToken"].ToString();
            ConfigHandler.SaveConfig();
            return AuthData["Data"]["AuthorizationToken"].ToString();
        }

        static List<CachedResponse> CachedResponses = new List<CachedResponse> { };
        static void CacheResponse(string URL,Dictionary<string,string> Headers,string sPostData,Newtonsoft.Json.Linq.JToken Response)
        {
            if ((Headers != null || sPostData != null) && !CompatiblePostURLs.Contains(URL)) { return; }
            foreach (CachedResponse Resp in CachedResponses) { if (ObjectMatches(URL, Headers, sPostData, Resp)) { CachedResponses.Remove(Resp); break; } }
            CachedResponses.Add(new CachedResponse(URL, sPostData, Headers,Response));
        }

        static Newtonsoft.Json.Linq.JToken GetCachedResponse(string URL, Dictionary<string, string> Headers, string sPostData)
        {
            if (CachedObjectExists(URL, Headers, sPostData))
            {
                foreach (CachedResponse Resp in CachedResponses) { if (ObjectMatches(URL, Headers, sPostData, Resp)) { return Resp.Response; } }
            }
            return null;
        }

        static List<string> CompatiblePostURLs = new List<string> { "/user" };
        static bool CachedObjectExists(string URL, Dictionary<string, string> Headers, string sPostData)
        {
            if ((Headers != null || sPostData != null) && !CompatiblePostURLs.Contains(URL)) { return false; }
            foreach (CachedResponse Resp in CachedResponses) { if (ObjectMatches(URL,Headers,sPostData,Resp)&&(DateTime.Now-Resp.DateTime).TotalMilliseconds<2000) { return true; } }
            return false;
        }

        static bool ObjectMatches(string URL,Dictionary<string,string> Headers,string sPostData,CachedResponse CachedResponse)
        {
            if (URL != CachedResponse.URL) return false;
            if (sPostData != CachedResponse.sPostData) return false;
            foreach (KeyValuePair<string,string> KeyPair in Headers)
            {
                if (!CachedResponse.Headers.Keys.Contains(KeyPair.Key)) return false;
                if (CachedResponse.Headers[KeyPair.Key] != KeyPair.Value) return false;
            }
            return true;
        }

    }

    class CachedResponse
    {
        public string URL, sPostData;
        public Dictionary<string, string> Headers;
        public Newtonsoft.Json.Linq.JToken Response;
        public DateTime DateTime;

        public CachedResponse(string url,string PostData,Dictionary<string,string> Header,Newtonsoft.Json.Linq.JToken response)
        {
            URL = url;sPostData = PostData;Headers = Header;Response = response;
            DateTime = DateTime.Now;
        }
    }
}
