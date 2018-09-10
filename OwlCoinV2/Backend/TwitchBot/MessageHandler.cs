using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwitchLib.Client;
using TwitchLib.Client.Models;
using TwitchLib.Client.Events;
using System.Threading;

namespace OwlCoinV2.Backend.TwitchBot
{
    public static class MessageHandler
    {
        static Random random = new Random();

        public static void HandleMessage(object sender, OnMessageReceivedArgs e)
        {
            new Thread(() => Proccessor(e)).Start();
        }

        static void Proccessor(OnMessageReceivedArgs e)
        {
            string[] SegmentedMessage = e.ChatMessage.Message.Split(" ".ToCharArray());
            string Command = SegmentedMessage[0].ToLower();
            string Prefix = Shared.ConfigHandler.Config["Prefix"].ToString();
            if (Command.StartsWith(Prefix))
            {
                Command = Command.Remove(0, Prefix.Length);

                //if (Command == "echo")
                //{
                //    if (SegmentedMessage.Length < 2) { NotLongEnough(e); return; }
                //    Bot.TwitchC.SendMessage(e.ChatMessage.Channel, e.ChatMessage.Message.Remove(0, Command.Length + Prefix.Length + 1));
                //}

                if (Command == "pay")
                {
                    if (SegmentedMessage.Length != 3) { NotLongEnough(e); return; }
                    string TheirID = SegmentedMessage[1]; Shared.IDType TheirIDType = Shared.IDType.Twitch;
                    if (TheirID.StartsWith("@")) { TheirID = TheirID.Replace("@", ""); TheirID= UserHandler.UserFromUsername(TheirID).Matches[0].Id; }
                    if (SegmentedMessage[2].ToLower() == "all") { SegmentedMessage[2] = Shared.Data.Accounts.GetBalance(e.ChatMessage.UserId,Shared.IDType.Twitch).ToString(); }

                    Shared.Data.EventResponse Response = Shared.Data.Accounts.PayUser(e.ChatMessage.UserId.ToString(), Shared.IDType.Twitch, TheirID, TheirIDType, int.Parse(SegmentedMessage[2]));
                    //if (Response.Success)
                    //{
                    Bot.TwitchC.SendMessage(e.ChatMessage.Channel, "@"+e.ChatMessage.Username+Response.Message);
                    //}
                }

                if (Command == "owlcoin" || Command == "bal"||Command=="balance")
                {
                    if (SegmentedMessage.Length==2&&SegmentedMessage[1].StartsWith("@"))
                    {
                        SegmentedMessage[1] = SegmentedMessage[1].Replace("@", "");
                        String TheirID = UserHandler.UserFromUsername(SegmentedMessage[1]).Matches[0].Id;
                        Shared.Data.UserData.CreateUser(TheirID, Shared.IDType.Twitch);
                        Bot.TwitchC.SendMessage(e.ChatMessage.Channel,"@"+e.ChatMessage.Username+" @"+ SegmentedMessage[1] + " has "+Shared.Data.Accounts.GetBalance(TheirID, Shared.IDType.Twitch) + " Owlcoin!");
                    }
                    Shared.Data.UserData.CreateUser(e.ChatMessage.UserId, Shared.IDType.Twitch);
                    Bot.TwitchC.SendMessage(e.ChatMessage.Channel, "@" + e.ChatMessage.Username + " you have " + Shared.Data.Accounts.GetBalance(e.ChatMessage.UserId, Shared.IDType.Twitch)+" Owlcoin!");
                }

                if (Command == "gamble" || Command == "roulette")
                {
                    if (SegmentedMessage.Length != 2) { NotLongEnough(e); return; }
                    int coins, amount;
                    coins = amount = Shared.Data.Accounts.GetBalance(e.ChatMessage.UserId.ToString(), Shared.IDType.Twitch);
                    if (SegmentedMessage[1] != "all")
                    {
                        if (!int.TryParse(SegmentedMessage[1], out amount)) { InvalidParameter(e); return; }
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

        static async void NotLongEnough(OnMessageReceivedArgs e)
        {
            Bot.TwitchC.SendMessage(e.ChatMessage.Channel, "You are missing parameters or have too many!");
        }

        static async void InvalidParameter(OnMessageReceivedArgs e)
        {
            Bot.TwitchC.SendMessage(e.ChatMessage.Channel, "You have one or more invalid parameter!");
        }

    }
}
