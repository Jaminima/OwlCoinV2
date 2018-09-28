using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwitchLib.Client;
using TwitchLib.Client.Models;
using TwitchLib.Client.Events;
using System.Net;
using System.IO;

namespace OwlCoinV2.Backend.TwitchBot.Commands.Viewer
{
    public static class Songs
    {
        public static void Proccessor(OnMessageReceivedArgs e, string[] SegmentedMessage)
        {
            if (SegmentedMessage.Length != 2) { MessageHandler.NotLongEnough(e); return; }
            string SubCommand = SegmentedMessage[1].ToLower();
            if (SubCommand == "current")
            {
                Current(e, SegmentedMessage);
            }
            if (SubCommand == "queue")
            {
                Queue(e, SegmentedMessage);
            }
            if (SubCommand == "playlist"||SubCommand=="list")
            {
                Playlist(e,SegmentedMessage);
            }
        }

        public static void Queue(OnMessageReceivedArgs e, string[] SegmentedMessage)
        {
            Bot.TwitchC.SendMessage(e.ChatMessage.Channel, "@" + e.ChatMessage.Username + " View queue here: https://beta.nightbot.tv/t/harbonator/song_requests");
        }

        public static void Playlist(OnMessageReceivedArgs e, string[] SegmentedMessage)
        {
            Bot.TwitchC.SendMessage(e.ChatMessage.Channel, "@" + e.ChatMessage.Username + " View playlist here: https://beta.nightbot.tv/t/harbonator/song_requests/playlist");
        }

        public static void Current(OnMessageReceivedArgs e, string[] SegmentedMessage)
        {
            Newtonsoft.Json.Linq.JObject SongData=GetSongData();
            if (SongData == null) { Bot.TwitchC.SendMessage(e.ChatMessage.Channel, "@" + e.ChatMessage.Username + " An Error Occured while fetching data! Please try again soon."); return; }
            string Name = SongData["_currentSong"]["track"]["title"].ToString();
            Bot.TwitchC.SendMessage(e.ChatMessage.Channel, "@" + e.ChatMessage.Username + " Current song is: " + Name);
        }

        public static Newtonsoft.Json.Linq.JObject GetSongData()
        {
            WebRequest Req = WebRequest.Create("https://api.nightbot.tv/1/song_requests/queue");
            Req.Method = "GET";
            Req.Headers.Add("Nightbot-Channel", "5ab3cf2fd4eb1704dd2dd5e5");
            try {
                WebResponse Res = Req.GetResponse();
                string D = new StreamReader(Res.GetResponseStream()).ReadToEnd();
                return Newtonsoft.Json.Linq.JObject.Parse(D);
            }
            catch { return null; }
        }

    }
}
