using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwitchLib.Client;
using TwitchLib.Client.Models;
using TwitchLib.Client.Events;

namespace OwlCoinV2.Backend.TwitchBot.Commands.Viewer
{
    public static class Duel
    {
        static Random random = new Random();

        public static void StartDuel(OnMessageReceivedArgs e, string[] SegmentedMessage)
        {
            int MinBet = int.Parse(Shared.ConfigHandler.Config["Duel"]["MinBet"].ToString());
            if (SegmentedMessage.Length != 3) { MessageHandler.NotLongEnough(e); return; }
            string TheirID;
            try { TheirID = UserHandler.UserFromUsername(SegmentedMessage[1].Replace("@", "")).Matches[0].Id; }
            catch { MessageHandler.SendMessage(e, Shared.ConfigHandler.Config["CommandResponses"]["Errors"]["NoUser"].ToString(),null); return; }

            if (TheirID == e.ChatMessage.UserId.ToString())
            {
                MessageHandler.SendMessage(e, Shared.ConfigHandler.Config["CommandResponses"]["Errors"]["Self"].ToString(), null);
                return;
            }
            int amount, myCoins, theirCoins;
            myCoins = amount = Shared.Data.Accounts.GetBalance(e.ChatMessage.UserId.ToString(), Shared.IDType.Twitch);
            theirCoins = Shared.Data.Accounts.GetBalance(TheirID, Shared.IDType.Twitch);
            if (SegmentedMessage[2].ToLower() != "all")
            {
                if (!int.TryParse(SegmentedMessage[2], out amount)) { MessageHandler.InvalidParameter(e); return; }
            }
            else { if (myCoins > theirCoins) { amount = theirCoins; } }
            amount = Math.Abs(amount);
            if (amount < MinBet) { MessageHandler.SendMessage(e, Shared.ConfigHandler.Config["CommandResponses"]["Errors"]["BetTooLow"].ToString(), null,MinBet); return; }
            if (amount <= myCoins)
            {
                if (amount <= theirCoins)
                {
                    if (Duels.duels.Add(new Shared.Duel(e.ChatMessage.UserId.ToString(), TheirID, amount)))
                    {
                        MessageHandler.SendMessage(e, Shared.ConfigHandler.Config["CommandResponses"]["Duel"]["Start"].ToString(), UserHandler.UserFromUserID(TheirID).Name, amount);
                    }
                    else
                    {
                        MessageHandler.SendMessage(e, Shared.ConfigHandler.Config["CommandResponses"]["Duel"]["AlreadyInDuel"].ToString(), null);
                    }
                }
                else
                {
                    MessageHandler.SendMessage(e, Shared.ConfigHandler.Config["CommandResponses"]["Errors"]["OtherNotEnough"].ToString(), null);
                }
            }
            else
            {
                MessageHandler.SendMessage(e, Shared.ConfigHandler.Config["CommandResponses"]["Errors"]["NotEnough"].ToString(), null);
            }
        }

        public static void Accept(OnMessageReceivedArgs e, string[] SegmentedMessage)
        {
            Shared.Duel duel = Duels.duels.SingleOrDefault(d => d.target == e.ChatMessage.UserId.ToString());
            if (duel == null)
            {
                Bot.TwitchC.SendMessage(e.ChatMessage.Channel, "@" + e.ChatMessage.Username + ", You are not in a duel");
                return;
            }
            Duels.duels.Remove(duel);
            int targetCoins = Shared.Data.Accounts.GetBalance(duel.target, Shared.IDType.Twitch);
            int dueleeCoins = Shared.Data.Accounts.GetBalance(duel.duelee, Shared.IDType.Twitch);
            if (targetCoins < duel.amount)
            {
                MessageHandler.SendMessage(e, Shared.ConfigHandler.Config["CommandResponses"]["Errors"]["OtherNotEnough"].ToString(), e.ChatMessage.Username);
                //Bot.TwitchC.SendMessage(e.ChatMessage.Channel, "@" + e.ChatMessage.Username + " only has " + targetCoins + " Owlcoins");
            }
            if (dueleeCoins < duel.amount)
            {
                MessageHandler.SendMessage(e, Shared.ConfigHandler.Config["CommandResponses"]["Errors"]["OtherNotEnough"].ToString(), e.ChatMessage.Username);
                //Bot.TwitchC.SendMessage(e.ChatMessage.Channel, "@" + e.ChatMessage.Username + " only has " + dueleeCoins + " Owlcoins");
            }
            Shared.Data.EventResponse Response;
            if (random.Next(100) < 50)
            {
                Response = Shared.Data.Accounts.PayUser(duel.target, Shared.IDType.Twitch, duel.duelee, Shared.IDType.Twitch, duel.amount);
                MessageHandler.SendMessage(e, Shared.ConfigHandler.Config["CommandResponses"]["Duel"]["Lose"].ToString(), UserHandler.UserFromUserID(duel.target).Name,duel.amount);
                //Bot.TwitchC.SendMessage(e.ChatMessage.Channel, "@" + UserHandler.UserFromUserID(duel.duelee).Name + " won " + duel.amount + " Owlcoins in a duel against @" + UserHandler.UserFromUserID(duel.target).Name + " and now has " + (dueleeCoins + duel.amount) + " Owlcoins!");
            }
            else
            {
                Response = Shared.Data.Accounts.PayUser(duel.duelee, Shared.IDType.Twitch, duel.target, Shared.IDType.Twitch, duel.amount);
                MessageHandler.SendMessage(e, Shared.ConfigHandler.Config["CommandResponses"]["Duel"]["Win"].ToString(), UserHandler.UserFromUserID(duel.duelee).Name,duel.amount);
                //Bot.TwitchC.SendMessage(e.ChatMessage.Channel, "@" + UserHandler.UserFromUserID(duel.target).Name + " won " + duel.amount + " Owlcoins in a duel against @" + UserHandler.UserFromUserID(duel.duelee).Name + " and now has " + (targetCoins + duel.amount) + " Owlcoins!");
            }
            if (!Response.Success)
            {
                Console.WriteLine(Response.Message);
            }
        }

        public static void Deny(OnMessageReceivedArgs e, string[] SegmentedMessage)
        {
            if (Duels.duels.RemoveWhere(duel => duel.target == e.ChatMessage.UserId.ToString()) == 1)
            {
                MessageHandler.SendMessage(e, Shared.ConfigHandler.Config["CommandResponses"]["Duel"]["Denied"].ToString(), null);
            }
            else
            {
                MessageHandler.SendMessage(e, Shared.ConfigHandler.Config["CommandResponses"]["Duel"]["NotInDuel"].ToString(), null);
            }
        }
    }
}
