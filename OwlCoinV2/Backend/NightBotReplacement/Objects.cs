using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OwlCoinV2.Backend.NightBotReplacement
{
    public class Song
    {
        public string YoutubeID, RequesterTwitchID;
        public Newtonsoft.Json.Linq.JToken ToJson()
        {
            return Newtonsoft.Json.Linq.JToken.FromObject(this);
        }
    }

    public class Queue
    {
        public List<Song> SongQueue=new List<Song> { };
        public Newtonsoft.Json.Linq.JToken ToJson()
        {
            return Newtonsoft.Json.Linq.JToken.FromObject(this);
        }
    }

    public enum PlayerState
    {
        Playing,
        Paused
    }
}
