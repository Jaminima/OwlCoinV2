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
            int MinBet = int.Parse(Shared.ConfigHandler.Config["Duel"]["MinBet"].ToString());
            if (SegmentedMessage.Length != 3) { MessageHandler.NotLongEnough(Message); return; }
            string TheirID = MessageHandler.GetDiscordID(SegmentedMessage[1]);
            if (Shared.InputVerification.ContainsLetter(TheirID)) { await MessageHandler.SendMessage(Message, Shared.ConfigHandler.Config["CommandResponses"]["Errors"]["NoUser"].ToString()); return; }
            if (TheirID == Message.Author.Id.ToString())
            {
                await MessageHandler.SendMessage(Message, Shared.ConfigHandler.Config["CommandResponses"]["Errors"]["Self"].ToString());
                return;
            }
            int amount, myCoins, theirCoins;
            myCoins = amount = Shared.Data.Accounts.GetBalance(Message.Author.Id.ToString(), Shared.IDType.Discord);
            theirCoins = Shared.Data.Accounts.GetBalance(TheirID, Shared.IDType.Discord);
            if (SegmentedMessage[2].ToLower() != "all")
            {
                if (!int.TryParse(SegmentedMessage[2], out amount)) { MessageHandler.InvalidParameter(Message); return; }
            }
            else { if (myCoins > theirCoins) { amount = theirCoins; } }
            amount = Math.Abs(amount);
            if (amount < MinBet) { await MessageHandler.SendMessage(Message, Shared.ConfigHandler.Config["CommandResponses"]["Errors"]["BetTooLow"].ToString()); return; }
            if (amount <= myCoins)
            {
                if (amount <= theirCoins)
                {
                    if (Duels.duels.Add(new Shared.Duel(Message.Author.Id.ToString(), TheirID, amount)))
                    {
                        await MessageHandler.SendMessage(Message, Shared.ConfigHandler.Config["CommandResponses"]["Duel"]["Start"].ToString(), TheirID, amount);
                    }
                    else
                    {
                        await MessageHandler.SendMessage(Message, Shared.ConfigHandler.Config["CommandResponses"]["Duel"]["AlreadyInDuel"].ToString(),TheirID);
                    }
                }
                else
                {
                    await MessageHandler.SendMessage(Message, Shared.ConfigHandler.Config["CommandResponses"]["Errors"]["OtherNotEnough"].ToString());
                }
            }
            else
            {
                await MessageHandler.SendMessage(Message, Shared.ConfigHandler.Config["CommandResponses"]["Errors"]["NotEnough"].ToString());
            }
        }

        public static async Task Accept(SocketMessage Message, string[] SegmentedMessage)
        {
            Shared.Duel duel = Duels.duels.SingleOrDefault(d => d.target == Message.Author.Id.ToString());
            if (duel == null)
            {
                await MessageHandler.SendMessage(Message, Shared.ConfigHandler.Config["CommandResponses"]["Duel"]["NotInDuel"].ToString(), null);
                return;
            }
            Duels.duels.Remove(duel);
            int targetCoins = Shared.Data.Accounts.GetBalance(duel.target, Shared.IDType.Discord);
            int dueleeCoins = Shared.Data.Accounts.GetBalance(duel.duelee, Shared.IDType.Discord);
            if (targetCoins < duel.amount)
            {
                await MessageHandler.SendMessage(Message, Shared.ConfigHandler.Config["CommandResponses"]["Errors"]["OtherNotEnough"].ToString(), Message.Author.Id.ToString());
            }
            if (dueleeCoins < duel.amount)
            {
                await MessageHandler.SendMessage(Message, Shared.ConfigHandler.Config["CommandResponses"]["Errors"]["OtherNotEnough"].ToString(), Message.Author.Id.ToString());
            }
            Shared.Data.EventResponse Response;
            if (random.Next(100) < 50)
            {
                Response = Shared.Data.Accounts.PayUser(duel.target, Shared.IDType.Discord, duel.duelee, Shared.IDType.Discord, duel.amount);
                await MessageHandler.SendMessage(Message, Shared.ConfigHandler.Config["CommandResponses"]["Duel"]["Lose"].ToString(), duel.duelee, duel.amount);
                //await Message.Channel.SendMessageAsync("<@" + duel.duelee + "> won " + duel.amount + " Owlcoins in a duel against <@" + duel.target + "> and now has " + (dueleeCoins + duel.amount) + " Owlcoins!");
            }
            else
            {
                Response = Shared.Data.Accounts.PayUser(duel.duelee, Shared.IDType.Discord, duel.target, Shared.IDType.Discord, duel.amount);
                await MessageHandler.SendMessage(Message, Shared.ConfigHandler.Config["CommandResponses"]["Duel"]["Won"].ToString(), duel.duelee, duel.amount);
                //await Message.Channel.SendMessageAsync("<@" + duel.target + "> won " + duel.amount + " Owlcoins in a duel against <@" + duel.duelee + "> and now has " + (targetCoins + duel.amount) + " Owlcoins!");
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
                await MessageHandler.SendMessage(Message, Shared.ConfigHandler.Config["CommandResponses"]["Duel"]["Denied"].ToString(), null);
            }
            else
            {
                await MessageHandler.SendMessage(Message, Shared.ConfigHandler.Config["CommandResponses"]["Duel"]["NotInDuel"].ToString(), null);
            }
        }
    }
}
