using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwitchLib.Client;
using TwitchLib.Client.Models;
using TwitchLib.Client.Events;

namespace OwlCoinV2.Backend.TwitchBot.Commands.Moderator
{
    public static class Commands
    {
        public static void GivePoints(OnMessageReceivedArgs e, string[] SegmentedMessage)
        {
            if (e.ChatMessage.IsModerator)
            {
                string TheirID = UserHandler.UserFromUsername(SegmentedMessage[1].Replace("@", "")).Matches[0].Id;
                Shared.Data.Accounts.GiveUser(TheirID,Shared.IDType.Twitch,int.Parse(SegmentedMessage[2]));
                Bot.TwitchC.SendMessage(e.ChatMessage.Channel, "@" + e.ChatMessage.Username + " gave @" + SegmentedMessage[1] + " " + SegmentedMessage[2] + " owlcoin!");
            }
            else { NotMod(e); }
        }

        public static void NotMod(OnMessageReceivedArgs e)
        {
            Bot.TwitchC.SendMessage(e.ChatMessage.Channel, "@" + e.ChatMessage.Username + " you are not a Moderator!");
        }

    }
}
