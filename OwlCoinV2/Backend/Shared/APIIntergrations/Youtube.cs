using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;

namespace OwlCoinV2.Backend.Shared.APIIntergrations
{
    public static class Youtube
    {
        public static string LatestVid()
        {
            WebRequest Req = WebRequest.Create("https://www.googleapis.com/youtube/v3/search?key="+ConfigHandler.LoginConfig["Youtube"]["AuthToken"].ToString()
                +"&channelId="+ ConfigHandler.LoginConfig["Youtube"]["ChannelID"].ToString()
                + "&part=snippet,id&order=date&maxResults=1");
            Req.Method = "GET";
            WebResponse Res = Req.GetResponse();
            string SData = new StreamReader(Res.GetResponseStream()).ReadToEnd();
            Newtonsoft.Json.Linq.JToken Resp = Newtonsoft.Json.Linq.JToken.Parse(SData);
            return "https://youtu.be/"+Resp["items"][0]["id"]["videoId"].ToString();
        }
    }
}
