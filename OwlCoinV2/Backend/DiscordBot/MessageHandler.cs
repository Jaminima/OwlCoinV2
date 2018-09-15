using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Discord.WebSocket;

namespace OwlCoinV2.Backend.DiscordBot
{
    public static class MessageHandler
    {

        public static Task HandleMessage(SocketMessage Message)
        {
            //new Thread(async () => await Proccesor(Message)).Start();
            Proccesor(Message);
            return null;
        }

        public static async Task Proccesor(SocketMessage Message)
        {
            string[] SegmentedMessage = Message.Content.Split(" ".ToCharArray());
            string Command = SegmentedMessage[0].ToLower();
            string Prefix = Shared.ConfigHandler.Config["Prefix"].ToString();
            if (Command.StartsWith(Prefix))
            {

                Shared.Data.UserData.CreateUser(Message.Author.Id.ToString(), Shared.IDType.Discord);
                Shared.Data.UserData.MergeAccounts(Message.Author.Id.ToString());

                Command = Command.Remove(0, Prefix.Length);

                //if (Command == "echo")
                //{
                //    if (SegmentedMessage.Length < 2) { NotLongEnough(Message); return; }
                //    await Message.Channel.SendMessageAsync(Message.Content.Remove(0, Command.Length + Prefix.Length + 1));
                //}

                if (Command == "pay" || Command == "giveowlcoin")
                {
                    await Commands.Viewer.Commands.Pay(Message, SegmentedMessage);
                }

                if (Command == "owlcoin" || Command == "bal" || Command == "balance")
                {
                    await Commands.Viewer.Commands.OwlCoin(Message, SegmentedMessage);
                }

                if (Command == "gamble" || Command == "roulette")
                {
                    await Commands.Viewer.Commands.Roulette(Message, SegmentedMessage);
                }

                if (Command == "duel")
                {
                    await Commands.Viewer.Duel.StartDuel(Message, SegmentedMessage);
                }

                if (Command == "accept")
                {
                    await Commands.Viewer.Duel.Accept(Message, SegmentedMessage);
                }

                if (Command == "deny")
                {
                    await Commands.Viewer.Duel.Deny(Message, SegmentedMessage);
                }

                if (Command == "slots")
                {
                    await Commands.Viewer.Commands.Slots(Message, SegmentedMessage);
                }

                if (Command == "give")
                {
                    await Commands.Moderator.Commands.GivePoints(Message, SegmentedMessage);
                }

                if (Command == "help")
                {
                    await Commands.Viewer.Commands.Help(Message, SegmentedMessage);
                }

            }

            foreach (string[] Pair in UserInteraction)
            {
                if (Pair[0] == Message.Author.Id.ToString())
                {
                    int MinsSince = (int)(int)(TimeSpan.FromTicks(DateTime.Now.Ticks - long.Parse(Pair[1])).TotalMinutes); ;
                    if ( MinsSince < 10)
                    {
                        Shared.Data.Accounts.GiveUser(Message.Author.Id.ToString(), Shared.IDType.Discord, 300);
                        UserInteraction.Remove(Pair);
                        UserInteraction.Add(new string[] { Message.Author.Id.ToString(), DateTime.Now.Ticks.ToString() });
                    }
                    return;
                }
            }
            UserInteraction.Add(new string[] { Message.Author.Id.ToString(), DateTime.Now.Ticks.ToString() });
            Shared.Data.Accounts.GiveUser(Message.Author.Id.ToString(), Shared.IDType.Discord, 300);

        }
        static List<string[]> UserInteraction = new List<string[]> { };

        public static async void NotLongEnough(SocketMessage Message)
        {
            await Message.Channel.SendMessageAsync("<@" + Message.Author.Id + ">, you are missing parameters or have too many!");
        }

        public static async void InvalidParameter(SocketMessage Message)
        {
            await Message.Channel.SendMessageAsync("<@" + Message.Author.Id + ">, you have one or more invalid parameter!");
        }

        public static string GetDiscordID(string message)
        {
            if (message.StartsWith("<@"))
            {
                message = message.Replace("<@", "").Replace(">", "").Replace("!", "");
            }
            return message;
        }
    }
}
