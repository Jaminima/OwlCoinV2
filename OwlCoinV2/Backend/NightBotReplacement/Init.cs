using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace OwlCoinV2.Backend.NightBotReplacement
{
    public static class Init
    {
        public static Queue Queue = new Queue();

        public static Newtonsoft.Json.Linq.JToken GetQueue()
        {
            return Queue.ToJson();
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
                    Queue.SongQueue.Add(Queue.SongQueue.Count,NewSong);
                    return true;
                }
            }
            return false;
        }

        static bool YoutubeVideoExists(string VideoCode)
        {
            WebRequest Req = WebRequest.Create("https://www.youtube.com/oembed?url=http://www.youtube.com/watch?v="+VideoCode+"&format=json");
            try
            {
                Req.GetResponse();
                return true;
            }
            catch { return false; }
        }

        static string GetYoutubeID(string URL)
        {
            return URL.Replace("https://www.youtube.com/watch?v=", "").Substring(0, 11);
        }


    }
}
