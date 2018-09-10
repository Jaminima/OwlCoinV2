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
    public static class Commands
    {
        static Random random = new Random();

        public static void JoinRaffle(OnMessageReceivedArgs e) { if (!Drops.RaffleParticipant.Contains(e.ChatMessage.UserId)) { Drops.RaffleParticipant.Add(e.ChatMessage.UserId); } }

        public static void Pay(OnMessageReceivedArgs e,string[] SegmentedMessage)
        {
            if (SegmentedMessage.Length != 3) { MessageHandler.NotLongEnough(e); return; }
            string TheirID = SegmentedMessage[1]; Shared.IDType TheirIDType = Shared.IDType.Twitch;
            if (TheirID.StartsWith("@")) { TheirID = TheirID.Replace("@", ""); TheirID = UserHandler.UserFromUsername(TheirID).Matches[0].Id; }
            if (SegmentedMessage[2].ToLower() == "all") { SegmentedMessage[2] = Shared.Data.Accounts.GetBalance(e.ChatMessage.UserId, Shared.IDType.Twitch).ToString(); }

            Shared.Data.EventResponse Response = Shared.Data.Accounts.PayUser(e.ChatMessage.UserId.ToString(), Shared.IDType.Twitch, TheirID, TheirIDType, int.Parse(SegmentedMessage[2]));
            //if (Response.Success)
            //{
            Bot.TwitchC.SendMessage(e.ChatMessage.Channel, "@" + e.ChatMessage.Username + Response.Message);
            //}
        }

        public static void OwlCoin(OnMessageReceivedArgs e, string[] SegmentedMessage)
        {
            if (SegmentedMessage.Length == 2 && SegmentedMessage[1].StartsWith("@"))
            {
                SegmentedMessage[1] = SegmentedMessage[1].Replace("@", "");
                String TheirID = UserHandler.UserFromUsername(SegmentedMessage[1]).Matches[0].Id;
                Shared.Data.UserData.CreateUser(TheirID, Shared.IDType.Twitch);
                Bot.TwitchC.SendMessage(e.ChatMessage.Channel, "@" + e.ChatMessage.Username + " @" + SegmentedMessage[1] + " has " + Shared.Data.Accounts.GetBalance(TheirID, Shared.IDType.Twitch) + " Owlcoin!");
            }
            Shared.Data.UserData.CreateUser(e.ChatMessage.UserId, Shared.IDType.Twitch);
            Bot.TwitchC.SendMessage(e.ChatMessage.Channel, "@" + e.ChatMessage.Username + " you have " + Shared.Data.Accounts.GetBalance(e.ChatMessage.UserId, Shared.IDType.Twitch) + " Owlcoin!");
        }

        public static void SongRequest(OnMessageReceivedArgs e, string[] SegmentedMessage)
        {
            int MyBal = Shared.Data.Accounts.GetBalance(e.ChatMessage.UserId, Shared.IDType.Twitch);
            int Required = 500;
            if (Drops.GetSubs().Contains(e.ChatMessage.UserId)) { Required = 250; }
            if (MyBal >= Required)
            {
                MyBal -= Required;
                Shared.Data.Accounts.TakeUser(e.ChatMessage.UserId, Shared.IDType.Twitch, Required);
                Bot.TwitchC.SendMessage(e.ChatMessage.Channel, "!sr" + e.ChatMessage.Message.Remove(0, Shared.ConfigHandler.Config["Prefix"].ToString().Length + SegmentedMessage[0].Length));
                Bot.TwitchC.SendMessage(e.ChatMessage.Channel, "@" + e.ChatMessage.Username + " song requested!");
            }
        }

        public static void Roulette(OnMessageReceivedArgs e, string[] SegmentedMessage)
        {
            if (SegmentedMessage.Length != 2) { MessageHandler.NotLongEnough(e); return; }
            int coins, amount;
            coins = amount = Shared.Data.Accounts.GetBalance(e.ChatMessage.UserId.ToString(), Shared.IDType.Twitch);
            if (SegmentedMessage[1] != "all")
            {
                if (!int.TryParse(SegmentedMessage[1], out amount)) { MessageHandler.InvalidParameter(e); return; }
            }

            if (amount <= coins)
            {
                if (random.Next(100) < int.Parse(Shared.ConfigHandler.Config["GambleWinChance"].ToString()))
                {
                    Shared.Data.Accounts.GiveUser(e.ChatMessage.UserId.ToString(), Shared.IDType.Twitch, amount);
                    Bot.TwitchC.SendMessage(e.ChatMessage.Channel, "@" + e.ChatMessage.Username + " won " + amount + " Owlcoins in roulette and now has " + (coins + amount) + " Owlcoins!");
                }
                else
                {
                    Shared.Data.Accounts.TakeUser(e.ChatMessage.UserId.ToString(), Shared.IDType.Twitch, amount);
                    Bot.TwitchC.SendMessage(e.ChatMessage.Channel, "@" + e.ChatMessage.Username + " lost " + amount + " Owlcoins in roulette and now has " + (coins - amount) + " Owlcoins!");
                }
            }
            else
            {
                Bot.TwitchC.SendMessage(e.ChatMessage.Channel, "@" + e.ChatMessage.Username + ", you only have " + coins + " Owlcoins");
            }
        }

    }
}
