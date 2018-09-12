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
                Shared.Data.Accounts.GiveUser(TheirID, Shared.IDType.Discord, int.Parse(SegmentedMessage[2]));
                await Message.Channel.SendMessageAsync("<@" + Message.Author.Id + "> gave <@" + TheirID + "> " + SegmentedMessage[2] + " owlcoin!");
            }
            else { NotMod(Message); }
        }

        static async Task<bool> IsMod(SocketMessage Message)
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
