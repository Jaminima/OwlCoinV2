using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwitchLib.Client;
using TwitchLib.Client.Models;
using TwitchLib.Client.Events;
using System.Threading;

namespace OwlCoinV2.Backend.Twitch
{
    public static class MessageHandler
    {
        public static void HandleMessage(object sender, OnMessageReceivedArgs e)
        {
            new Thread(() => Proccessor(e)).Start();
        }

        static void Proccessor(OnMessageReceivedArgs e)
        {
            string Command = e.ChatMessage.Message.Split(" ".ToCharArray())[0].ToLower();
            string Prefix = Shared.ConfigHandler.Config["Prefix"].ToString();
            if (Command.StartsWith(Prefix))
            {
                Command = Command.Remove(0, Prefix.Length);

                if (Command == "echo")
                {
                    Bot.TwitchC.SendMessage(e.ChatMessage.Channel, e.ChatMessage.Message.Remove(0, Command.Length + Prefix.Length + 1));
                }

            }
        }

    }
}
