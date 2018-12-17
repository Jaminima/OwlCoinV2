﻿using System;
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
            if (SegmentedMessage.Length < 2) { MessageHandler.NotLongEnough(e); return; }
            string SubCommand = SegmentedMessage[1].ToLower();
            if (SubCommand == "currenttest")
            {
                Current(e, SegmentedMessage);
            }
            //if (SubCommand == "queue")
            //{
            //    Queue(e, SegmentedMessage);
            //}
            //if (SubCommand == "playlist" || SubCommand == "list")
            //{
            //    Playlist(e, SegmentedMessage);
            //}
            //if (SubCommand == "canceltest")
            //{
            //    CancelSong(e, SegmentedMessage);
            //}
            if (SubCommand == "playtest")
            {
                Moderator.Commands.Play(e, SegmentedMessage);
            }
            if (SubCommand == "pausetest")
            {
                Moderator.Commands.Pause(e, SegmentedMessage);
            }
            if (SubCommand == "skiptest")
            {
                Moderator.Commands.Skip(e, SegmentedMessage);
            }
            if (SubCommand == "volumetest")
            {
                Moderator.Commands.Volume(e, SegmentedMessage);
            }
            if (SubCommand == "removetest")
            {
                Moderator.Commands.Remove(e, SegmentedMessage);
            }
            //if (SubCommand == "promote")
            //{
            //    Moderator.Commands.Promote(e, SegmentedMessage);
            //}
        }

        public static void Queue(OnMessageReceivedArgs e, string[] SegmentedMessage)
        {
            MessageHandler.SendMessage(e, Shared.ConfigHandler.Config["CommandResponses"]["Songs"]["Queue"].ToString(), null);
        }

        public static void Playlist(OnMessageReceivedArgs e, string[] SegmentedMessage)
        {
            MessageHandler.SendMessage(e, Shared.ConfigHandler.Config["CommandResponses"]["Songs"]["Playlist"].ToString(), null);
        }

        public static void Current(OnMessageReceivedArgs e, string[] SegmentedMessage)
        {
            Newtonsoft.Json.Linq.JToken YT = Shared.APIIntergrations.Youtube.VidDetails(NightBotReplacement.Init.GetSong()["YoutubeID"].ToString());
            string Name = YT["items"][0]["snippet"]["title"].ToString()+" -- https://youtu.be/"+YT["items"][0]["id"].ToString();
            MessageHandler.SendMessage(e, Shared.ConfigHandler.Config["CommandResponses"]["Songs"]["Current"].ToString(), null,-1,-1,Name);
        }

        public static Dictionary<String, String> PreviousSongRequests = new Dictionary<string, string> { };
        //public static void CancelSong(OnMessageReceivedArgs e, string[] SegmentedMessage)
        //{
        //    //Newtonsoft.Json.Linq.JToken QueueData = Shared.APIIntergrations.Nightbot.Requests.GetQueue();
        //    //if (PreviousSongRequests.ContainsKey(e.ChatMessage.UserId))
        //    //{
        //    //    Newtonsoft.Json.Linq.JToken Data = Shared.APIIntergrations.Nightbot.Requests.RemoveID(PreviousSongRequests[e.ChatMessage.UserId]);
        //    //    if (Data["status"].ToString() == "200")
        //    //    {
        //    //        int Required = int.Parse(Shared.ConfigHandler.Config["Songs"]["Cost"]["Viewer"].ToString());
        //    //        if (e.ChatMessage.IsSubscriber) { Required = int.Parse(Shared.ConfigHandler.Config["Songs"]["Cost"]["Subscriber"].ToString()); }
        //    //        Shared.Data.Accounts.GiveUser(e.ChatMessage.UserId, Shared.IDType.Twitch,Required);
        //    //        MessageHandler.SendMessage(e, Shared.ConfigHandler.Config["CommandResponses"]["Songs"]["Cancel"].ToString(), null);
        //    //    }
        //    //    else { MessageHandler.SendMessage(e, Shared.ConfigHandler.Config["CommandResponses"]["Songs"]["CancelFailed"].ToString(), null); }
        //    //}
        //    //else { MessageHandler.SendMessage(e, Shared.ConfigHandler.Config["CommandResponses"]["Songs"]["CancelFailed"].ToString(), null); }
        //}

        public static Newtonsoft.Json.Linq.JObject GetSongData()
        {
            WebRequest Req = WebRequest.Create("https://api.nightbot.tv/1/song_requests/queue");
            Req.Method = "GET";
            Req.Headers.Add("Nightbot-Channel", Shared.ConfigHandler.LoginConfig["NightBot"]["ChannelID"].ToString());
            try {
                WebResponse Res = Req.GetResponse();
                string D = new StreamReader(Res.GetResponseStream()).ReadToEnd();
                return Newtonsoft.Json.Linq.JObject.Parse(D);
            }
            catch { return null; }
        }

    }
}
