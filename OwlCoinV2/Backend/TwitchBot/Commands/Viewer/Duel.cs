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
            if (SegmentedMessage.Length != 3) { MessageHandler.NotLongEnough(e); return; }
            string TheirID = UserHandler.UserFromUsername(SegmentedMessage[1].Replace("@","")).Matches[0].Id;
            if (TheirID == e.ChatMessage.UserId.ToString())
            {
                Bot.TwitchC.SendMessage(e.ChatMessage.Channel, "@" + e.ChatMessage.Username + ", You can't duel yourself");
                return;
            }
            int amount, myCoins, theirCoins;
            myCoins = amount = Shared.Data.Accounts.GetBalance(e.ChatMessage.UserId.ToString(), Shared.IDType.Twitch);
            theirCoins = Shared.Data.Accounts.GetBalance(TheirID, Shared.IDType.Twitch);
            if (SegmentedMessage[2].ToLower() != "all")
            {
                if (!int.TryParse(SegmentedMessage[2], out amount)) { MessageHandler.InvalidParameter(e); return; }
            }
            amount = Math.Abs(amount);
            if (amount <= myCoins)
            {
                if (amount <= theirCoins)
                {
                    if (Duels.duels.Add(new Shared.Duel(e.ChatMessage.UserId.ToString(), TheirID, amount)))
                    {
                        Bot.TwitchC.SendMessage(e.ChatMessage.Channel, "@"+ UserHandler.UserFromUserID(TheirID).Name +" @" + e.ChatMessage.Username + " wants to duel you for " + amount + " Owlcoins, you can !accept or !deny within 2 minutes");
                    }
                    else
                    {
                        Bot.TwitchC.SendMessage(e.ChatMessage.Channel, "@" + e.ChatMessage.Username + ", this person is already in a duel");
                    }
                }
                else
                {
                    Bot.TwitchC.SendMessage(e.ChatMessage.Channel, "@" + e.ChatMessage.Username + " only has " + theirCoins + " Owlcoins");
                }
            }
            else
            {
                Bot.TwitchC.SendMessage(e.ChatMessage.Channel, "@" + e.ChatMessage.Username + ", you only have " + myCoins + " Owlcoins");
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
                Bot.TwitchC.SendMessage(e.ChatMessage.Channel, "@" + e.ChatMessage.Username + " only has " + targetCoins + " Owlcoins");
            }
            if (dueleeCoins < duel.amount)
            {
                Bot.TwitchC.SendMessage(e.ChatMessage.Channel, "@" + e.ChatMessage.Username + " only has " + dueleeCoins + " Owlcoins");
            }
            Shared.Data.EventResponse Response;
            if (random.Next(100) < 50)
            {
                Response = Shared.Data.Accounts.PayUser(duel.target, Shared.IDType.Twitch, duel.duelee, Shared.IDType.Twitch, duel.amount);
                Bot.TwitchC.SendMessage(e.ChatMessage.Channel, "@" + UserHandler.UserFromUserID(duel.duelee).Name + " won " + duel.amount + " Owlcoins in a duel against @" + UserHandler.UserFromUserID(duel.target).Name + " and now has " + (dueleeCoins + duel.amount) + " Owlcoins!");
            }
            else
            {
                Response = Shared.Data.Accounts.PayUser(duel.duelee, Shared.IDType.Twitch, duel.target, Shared.IDType.Twitch, duel.amount);
                Bot.TwitchC.SendMessage(e.ChatMessage.Channel, "@" + UserHandler.UserFromUserID(duel.target).Name + " won " + duel.amount + " Owlcoins in a duel against @" + UserHandler.UserFromUserID(duel.duelee).Name + " and now has " + (targetCoins + duel.amount) + " Owlcoins!");
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
                Bot.TwitchC.SendMessage(e.ChatMessage.Channel, "@" + e.ChatMessage.Username + ", You have denied the duel");
            }
            else
            {
                Bot.TwitchC.SendMessage(e.ChatMessage.Channel, "@" + e.ChatMessage.Username + ", You are not in a duel");
            }
        }
    }
}
