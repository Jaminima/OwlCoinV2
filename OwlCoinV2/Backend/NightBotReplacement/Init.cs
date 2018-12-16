using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;

namespace OwlCoinV2.Backend.NightBotReplacement
{
    public static class Init
    {
        public static Queue Queue = new Queue();
        public static PlayerState PlayerState = PlayerState.Playing;

        public static Newtonsoft.Json.Linq.JToken GetSong()
        {
            if (Queue.SongQueue.Count != 0)
            {
                return Queue.SongQueue[0].ToJson();
            }
            Song Default = new Song();
            Default.YoutubeID = "Xr9Oubxw1gA";
            return Default.ToJson();
        }
        
        public static Newtonsoft.Json.Linq.JToken GetQueue()
        {
            return Queue.ToJson();
        }

        public static Newtonsoft.Json.Linq.JToken GetState()
        {
            return Newtonsoft.Json.Linq.JToken.Parse("{'State':'"+PlayerState.ToString()+"'}");
        }

        public static bool Enqueue(string URLName,string RequesterTwitchID)
        {
            if (URLName.StartsWith("https://www.youtube.com/watch?v="))
            {
                string YoutubeID = GetYoutubeID(URLName);
                if (YoutubeVideoExists(YoutubeID))
                {
                    Song NewSong = new Song();
                    NewSong.RequesterTwitchID = RequesterTwitchID;
                    NewSong.YoutubeID = YoutubeID;
                    Queue.SongQueue.Add(NewSong);
                    return true;
                }
            }
            return false;
        }

        public static bool Dequeue(int ID)
        {
            if (Queue.SongQueue.Count>ID)
            {
                Queue.SongQueue.RemoveAt(ID);
                return true;
            }
            return false;
        }

        static bool YoutubeVideoExists(string VideoCode)
        {
            WebRequest Req = WebRequest.Create("https://www.youtube.com/oembed?url=http://www.youtube.com/watch?v="+VideoCode+"&format=json");
            Req.Method = "GET";
            Req.ContentType = "application/json";
            Req.Timeout = 1000;
            try
            {
                WebResponse Res = Req.GetResponse();
                string SData = new StreamReader(Res.GetResponseStream()).ReadToEnd();
                return true;
            }
            catch (WebException E) { Console.WriteLine(E); return false; }
        }

        static string GetYoutubeID(string URL)
        {
            return URL.Replace("https://www.youtube.com/watch?v=", "").Substring(0, 11);
        }


    }
}
