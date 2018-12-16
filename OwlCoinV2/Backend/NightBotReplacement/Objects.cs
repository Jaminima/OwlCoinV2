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
    }

    public class Queue
    {
        public Dictionary<int,Song> SongQueue=new Dictionary<int, Song> { };
        public Newtonsoft.Json.Linq.JToken ToJson()
        {
            return Newtonsoft.Json.Linq.JToken.FromObject(this);
        }
    }
}
