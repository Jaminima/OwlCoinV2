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
            WebRequest Request = WebRequest.Create(ConfigHandler.Config["API"]["URL"].ToString() + ":" + ConfigHandler.Config["API"]["Port"] + URL);
            Request.Method = "GET";
            WebResponse Response = Request.GetResponse();
            string D = new StreamReader(Response.GetResponseStream()).ReadToEnd();
            Newtonsoft.Json.Linq.JToken JD = Newtonsoft.Json.Linq.JToken.Parse(D);
            return JD;
        }

        public static Newtonsoft.Json.Linq.JToken POST(string URL, Dictionary<string, string> Headers = null,string sPostData="")
        {
            WebRequest Request = WebRequest.Create(ConfigHandler.Config["API"]["URL"].ToString() + ":" + ConfigHandler.Config["API"]["Port"] +URL);
            Request.Method = "POST";
            byte[] PostData = Encoding.UTF8.GetBytes(sPostData);
            if (Headers != null) { foreach (KeyValuePair<string, string> Pair in Headers) { Request.Headers.Add(Pair.Key, Pair.Value); } }
            Request.Headers.Add("AuthToken", ConfigHandler.Config["API"]["AuthToken"].ToString());
            Request.ContentLength = PostData.Length;
            Stream PostStream = Request.GetRequestStream();
            PostStream.Write(PostData, 0, PostData.Length);
            PostStream.Flush();
            PostStream.Close();
            WebResponse Response = Request.GetResponse();
            string D = new StreamReader(Response.GetResponseStream()).ReadToEnd();
            Newtonsoft.Json.Linq.JToken JD = Newtonsoft.Json.Linq.JToken.Parse(D);
            return JD;
        }

    }
}
