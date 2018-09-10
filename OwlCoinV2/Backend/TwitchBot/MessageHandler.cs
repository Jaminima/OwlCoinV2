﻿using System;
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

                //if (Command == "echo")
                //{
                //    if (SegmentedMessage.Length < 2) { NotLongEnough(e); return; }
                //    Bot.TwitchC.SendMessage(e.ChatMessage.Channel, e.ChatMessage.Message.Remove(0, Command.Length + Prefix.Length + 1));
                //}

                if (Command == "join")
                {
                    Commands.Viewer.Commands.JoinRaffle(e);
                }

                if (Command == "pay")
                {
                    Commands.Viewer.Commands.Pay(e, SegmentedMessage);
                }

                if (Command == "owlcoin" || Command == "bal"||Command=="balance")
                {
                    Commands.Viewer.Commands.OwlCoin(e, SegmentedMessage);
                }

                if (Command == "r")
                {
                    Commands.Viewer.Commands.SongRequest(e, SegmentedMessage);
                }

                if (Command == "gamble" || Command == "roulette")
                {
                    Commands.Viewer.Commands.SongRequest(e, SegmentedMessage);
                }

            }

        }

        public static void NotLongEnough(OnMessageReceivedArgs e)
        {
            Bot.TwitchC.SendMessage(e.ChatMessage.Channel, "@"+e.ChatMessage.Username+" are missing parameters or have too many!");
        }

        public static void InvalidParameter(OnMessageReceivedArgs e)
        {
            Bot.TwitchC.SendMessage(e.ChatMessage.Channel, "@"+e.ChatMessage.Username+" have one or more invalid parameter!");
        }

    }
}
