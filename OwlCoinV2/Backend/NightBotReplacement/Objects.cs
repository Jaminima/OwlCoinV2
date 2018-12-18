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

    public class State
    {
        public PlayerState PlayerState=PlayerState.Playing;
        public int Volume = 20;
        public Newtonsoft.Json.Linq.JToken ToJson()
        {
            return Newtonsoft.Json.Linq.JToken.FromObject(this);
        }
    }

    public class Enqueued
    {
        public int Position;
        public string YoutubeCode;
        public string Title;
        public string Author;
        public ErrorReason ErrorReason;
    }

    public enum PlayerState
    {
        Playing,
        Paused
    }
    public enum ErrorReason
    {
        InQueue,
        InValidURL,
        Success,
        SearchTermInvalid
    }
}
