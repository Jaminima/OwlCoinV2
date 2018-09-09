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
                Console.WriteLine(e.ChatMessage.IsSubscriber);

                Command = Command.Remove(0, Prefix.Length);

                if (Command == "echo")
                {
                    Bot.TwitchC.SendMessage(e.ChatMessage.Channel, e.ChatMessage.Message.Remove(0, Command.Length + Prefix.Length + 1));
                }
                if (Command == "owlcoin" || Command == "bal"||Command=="balance")
                {
                    if (SegmentedMessage.Length==2&&SegmentedMessage[1].StartsWith("@"))
                    {
                        SegmentedMessage[1] = SegmentedMessage[1].Replace("@", "");
                        Shared.Data.UserData.CreateUser(SegmentedMessage[1], Shared.IDType.Twitch);
                        Bot.TwitchC.SendMessage(e.ChatMessage.Channel,"@"+e.ChatMessage.Username+" @"+SegmentedMessage[1]+" has "+Shared.Data.Accounts.GetBalance(SegmentedMessage[1], Shared.IDType.Twitch) + " Owlcoin!");
                    }
                    Bot.TwitchC.SendMessage(e.ChatMessage.Channel, "@" + e.ChatMessage.Username + " you have " + Shared.Data.Accounts.GetBalance(e.ChatMessage.UserId, Shared.IDType.Twitch)+" Owlcoin!");
                }

            }
        }

    }
}
