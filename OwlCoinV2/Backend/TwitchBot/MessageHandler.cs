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

            Shared.Data.UserData.CreateUser(e.ChatMessage.UserId.ToString(), Shared.IDType.Twitch);

            AwardForInteraction(e);
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
                    return;
                }

                if (!CommandDelayPassed(e) && !e.ChatMessage.IsModerator && !e.ChatMessage.IsBroadcaster) { return; }

                if (Newtonsoft.Json.Linq.JObject.Parse(Shared.ConfigHandler.Config["CommandResponses"]["SimpleEcho"].ToString()).ContainsKey(Command))
                {
                    SendMessage(e, Shared.ConfigHandler.Config["CommandResponses"]["SimpleEcho"][Command].ToString(), null);
                }

                if (Command == "uptime")
                {
                    Commands.Viewer.Commands.Uptime(e, SegmentedMessage);
                }

                if (Command == "pay"||Command=="give"+Shared.ConfigHandler.Config["CurrencyName"].ToString().ToLower())
                {
                    Commands.Viewer.Commands.Pay(e, SegmentedMessage);
                }

                if (Command == Shared.ConfigHandler.Config["CurrencyName"].ToString().ToLower() || Command == "bal"||Command=="balance"||Command==Shared.ConfigHandler.Config["CurrencyAbreviation"].ToString().ToLower())
                {
                    Commands.Viewer.Commands.OwlCoin(e, SegmentedMessage);
                }

                if (Command == "r" || Command == "sr")
                {
                    Commands.Viewer.Commands.SongRequest(e, SegmentedMessage);
                }

                if (Command == "gamble" || Command == "roulette")
                {
                    Commands.Viewer.Commands.Roulette(e, SegmentedMessage);
                }

                if (Command == "give")
                {
                    Commands.Moderator.Commands.GivePoints(e,SegmentedMessage);
                }

                if (Command == "setgame")
                {
                    Commands.Moderator.Commands.SetGame(e, SegmentedMessage);
                }
                if (Command == "settitle")
                {
                    Commands.Moderator.Commands.SetTitle(e, SegmentedMessage);
                }

                if (Command == "songs"||Command=="song")
                {
                    Commands.Viewer.Songs.Proccessor(e, SegmentedMessage);
                }

                if (Command == "accountage" || Command == "age")
                {
                    Commands.Viewer.Commands.AccountAge(e, SegmentedMessage);
                }

                if (Command == "alert"||Command=="alerts"||Command=="redeem")
                {
                    Commands.Viewer.Commands.RequestAlert(e,SegmentedMessage);
                }

                if (Command == "slots")
                {
                    Commands.Viewer.Commands.Slots(e, SegmentedMessage);
                }

                if (Command == "duel")
                {
                    Commands.Viewer.Duel.StartDuel(e, SegmentedMessage);
                }

                if (Command == "accept")
                {
                    Commands.Viewer.Duel.Accept(e, SegmentedMessage);
                }

                if (Command == "deny")
                {
                    Commands.Viewer.Duel.Deny(e, SegmentedMessage);
                }

                if (Command == "fish")
                {
                    Commands.Viewer.Commands.Fish(e,SegmentedMessage);
                }

                if (Command == "refresh" || Command == "refreshconfig")
                {
                    Commands.Moderator.Commands.RefreshConfig(e, SegmentedMessage);
                }

                if (Command == "latestvid")
                {
                    SendMessage(e, Shared.ConfigHandler.Config["CommandResponses"]["LatestVideo"].ToString(), null, -1, -1, Shared.APIIntergrations.Youtube.LatestVid());
                }

                if (Command == "help")
                {
                    Commands.Viewer.Commands.Help(e, SegmentedMessage);
                }

                

            }
            else
            {
                //jccjaminima gave 100 Owlcoin to owlcoinbot PogChamp
                //if (e.ChatMessage.Message.EndsWith("Owlcoin to owlcoinbot PogChamp"))
                //{
                //    string ID = UserHandler.UserFromUsername(SegmentedMessage[0]).Matches[0].Id;
                //    Shared.Data.UserData.CreateUser(ID, Shared.IDType.Twitch);
                //    Shared.Data.Accounts.GiveUser(ID,Shared.IDType.Twitch,int.Parse(SegmentedMessage[2]));
                //    Bot.TwitchC.SendMessage(e.ChatMessage.Channel, "@" + SegmentedMessage[0] + " Added " + SegmentedMessage[2] + " Owlcoin to your OwlcoinV2 Account!");
                //}
            }

        }
        static List<string[]> UserInteraction = new List<string[]> { };
        static void AwardForInteraction(OnMessageReceivedArgs e)
        {
            foreach (string[] Pair in UserInteraction)
            {
                if (Pair[0] == e.ChatMessage.UserId)
                {
                    int MinsSince = (int)(int)(TimeSpan.FromTicks(DateTime.Now.Ticks - long.Parse(Pair[1])).TotalMinutes); ;
                    if (MinsSince > 10)
                    {
                        Shared.Data.Accounts.GiveUser(e.ChatMessage.UserId, Shared.IDType.Twitch, int.Parse(Shared.ConfigHandler.Config["Rewards"]["Twitch"]["Chatting"].ToString()));
                        UserInteraction.Remove(Pair);
                        UserInteraction.Add(new string[] { e.ChatMessage.UserId, DateTime.Now.Ticks.ToString() });
                    }
                    return;
                }
            }
            UserInteraction.Add(new string[] { e.ChatMessage.UserId, DateTime.Now.Ticks.ToString() });
            Shared.Data.Accounts.GiveUser( e.ChatMessage.UserId, Shared.IDType.Twitch, int.Parse(Shared.ConfigHandler.Config["Rewards"]["Twitch"]["Chatting"].ToString()));
        }

        static List<String[]> CommandInteraction = new List<string[]> { };
        static bool CommandDelayPassed(OnMessageReceivedArgs e)
        {
            foreach (string[] Pair in CommandInteraction)
            {
                if (Pair[0] == e.ChatMessage.UserId)
                {
                    TimeSpan T= TimeSpan.FromTicks(DateTime.Now.Ticks - long.Parse(Pair[1]));
                    if (T.TotalSeconds >= int.Parse(Shared.ConfigHandler.Config["MessageDelay"].ToString()))
                    {
                        CommandInteraction.Remove(Pair);
                        CommandInteraction.Add(new string[] { e.ChatMessage.UserId, DateTime.Now.Ticks.ToString() });
                        return true;
                    }
                    else { return false; }
                }
            }
            CommandInteraction.Add(new string[] { e.ChatMessage.UserId, DateTime.Now.Ticks.ToString() });
            return true;
        }

        public static void NotLongEnough(OnMessageReceivedArgs e)
        {
            SendMessage(e, Shared.ConfigHandler.Config["CommandResponses"]["Errors"]["MissingParameters"].ToString(), null);
        }

        public static void InvalidParameter(OnMessageReceivedArgs e)
        {
            SendMessage(e, Shared.ConfigHandler.Config["CommandResponses"]["Errors"]["InvalidParameters"].ToString(), null);
        }

        public static void NegativeValue(OnMessageReceivedArgs e)
        {
            SendMessage(e, Shared.ConfigHandler.Config["CommandResponses"]["Errors"]["NegativeValue"].ToString(), null);
        }

        public static void SendMessage(OnMessageReceivedArgs e,string Message)
        {
            Bot.TwitchC.SendMessage(e.ChatMessage.Channel, Message);
        }
        public static void SendMessage(OnMessageReceivedArgs e, string Message, string TargetUsername = null, int Amount = -1, int NewBal = -1, string OtherString = "")
        {
            Bot.TwitchC.SendMessage(e.ChatMessage.Channel,ParseConfigString(Message,e.ChatMessage,TargetUsername,Amount,NewBal,OtherString));
        }
        public static void SendMessage(string Channel, string Message,string SenderUserName, string TargetUsername = null, int Amount = -1, int NewBal = -1, string OtherString = "")
        {
            Bot.TwitchC.SendMessage(Channel, ParseConfigString(Message,SenderUserName, TargetUsername, Amount, NewBal,OtherString));
        }

        public static string ParseConfigString(string ConfString,ChatMessage e,string TargetUsername=null,int Amount=-1,int NewBal=-1,string OtherString="")
        {
            ConfString = ConfString.Replace("@<OtherString>", OtherString);
            if (e != null) { ConfString = ConfString.Replace("@<SenderUser>", "@" + e.Username); }
            ConfString = ConfString.Replace("@<CurrencyName>", Shared.ConfigHandler.Config["CurrencyName"].ToString());
            ConfString = ConfString.Replace("@<TargetUser>", "@" + TargetUsername);
            ConfString = ConfString.Replace("@<Amount>", Amount.ToString("N0"));
            ConfString = ConfString.Replace("@<NewBalance>", NewBal.ToString("N0"));
            ConfString = ConfString.Replace("@<Prefix>", Shared.ConfigHandler.Config["Prefix"].ToString());
            return ConfString;
        }
        public static string ParseConfigString(string ConfString, string SenderUsername, string TargetUsername = null, int Amount = -1, int NewBal = -1, string OtherString = "")
        {
            ConfString = ConfString.Replace("@<OtherString>", OtherString);
            ConfString = ConfString.Replace("@<SenderUser>", "@" + SenderUsername);
            ConfString = ConfString.Replace("@<CurrencyName>", Shared.ConfigHandler.Config["CurrencyName"].ToString());
            ConfString = ConfString.Replace("@<TargetUser>", "@" + TargetUsername);
            ConfString = ConfString.Replace("@<Amount>", Amount.ToString("N0"));
            ConfString = ConfString.Replace("@<NewBalance>", NewBal.ToString("N0"));
            ConfString = ConfString.Replace("@<Prefix>", Shared.ConfigHandler.Config["Prefix"].ToString());
            return ConfString;
        }

    }
}
