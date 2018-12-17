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
        public static Queue PlayList = new Queue();
        public static State PlayerState = new State();

        public static Newtonsoft.Json.Linq.JToken GetSong()
        {
            if (Queue.SongQueue.Count == 0)
            {
                if (PlayList.SongQueue.Count == 0) { LoadPlaylist(); }
                Queue.SongQueue.Add(PlayList.SongQueue[0]);
                PlayList.SongQueue.RemoveAt(0);
            }
            return Queue.SongQueue[0].ToJson();
        }

        static Random Rnd = new Random();
        public static void LoadPlaylist()
        {
            PlayList = new Queue();
            string[] Lines = System.IO.File.ReadAllLines("./Data/Playlist.txt");
            string[] ShuffledLines = new string[Lines.Length];
            foreach(string Line in Lines)
            {
                int NewLocation = Rnd.Next(0,Lines.Length);
                while (ShuffledLines[NewLocation] != null)
                {
                    NewLocation = Rnd.Next(0, Lines.Length);
                }
                ShuffledLines[NewLocation] = Line;
            }
            foreach (string Line in ShuffledLines)
            {
                Enqueue(Line, null,true);
            }
        }

        public static Newtonsoft.Json.Linq.JToken GetState()
        {
            return PlayerState.ToJson();
        }

        public static Enqueued Enqueue(string URLName,string RequesterTwitchID,bool TooPlaylist=false)
        {
            Enqueued EnQueued = new Enqueued();
            if (URLName.StartsWith("https://www.youtube.com/watch?v="))
            {
                string YoutubeID = GetYoutubeID(URLName);
                if (YoutubeVideoExists(YoutubeID))
                {
                    Newtonsoft.Json.Linq.JToken YT = Shared.APIIntergrations.Youtube.VidDetails(YoutubeID);
                    if (System.Xml.XmlConvert.ToTimeSpan(YT["items"][0]["contentDetails"]["duration"].ToString()).TotalSeconds<600)
                    {
                        if (YoutubeVideoInQueue(YoutubeID)) { EnQueued.ErrorReason = ErrorReason.InQueue; return EnQueued; }
                        Song NewSong = new Song();
                        NewSong.RequesterTwitchID = RequesterTwitchID;
                        NewSong.YoutubeID = YoutubeID;
                        if (!TooPlaylist)
                        {
                            Queue.SongQueue.Add(NewSong);
                            EnQueued.Position = Queue.SongQueue.Count - 1;
                            EnQueued.Title = YT["items"][0]["snippet"]["title"].ToString();
                            EnQueued.Author = YT["items"][0]["snippet"]["channelTitle"].ToString();
                            EnQueued.ErrorReason = ErrorReason.Success;
                            return EnQueued;
                        }
                        else { PlayList.SongQueue.Add(NewSong); }
                    }
                }
            }
            EnQueued.ErrorReason = ErrorReason.InValidURL;
            return EnQueued;
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

        static bool YoutubeVideoInQueue(string VideoCode)
        {
            foreach (Song Song in Queue.SongQueue)
            {
                if (Song.YoutubeID == VideoCode) { return true; }
            }
            return false;
        }

        static string GetYoutubeID(string URL)
        {
            return URL.Replace("https://www.youtube.com/watch?v=", "").Substring(0, 11);
        }


    }
}
