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

                if (Command == "echo")
                {
                    Bot.TwitchC.SendMessage(e.ChatMessage.Channel, e.ChatMessage.Message.Remove(0, Command.Length + Prefix.Length + 1));
                }

                if (Command == "pay")
                {
                    if (SegmentedMessage.Length != 3) { NotLongEnough(e); return; }
                    string TheirID = SegmentedMessage[1]; Shared.IDType TheirIDType = Shared.IDType.Twitch;
                    if (TheirID.StartsWith("@")) { TheirID = TheirID.Replace("@", ""); TheirID= UserHandler.UserFromUsername(TheirID).Matches[0].Id; }

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

            }

        }

        static async void NotLongEnough(OnMessageReceivedArgs e)
        {
            Bot.TwitchC.SendMessage(e.ChatMessage.Channel, "You are missing parameters or have too many!");
        }

    }
}
