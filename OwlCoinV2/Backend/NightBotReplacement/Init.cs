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
        static Queue UnShuffledPlayList;
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
            if (UnShuffledPlayList == null)
            {
                UnShuffledPlayList = new Queue();
                string PageToken = null;
                Newtonsoft.Json.Linq.JToken YTPlaylistItem = null;
                while (PageToken != null || YTPlaylistItem == null)
                {
                    YTPlaylistItem = Shared.APIIntergrations.Youtube.PlayListRead("PLPowxVhPe3JyPGiidXbm3QdC2H9yiS6qB", PageToken);
                    try { PageToken = YTPlaylistItem["nextPageToken"].ToString(); } catch { PageToken = null; }
                    foreach (Newtonsoft.Json.Linq.JToken QueueItem in YTPlaylistItem["items"])
                    {
                        Song NewSong = new Song();
                        NewSong.YoutubeID = QueueItem["contentDetails"]["videoId"].ToString();
                        UnShuffledPlayList.SongQueue.Add(NewSong);
                    }
                }
            }
            PlayList = new Queue();
            List<int> AddedSongs = new List<int> { };
            while (AddedSongs.Count != UnShuffledPlayList.SongQueue.Count)
            {
                int SongToAdd = Rnd.Next(0, UnShuffledPlayList.SongQueue.Count);
                if (!AddedSongs.Contains(SongToAdd))
                {
                    PlayList.SongQueue.Add(UnShuffledPlayList.SongQueue[SongToAdd]);
                    AddedSongs.Add(SongToAdd);
                }
            }
        }

        public static Newtonsoft.Json.Linq.JToken GetState()
        {
            return PlayerState.ToJson();
        }

        public static Enqueued Enqueue(string URLName,string RequesterTwitchID,bool TooPlaylist=false)
        {
            Enqueued EnQueued = new Enqueued();
            if (URLName.Contains("v=")||URLName.StartsWith("https://youtu.be/"))
            {
                string YoutubeID = GetYoutubeID(URLName);
                if (YoutubeVideoExists(YoutubeID))
                {
                    Newtonsoft.Json.Linq.JToken YT = Shared.APIIntergrations.Youtube.VidDetails(YoutubeID);
                    if (YT["items"].Count() == 0)
                    {
                        EnQueued.ErrorReason = ErrorReason.SearchTermInvalid;
                    }
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
                        }
                        else { PlayList.SongQueue.Add(NewSong); }
                        EnQueued.ErrorReason = ErrorReason.Success;
                        return EnQueued;
                    }
                }
                EnQueued.ErrorReason = ErrorReason.InValidURL;
                return EnQueued;
            }
            Newtonsoft.Json.Linq.JToken YTSearchResponse = Shared.APIIntergrations.Youtube.VidFromKeyWords(URLName);
            foreach (Newtonsoft.Json.Linq.JToken ResponseItem in YTSearchResponse["items"])
            {
                Enqueued Try = Enqueue("https://www.youtube.com/watch?v="+ResponseItem["id"]["videoId"].ToString(), RequesterTwitchID, TooPlaylist);
                if (Try.ErrorReason == ErrorReason.Success) { return Try; }
            }
            EnQueued.ErrorReason = ErrorReason.SearchTermInvalid;
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
            if (URL.Contains("v=")) { return URL.Split(new string[] { "v=" }, StringSplitOptions.None)[1].Substring(0, 11); }
            return URL.Replace("https://youtu.be/", "");
        }


    }
}
