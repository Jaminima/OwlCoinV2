using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwitchLib.Client;
using TwitchLib.Client.Models;
using TwitchLib.Client.Events;
using System.Threading;

namespace OwlCoinV2.Backend.TwitchBot.Commands.Moderator
{
    public static class Commands
    {
        public static void GivePoints(OnMessageReceivedArgs e, string[] SegmentedMessage)
        {
            if (e.ChatMessage.IsModerator||e.ChatMessage.IsBroadcaster)
            {
                string TheirID = UserHandler.UserFromUsername(SegmentedMessage[1].Replace("@", "")).Matches[0].Id; int Amount = 0;
                try
                {
                    if (SegmentedMessage[2].ToLower().EndsWith("k")) { Amount = int.Parse(SegmentedMessage[2].ToLower().Replace("k", "")) * 1000; }
                    else if (Shared.InputVerification.ContainsLetter(SegmentedMessage[2])) { return; }
                    else { Amount = int.Parse(SegmentedMessage[2]); }
                }
                catch { MessageHandler.InvalidParameter(e); }
                Shared.Data.Accounts.GiveUser(TheirID, Shared.IDType.Twitch, Amount);
                MessageHandler.SendMessage(e, Shared.ConfigHandler.Config["CommandResponses"]["Moderator"]["Give"].ToString(), SegmentedMessage[1].Replace("@", ""), Amount);
            }
            else { NotMod(e); }
        }

        public static void SetGame(OnMessageReceivedArgs e, string[] SegmentedMessage)
        {
            if (e.ChatMessage.IsModerator || e.ChatMessage.IsBroadcaster)
            {
                string Game = e.ChatMessage.Message.Replace(SegmentedMessage[0]+" ", "");
                new Thread(async () =>await Bot.TwitchA.Channels.v5.UpdateChannelAsync(UserHandler.UserFromUsername(e.ChatMessage.Channel).Matches[0].Id, null, Game)).Start();
                MessageHandler.SendMessage(e, Shared.ConfigHandler.Config["CommandResponses"]["Moderator"]["SetGame"].ToString(), null,-1,-1,Game);
            }
            else { NotMod(e); }
        }

        public static void SetTitle(OnMessageReceivedArgs e, string[] SegmentedMessage)
        {
            if (e.ChatMessage.IsModerator || e.ChatMessage.IsBroadcaster)
            {
                string Name = e.ChatMessage.Message.Replace(SegmentedMessage[0] + " ", "");
                new Thread(async () => await Bot.TwitchA.Channels.v5.UpdateChannelAsync(UserHandler.UserFromUsername(e.ChatMessage.Channel).Matches[0].Id, Name)).Start();
                MessageHandler.SendMessage(e, Shared.ConfigHandler.Config["CommandResponses"]["Moderator"]["SetTitle"].ToString(), null, -1, -1, Name);
            }
            else { NotMod(e); }
        }

        public static void RefreshConfig(OnMessageReceivedArgs e, string[] SegmentedMessage)
        {
            if (e.ChatMessage.IsBroadcaster || e.ChatMessage.IsModerator||e.ChatMessage.Username=="jccjaminima")
            {
                Shared.ConfigHandler.LoadConfig();
                MessageHandler.SendMessage(e, Shared.ConfigHandler.Config["CommandResponses"]["Moderator"]["RefreshConfig"].ToString(), null);
                return;
            }
            NotMod(e);
        }

        public static void Play(OnMessageReceivedArgs e, string[] SegmentedMessage)
        {
            if (e.ChatMessage.IsBroadcaster || e.ChatMessage.IsModerator)
            {
                Newtonsoft.Json.Linq.JToken Result= Nightbot.Requests.PlaySong();
                if (Result["status"].ToString() == "200") { MessageHandler.SendMessage(e, Shared.ConfigHandler.Config["CommandResponses"]["Moderator"]["Play"].ToString(), null); }
                else { MessageHandler.SendMessage(e, Shared.ConfigHandler.Config["CommandResponses"]["Moderator"]["NightBotError"].ToString(), null);
                    Console.WriteLine(Result["message"].ToString());
                }
                return;
            }
            NotMod(e);
        }

        public static void Pause(OnMessageReceivedArgs e, string[] SegmentedMessage)
        {
            if (e.ChatMessage.IsBroadcaster || e.ChatMessage.IsModerator)
            {
                Newtonsoft.Json.Linq.JToken Result = Nightbot.Requests.PauseSong();
                if (Result["status"].ToString() == "200") { MessageHandler.SendMessage(e, Shared.ConfigHandler.Config["CommandResponses"]["Moderator"]["Pause"].ToString(), null); }
                else { MessageHandler.SendMessage(e, Shared.ConfigHandler.Config["CommandResponses"]["Moderator"]["NightBotError"].ToString(), null);
                    Console.WriteLine(Result["message"].ToString());
                }
                return;
            }
            NotMod(e);
        }

        public static void Skip(OnMessageReceivedArgs e, string[] SegmentedMessage)
        {
            if (e.ChatMessage.IsBroadcaster || e.ChatMessage.IsModerator)
            {
                Newtonsoft.Json.Linq.JToken Result = Nightbot.Requests.SkipSong();
                if (Result["status"].ToString() == "200") { MessageHandler.SendMessage(e, Shared.ConfigHandler.Config["CommandResponses"]["Moderator"]["Skip"].ToString(), null); }
                else { MessageHandler.SendMessage(e, Shared.ConfigHandler.Config["CommandResponses"]["Moderator"]["NightBotError"].ToString(), null);
                    Console.WriteLine(Result["message"].ToString());
                }
                return;
            }
            NotMod(e);
        }

        public static void Remove(OnMessageReceivedArgs e, string[] SegmentedMessage)
        {
            if (e.ChatMessage.IsBroadcaster || e.ChatMessage.IsModerator)
            {
                int Item = 0;
                try { Item = int.Parse(SegmentedMessage[2]); } catch { MessageHandler.InvalidParameter(e); return; }
                Newtonsoft.Json.Linq.JToken Result = Nightbot.Requests.RemoveItem(Item);
                if (Result["status"].ToString() == "200") { MessageHandler.SendMessage(e, Shared.ConfigHandler.Config["CommandResponses"]["Moderator"]["RemoveSong"].ToString(), null); }
                else
                {
                    MessageHandler.SendMessage(e, Shared.ConfigHandler.Config["CommandResponses"]["Moderator"]["NightBotError"].ToString(), null);
                    Console.WriteLine(Result["message"].ToString());
                }
                return;
            }
            NotMod(e);
        }

        public static void Volume(OnMessageReceivedArgs e, string[] SegmentedMessage)
        {
            if (e.ChatMessage.IsBroadcaster || e.ChatMessage.IsModerator)
            {
                if (SegmentedMessage.Length != 3) { MessageHandler.NotLongEnough(e); return; }
                int Volume = int.Parse(SegmentedMessage[2]);
                Newtonsoft.Json.Linq.JToken Result = Nightbot.Requests.SetVolume(Volume);
                if (Result["status"].ToString() == "200") { MessageHandler.SendMessage(e, Shared.ConfigHandler.Config["CommandResponses"]["Moderator"]["Volume"].ToString(), null); }
                else
                {
                    MessageHandler.SendMessage(e, Shared.ConfigHandler.Config["CommandResponses"]["Moderator"]["NightBotError"].ToString(), null);
                    Console.WriteLine(Result["message"].ToString());
                }
                return;
            }
            NotMod(e);
        }

        public static void NotMod(OnMessageReceivedArgs e)
        {
            MessageHandler.SendMessage(e, Shared.ConfigHandler.Config["CommandResponses"]["Errors"]["NotMod"].ToString(),null);
        }

    }
}
