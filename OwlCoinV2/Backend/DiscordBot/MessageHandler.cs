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

        public static async Task HandleMessage(SocketMessage Message)
        {
            //new Thread(async () => await Proccesor(Message)).Start();
            await Proccesor(Message);
            return;
        }

        public static async Task Proccesor(SocketMessage Message)
        {
            string[] SegmentedMessage = Message.Content.Split(" ".ToCharArray());
            string Command = SegmentedMessage[0].ToLower();
            string Prefix = Shared.ConfigHandler.Config["Prefix"].ToString();

            Shared.Data.EventResponse CreateUser = Shared.Data.UserData.CreateUser(Message.Author.Id.ToString(), Shared.IDType.Discord);
            Shared.Data.UserData.MergeAccounts(Message.Author.Id.ToString());

            await AwardForInteraction(Message);
            if (Command.StartsWith(Prefix))
            {

                Command = Command.Remove(0, Prefix.Length);

                //if (Command == "echo")
                //{
                //    if (SegmentedMessage.Length < 2) { NotLongEnough(Message); return; }
                //    await Message.Channel.SendMessageAsync(Message.Content.Remove(0, Command.Length + Prefix.Length + 1));
                //}

                //if (Command == "join") { await AudioBot.Bot.JoinVoice(Message); }
                //if (Command == "play") {
                //    await AudioBot.Bot.PlaySong(Message); }

                if (Command == "notifications" || Command == "notification")
                {
                    await Commands.Viewer.Commands.Notifications(Message, SegmentedMessage);
                }

                if (Message.Channel.GetType() == typeof(SocketDMChannel)) { return; }

                if (Newtonsoft.Json.Linq.JObject.Parse(Shared.ConfigHandler.Config["CommandResponses"]["SimpleEcho"].ToString()).ContainsKey(Command))
                {
                    await SendMessage(Message, Shared.ConfigHandler.Config["CommandResponses"]["SimpleEcho"][Command].ToString());
                }

                if (Command == "pay" || Command == "give" + Shared.ConfigHandler.Config["CurrencyName"].ToString().ToLower())
                {
                    await Commands.Viewer.Commands.Pay(Message, SegmentedMessage);
                }

                if (Command == Shared.ConfigHandler.Config["CurrencyName"].ToString().ToLower() || Command == "bal" || Command == "balance" || Command == Shared.ConfigHandler.Config["CurrencyAbreviation"].ToString().ToLower())
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

                if (Command == "refresh" || Command == "refreshconfig")
                {
                    await Commands.Moderator.Commands.RefreshConfig(Message, SegmentedMessage);
                }

                if (Command == "help")
                {
                    await Commands.Viewer.Commands.Help(Message, SegmentedMessage);
                }
            }
        }
        static List<string[]> UserInteraction = new List<string[]> { };

        static async Task AwardForInteraction(SocketMessage Message)
        {
            foreach (string[] Pair in UserInteraction)
            {
                if (Pair[0] == Message.Author.Id.ToString())
                {
                    int MinsSince = (int)(int)(TimeSpan.FromTicks(DateTime.Now.Ticks - long.Parse(Pair[1])).TotalMinutes); ;
                    if (MinsSince > 10)
                    {
                        Shared.Data.Accounts.GiveUser(Message.Author.Id.ToString(), Shared.IDType.Discord, int.Parse(Shared.ConfigHandler.Config["Rewards"]["Discord"]["Chatting"].ToString()));
                        UserInteraction.Remove(Pair);
                        UserInteraction.Add(new string[] { Message.Author.Id.ToString(), DateTime.Now.Ticks.ToString() });
                    }
                    return;
                }
            }
            UserInteraction.Add(new string[] { Message.Author.Id.ToString(), DateTime.Now.Ticks.ToString() });
            Shared.Data.Accounts.GiveUser(Message.Author.Id.ToString(), Shared.IDType.Discord, 300);
        }

        public static async void NotLongEnough(SocketMessage Message)
        {
            await SendMessage(Message, Shared.ConfigHandler.Config["CommandResponses"]["Errors"]["MissingParameters"].ToString());
        }

        public static async void InvalidParameter(SocketMessage Message)
        {
            await SendMessage(Message, Shared.ConfigHandler.Config["CommandResponses"]["Errors"]["InvalidParameters"].ToString());
        }

        public static string GetDiscordID(string message)
        {
            if (message.StartsWith("<@"))
            {
                message = message.Replace("<@", "").Replace(">", "").Replace("!", "");
            }
            return message;
        }

        public static async Task SendMessage(SocketMessage e,string Message, string TargetUserID = null, int Amount = -1, int NewBal = -1, string OtherString = "")
        {
            await e.Channel.SendMessageAsync(ParseConfigString(Message,e.Author, TargetUserID, Amount, NewBal, OtherString));
        }

        public static string ParseConfigString(string ConfString, SocketUser e, string TargetUserID = null, int Amount = -1, int NewBal = -1, string OtherString = "")
        {
            ConfString = ConfString.Replace("/me", "");
            ConfString = ConfString.Replace("@<OtherString>", OtherString);
            if (e != null) { ConfString = ConfString.Replace("@<SenderUser>", "<@" + e.Id+">"); }
            ConfString = ConfString.Replace("@<CurrencyName>", Shared.ConfigHandler.Config["CurrencyName"].ToString());
            ConfString = ConfString.Replace("@<TargetUser>", "<@" + TargetUserID+">");
            ConfString = ConfString.Replace("@<Amount>", Amount.ToString("N0"));
            ConfString = ConfString.Replace("@<NewBalance>", NewBal.ToString("N0"));
            ConfString = ConfString.Replace("@<Prefix>", Shared.ConfigHandler.Config["Prefix"].ToString());
            return ConfString;
        }

    }
}
