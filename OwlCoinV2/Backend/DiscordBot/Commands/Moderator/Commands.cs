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
                await MessageHandler.SendMessage(Message, Shared.ConfigHandler.Config["CommandResponses"]["Moderator"]["Give"].ToString(), TheirID, Amount);
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

        public static async Task RefreshConfig(SocketMessage Message, string[] SegmentedMessage)
        {
            if (IsMod(Message).Result||Message.Author.Id.ToString()== "300712019466911744")
            {
                Shared.ConfigHandler.LoadConfig();
                await MessageHandler.SendMessage(Message, Shared.ConfigHandler.Config["CommandResponses"]["Moderator"]["RefreshConfig"].ToString(), null);
                return;
            }
            await NotMod(Message);
        }

        public static async Task NotMod(SocketMessage Message)
        {
            await MessageHandler.SendMessage(Message, Shared.ConfigHandler.Config["CommandResponses"]["Errors"]["NotMod"].ToString());
        }

    }
}
