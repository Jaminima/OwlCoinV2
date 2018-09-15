using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;

namespace OwlCoinV2.Backend.DiscordBot.Commands.Viewer
{
    public static class Duel
    {
        static Random random = new Random();

        public static async Task StartDuel(SocketMessage Message, string[] SegmentedMessage)
        {
            if (SegmentedMessage.Length != 3) { MessageHandler.NotLongEnough(Message); return; }
            string TheirID = MessageHandler.GetDiscordID(SegmentedMessage[1]);
            if (TheirID == Message.Author.Id.ToString())
            {
                await Message.Channel.SendMessageAsync("<@" + Message.Author.Id + ">, You can't duel yourself");
                return;
            }
            int amount, myCoins, theirCoins;
            myCoins = amount = Shared.Data.Accounts.GetBalance(Message.Author.Id.ToString(), Shared.IDType.Discord);
            theirCoins = Shared.Data.Accounts.GetBalance(TheirID, Shared.IDType.Discord);
            if (SegmentedMessage[2].ToLower() != "all")
            {
                if (!int.TryParse(SegmentedMessage[2], out amount)) { MessageHandler.InvalidParameter(Message); return; }
            }
            if (amount <= myCoins)
            {
                if (amount <= theirCoins)
                {
                    if (Duels.duels.Add(new Shared.Duel(Message.Author.Id.ToString(), TheirID, amount)))
                    {
                        await Message.Channel.SendMessageAsync("<@" + TheirID + ">, <@" + Message.Author.Id + "> wants to duel you for " + amount + " Owlcoins, you can oc!accept or oc!deny within 2 minutes");
                    }
                    else
                    {
                        await Message.Channel.SendMessageAsync("<@" + Message.Author.Id + ">, this person is already in a duel");
                    }
                }
                else
                {
                    await Message.Channel.SendMessageAsync("<@" + TheirID + "> only has " + theirCoins + " Owlcoins");
                }
            }
            else
            {
                await Message.Channel.SendMessageAsync("<@" + Message.Author.Id + ">, you only have " + myCoins + " Owlcoins");
            }
        }

        public static async Task Accept(SocketMessage Message, string[] SegmentedMessage)
        {
            Shared.Duel duel = Duels.duels.SingleOrDefault(d => d.target == Message.Author.Id.ToString());
            if (duel == null)
            {
                await Message.Channel.SendMessageAsync("<@" + Message.Author.Id + ">, You are not in a duel");
                return;
            }
            Duels.duels.Remove(duel);
            int targetCoins = Shared.Data.Accounts.GetBalance(duel.target, Shared.IDType.Discord);
            int dueleeCoins = Shared.Data.Accounts.GetBalance(duel.duelee, Shared.IDType.Discord);
            if (targetCoins < duel.amount)
            {
                await Message.Channel.SendMessageAsync("<@" + duel.target + "> only has " + targetCoins + " Owlcoins");
            }
            if (dueleeCoins < duel.amount)
            {
                await Message.Channel.SendMessageAsync("<@" + duel.duelee + "> only has " + dueleeCoins + " Owlcoins");
            }
            Shared.Data.EventResponse Response;
            if (random.Next(100) < 50)
            {
                Response = Shared.Data.Accounts.PayUser(duel.target, Shared.IDType.Discord, duel.duelee, Shared.IDType.Discord, duel.amount);
                await Message.Channel.SendMessageAsync("<@" + duel.duelee + "> won " + duel.amount + " Owlcoins in a duel against <@" + duel.target + "> and now has " + (dueleeCoins + duel.amount) + " Owlcoins!");
            }
            else
            {
                Response = Shared.Data.Accounts.PayUser(duel.duelee, Shared.IDType.Discord, duel.target, Shared.IDType.Discord, duel.amount);
                await Message.Channel.SendMessageAsync("<@" + duel.target + "> won " + duel.amount + " Owlcoins in a duel against <@" + duel.duelee + "> and now has " + (targetCoins + duel.amount) + " Owlcoins!");
            }
            if (!Response.Success)
            {
                Console.WriteLine(Response.Message);
            }
        }

        public static async Task Deny(SocketMessage Message, string[] SegmentedMessage)
        {
            if (Duels.duels.RemoveWhere(duel => duel.target == Message.Author.Id.ToString()) == 1)
            {
                await Message.Channel.SendMessageAsync("<@" + Message.Author.Id + ">, You have denied the duel");
            }
            else
            {
                await Message.Channel.SendMessageAsync("<@" + Message.Author.Id + ">, You are not in a duel");
            }
        }
    }
}
