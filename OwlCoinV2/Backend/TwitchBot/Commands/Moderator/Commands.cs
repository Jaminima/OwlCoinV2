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
                if (SegmentedMessage[2].ToLower().EndsWith("k")) { Amount = int.Parse(SegmentedMessage[2].ToLower().Replace("k", "")) * 1000; }
                else if (Shared.InputVerification.ContainsLetter(SegmentedMessage[2])) { return; }
                else { Amount = int.Parse(SegmentedMessage[2]); }
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

        public static void NotMod(OnMessageReceivedArgs e)
        {
            Bot.TwitchC.SendMessage(e.ChatMessage.Channel, "@" + e.ChatMessage.Username + " you are not a Moderator!");
        }

    }
}
