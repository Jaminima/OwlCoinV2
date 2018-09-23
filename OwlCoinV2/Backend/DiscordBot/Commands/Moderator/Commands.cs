using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;
using Discord.Net;
using Discord.Commands;
using Discord;

namespace OwlCoinV2.Backend.DiscordBot.Commands.Moderator
{
    public static class Commands
    {

        public static async Task GivePoints(SocketMessage Message,string[] SegmentedMessage)
        {
            if (IsMod(Message).Result)
            {
                string TheirID = MessageHandler.GetDiscordID(SegmentedMessage[1]);
                int Amount = 0;
                if (SegmentedMessage[2].ToLower().EndsWith("k")) { Amount = int.Parse(SegmentedMessage[2].ToLower().Replace("k", "")) * 1000; }
                else if (Shared.InputVerification.ContainsLetter(SegmentedMessage[2])) { return; }
                else { Amount = int.Parse(SegmentedMessage[2]); }
                Shared.Data.Accounts.GiveUser(TheirID, Shared.IDType.Discord, Amount);
                await Message.Channel.SendMessageAsync("<@" + Message.Author.Id + "> gave <@" + TheirID + "> " + SegmentedMessage[2] + " owlcoin!");
            }
            else { await NotMod(Message); }
        }

        public static async Task<bool> IsMod(SocketMessage Message)
        { 
            foreach (SocketRole Role in ((SocketGuildUser)Message.Author).Roles)
            {
                if (Role.Name == "Mod")
                {
                    return true;
                }
            }
            return false;
        }

        public static async Task NotMod(SocketMessage Message)
        {
             await Message.Channel.SendMessageAsync("<@" + Message.Author.Id + "> you are not a Moderator!");
        }

    }
}
