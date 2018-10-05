using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwitchLib.Client;
using TwitchLib.Client.Models;
using TwitchLib.Client.Events;
using System.Threading;

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
            if (TheirID.StartsWith("@")) { TheirID = TheirID.Replace("@", "");
                try { TheirID = UserHandler.UserFromUsername(TheirID).Matches[0].Id; }
                catch { MessageHandler.SendMessage(e,MessageHandler.ParseConfigString(Shared.ConfigHandler.Config["CommandResponses"]["Errors"]["NoUser"].ToString(),e.ChatMessage)); return; } }
            int Amount = 0;
            if (SegmentedMessage[2].ToLower() == "all") { Amount = Shared.Data.Accounts.GetBalance(e.ChatMessage.UserId.ToString(), Shared.IDType.Twitch); }
            else if (SegmentedMessage[2].ToLower().EndsWith("k"))
            {
                if (!int.TryParse(SegmentedMessage[2].ToLower().Replace("k", ""), out Amount)) { MessageHandler.InvalidParameter(e); return; }
                Amount *= 1000;
            }
            else { if (!int.TryParse(SegmentedMessage[2].ToLower(), out Amount)) { MessageHandler.InvalidParameter(e); return; } }
            if (Amount < 0) { MessageHandler.NegativeValue(e); return; }
            
            Shared.Data.EventResponse Response = Shared.Data.Accounts.PayUser(e.ChatMessage.UserId.ToString(), Shared.IDType.Twitch, TheirID, TheirIDType, Amount);

            if (Response.Success) { MessageHandler.SendMessage(e, MessageHandler.ParseConfigString(Shared.ConfigHandler.Config["CommandResponses"]["Pay"]["PaymentComplete"].ToString(),e.ChatMessage,null,Amount, Shared.Data.Accounts.GetBalance(e.ChatMessage.UserId.ToString(), Shared.IDType.Twitch))); }
            else { MessageHandler.SendMessage(e,MessageHandler.ParseConfigString(Response.Message,e.ChatMessage)); }
        }

        public static void OwlCoin(OnMessageReceivedArgs e, string[] SegmentedMessage)
        {
            if (SegmentedMessage.Length == 2 && SegmentedMessage[1].StartsWith("@"))
            {
                SegmentedMessage[1] = SegmentedMessage[1].Replace("@", "");
                String TheirID;
                try { TheirID = UserHandler.UserFromUsername(SegmentedMessage[1]).Matches[0].Id; } catch { Bot.TwitchC.SendMessage(e.ChatMessage.Channel, "@" + e.ChatMessage.Username + " That user doesnt exist!"); return; }
                Shared.Data.UserData.CreateUser(TheirID, Shared.IDType.Twitch);
                MessageHandler.SendMessage(e,MessageHandler.ParseConfigString(Shared.ConfigHandler.Config["CommandResponses"]["Bal"]["Other"].ToString(),e.ChatMessage,SegmentedMessage[1], Shared.Data.Accounts.GetBalance(TheirID, Shared.IDType.Twitch)));
                return;
            }
            else
            {
                Shared.Data.UserData.CreateUser(e.ChatMessage.UserId, Shared.IDType.Twitch);
                MessageHandler.SendMessage(e, MessageHandler.ParseConfigString(Shared.ConfigHandler.Config["CommandResponses"]["Bal"]["Self"].ToString(),e.ChatMessage,null, Shared.Data.Accounts.GetBalance(e.ChatMessage.UserId, Shared.IDType.Twitch)));
            }
        }

        public static void SongRequest(OnMessageReceivedArgs e, string[] SegmentedMessage)
        {
            int MyBal = Shared.Data.Accounts.GetBalance(e.ChatMessage.UserId, Shared.IDType.Twitch);
            int Required = int.Parse(Shared.ConfigHandler.Config["Songs"]["Cost"]["Viewer"].ToString());
            if (e.ChatMessage.IsSubscriber) { Required = int.Parse(Shared.ConfigHandler.Config["Songs"]["Cost"]["Subscriber"].ToString()); }
            if (MyBal >= Required)
            {
                MyBal -= Required;
                Shared.Data.Accounts.TakeUser(e.ChatMessage.UserId, Shared.IDType.Twitch, Required);
                Bot.TwitchC.SendMessage(e.ChatMessage.Channel, "!sr" + e.ChatMessage.Message.Remove(0,Shared.ConfigHandler.Config["Prefix"].ToString().Length+1));
            }
            else { MessageHandler.SendMessage(e,MessageHandler.ParseConfigString(Shared.ConfigHandler.Config["CommandResponses"]["Errors"]["NotEnough"].ToString(),e.ChatMessage)); }
        }

        public static void Roulette(OnMessageReceivedArgs e, string[] SegmentedMessage)
        {
            int MinBet = int.Parse(Shared.ConfigHandler.Config["Roulette"]["MinBet"].ToString());
            if (SegmentedMessage.Length != 2) { MessageHandler.NotLongEnough(e); return; }
            int coins, amount;
            coins = amount = Shared.Data.Accounts.GetBalance(e.ChatMessage.UserId.ToString(), Shared.IDType.Twitch);
            if (SegmentedMessage[1].ToLower().EndsWith("k"))
            {
                if (!int.TryParse(SegmentedMessage[1].ToLower().Replace("k", ""), out amount)) { MessageHandler.InvalidParameter(e); return; }
                amount *= 1000;
            }
            else if (SegmentedMessage[1] != "all")
            {
                if (!int.TryParse(SegmentedMessage[1], out amount)) { MessageHandler.InvalidParameter(e); return; }
            }
            amount = Math.Abs(amount);
            if (amount < MinBet) {
                MessageHandler.SendMessage(e, MessageHandler.ParseConfigString(Shared.ConfigHandler.Config["CommandResponses"]["Errors"]["BetTooLow"].ToString(), e.ChatMessage,null, MinBet));
                return;
            }
            if (amount <= coins)
            {
                if (random.Next(100) < int.Parse(Shared.ConfigHandler.Config["GambleWinChance"].ToString()))
                {
                    Shared.Data.Accounts.GiveUser(e.ChatMessage.UserId.ToString(), Shared.IDType.Twitch, amount);
                    MessageHandler.SendMessage(e,MessageHandler.ParseConfigString(Shared.ConfigHandler.Config["CommandResponses"]["Roulette"]["Win"].ToString(),e.ChatMessage,null,amount));
                }
                else
                {
                    Shared.Data.Accounts.TakeUser(e.ChatMessage.UserId.ToString(), Shared.IDType.Twitch, amount);
                    MessageHandler.SendMessage(e, MessageHandler.ParseConfigString(Shared.ConfigHandler.Config["CommandResponses"]["Roulette"]["Lose"].ToString(), e.ChatMessage, null, amount));
                }
            }
            else
            {
                MessageHandler.SendMessage(e, MessageHandler.ParseConfigString(Shared.ConfigHandler.Config["CommandResponses"]["Errors"]["NotEnough"].ToString(), e.ChatMessage));
            }
        }

        public static void Help(OnMessageReceivedArgs e, string[] SegmentedMessage)
        {
            if (e.ChatMessage.IsBroadcaster || e.ChatMessage.IsModerator)
            {
                MessageHandler.SendMessage(e, MessageHandler.ParseConfigString(Shared.ConfigHandler.Config["CommandResponses"]["Help"]["Moderator"].ToString(), e.ChatMessage));
            }
            MessageHandler.SendMessage(e, MessageHandler.ParseConfigString(Shared.ConfigHandler.Config["CommandResponses"]["Help"]["Viewer"].ToString(), e.ChatMessage));
        }

        public static void AccountAge(OnMessageReceivedArgs e, string[] SegmentedMessage)
        {
            DateTime CreatedAt;
            if (SegmentedMessage.Length == 2 && SegmentedMessage[1].StartsWith("@"))
            {
                SegmentedMessage[1] = SegmentedMessage[1].Replace("@", "");
                try { CreatedAt = UserHandler.UserFromUsername(SegmentedMessage[1]).Matches[0].CreatedAt; }
                catch {MessageHandler.SendMessage(e,MessageHandler.ParseConfigString(Shared.ConfigHandler.Config["CommandResponses"]["Errors"]["NoUser"].ToString(),e.ChatMessage)); return; }
                MessageHandler.SendMessage(e,MessageHandler.ParseConfigString(Shared.ConfigHandler.Config["CommandResponses"]["Age"]["Other"].ToString(),e.ChatMessage,SegmentedMessage[1],-1,-1,AgeString(CreatedAt)));
                return;
            }
            CreatedAt = UserHandler.UserFromUserID(e.ChatMessage.UserId).CreatedAt;
            MessageHandler.SendMessage(e, MessageHandler.ParseConfigString(Shared.ConfigHandler.Config["CommandResponses"]["Age"]["Self"].ToString(), e.ChatMessage,null,-1,-1, AgeString(CreatedAt)));
        }

        static string AgeString(DateTime CreatedAt)
        {
            TimeSpan Span = DateTime.Now - CreatedAt;
            string Age = "";
            int Years = (int)Math.Floor((decimal)Span.Days / 365);
            int Months = (int)Math.Floor((decimal)(Span.Days - (Years * 365)) / 30);
            int Days = Span.Days - ((Years * 365) + (Months * 30));

            if (Years != 0) { if (Years == 1) { Age += Years + " Year "; } else { Age += Years + " Years "; } }if (Months != 0 && Days == 0 && Span.Hours == 0) { Age += "and "; }
            if (Months != 0) { if (Months == 1) { Age += Months + " Month "; } else { Age += Months + " Months "; } } if (Days != 0 && Span.Hours == 0) { Age += "and "; }
            if (Days != 0) { if (Days == 1) { Age += Days + " Day "; } else { Age += Days + " Days "; } }
            if (Span.Hours != 0) { if (Span.Hours == 1) { Age +="and "+ Span.Hours + " Hour "; } else { Age +="and "+ Span.Hours + " Hours "; } }

            return Age;
        }

        static List<string[]> LastRequested = new List<string[]> { };
        static string LastReq = "0";

        public static void RequestAlert(OnMessageReceivedArgs e, string[] SegmentedMessage)
        {
            if (SegmentedMessage.Length < 2) { MessageHandler.NotLongEnough(e); return; }
            foreach(string[] Pair in LastRequested)
            {
                if (Pair[0] == e.ChatMessage.UserId)
                {
                    int MinsLeft = 10-(int)(TimeSpan.FromTicks(DateTime.Now.Ticks - long.Parse(Pair[1])).TotalMinutes);
                    if (MinsLeft>0)
                    {
                        string Mins = "min";
                        if (MinsLeft != 1) { Mins = Mins + "s"; }
                        MessageHandler.SendMessage(e, MessageHandler.ParseConfigString(Shared.ConfigHandler.Config["CommandResponses"]["Errors"]["TooFast"].ToString(), e.ChatMessage,null,MinsLeft));
                        return;
                    }
                    LastRequested.Remove(Pair);
                    break;
                }
            }
            string SearchFor = e.ChatMessage.Message.Replace(SegmentedMessage[0]+" ","");
            int AlertID=0;
            string Determinant = "";
            foreach(string S in SearchFor.ToLower().Split(" ".ToCharArray())) { Determinant = Determinant + "(Name LIKE \"%" + S + "%\") OR"; }
            Determinant = Determinant.Remove(Determinant.Length - 2);
            try { AlertID = int.Parse(Init.SQLInstance.Select("Alerts", "AlertID", ""+Determinant+"")[0]); }
            catch (Exception E) { Console.WriteLine(E); Bot.TwitchC.SendMessage(e.ChatMessage.Channel, "@" + e.ChatMessage.Username + " unable to find Alert " + SearchFor); return; }
            string ImageURL = Init.SQLInstance.Select("Alerts", "ImageUrl", "AlertID=" + AlertID)[0],
                SoundURL = Init.SQLInstance.Select("Alerts", "SoundUrl", "AlertID=" + AlertID)[0];
            int Cost = int.Parse(Init.SQLInstance.Select("Alerts", "Cost", "AlertID=" + AlertID)[0]);
            int TSinceLast = (int)(TimeSpan.FromTicks(DateTime.Now.Ticks - long.Parse(LastReq)).TotalSeconds);
            if (TSinceLast < 120&&LastReq!="0") { MessageHandler.SendMessage(e, MessageHandler.ParseConfigString(Shared.ConfigHandler.Config["CommandResponses"]["Errors"]["TooFast"].ToString(), e.ChatMessage, null, (120 - TSinceLast))); return; }
            if (Shared.Data.Accounts.TakeUser(e.ChatMessage.UserId, Shared.IDType.Twitch, Cost))
            {
                if (Streamlabs.Alert.SendRequest(ImageURL, SoundURL))
                {
                    Bot.TwitchC.SendMessage(e.ChatMessage.Channel, "@" + e.ChatMessage.Username + " Alert sent!");
                    LastReq = DateTime.Now.Ticks.ToString();
                    LastRequested.Add(new string[] { e.ChatMessage.UserId, DateTime.Now.Ticks.ToString() });
                }
                else { Bot.TwitchC.SendMessage(e.ChatMessage.Channel, "@" + e.ChatMessage.Username + " Alert failed to send! Please try again soon!"); }
            }
            else { MessageHandler.SendMessage(e, MessageHandler.ParseConfigString(Shared.ConfigHandler.Config["CommandResponses"]["Errors"]["NotEnough"].ToString(), e.ChatMessage)); }
        }

        public static void Slots(OnMessageReceivedArgs e,string[] SegmentedMessage)
        {
            int MinBet = int.Parse(Shared.ConfigHandler.Config["Slots"]["MinBet"].ToString());
            if (SegmentedMessage.Length != 2) { MessageHandler.NotLongEnough(e); return; }
            int coins, amount;
            coins = amount = Shared.Data.Accounts.GetBalance(e.ChatMessage.UserId.ToString(), Shared.IDType.Twitch);
            if (SegmentedMessage[1].ToLower().EndsWith("k"))
            {
                if (!int.TryParse(SegmentedMessage[1].ToLower().Replace("k", ""), out amount)) { MessageHandler.InvalidParameter(e); return; }
                amount *= 1000;
            }
            else if (SegmentedMessage[1].ToLower() != "all")
            {
                if (!int.TryParse(SegmentedMessage[1], out amount)) { MessageHandler.InvalidParameter(e); return; }
            }
            amount = Math.Abs(amount);
            if (amount < MinBet) { MessageHandler.SendMessage(e, MessageHandler.ParseConfigString(Shared.ConfigHandler.Config["CommandResponses"]["Errors"]["BetTooLow"].ToString(), e.ChatMessage, null, MinBet)); return; }
            if (amount <= coins)
            {
                string[] emotes = Shared.ConfigHandler.Config["Slots"]["Twitch"].Select(r => r.ToString()).ToArray();
                int roll = random.Next(0,100);
                int combo = random.Next(2);
                if (roll < 10)
                {
                    amount *= 4;
                    combo = 2;
                }
                if (roll < 10)
                {
                    Shared.Data.Accounts.GiveUser(e.ChatMessage.UserId.ToString(), Shared.IDType.Twitch, amount);
                    MessageHandler.SendMessage(e, MessageHandler.ParseConfigString(Shared.ConfigHandler.Config["CommandResponses"]["Slots"]["Win"].ToString(), e.ChatMessage, null, amount,-1,"[ " + emotes[combo] + " | " + emotes[combo] + " | " + emotes[combo] + " ]"));
                }
                else
                {
                    combo = Enumerable.Range(1, 25).Where(x => x != 13).ElementAt(random.Next(24));
                    Shared.Data.Accounts.TakeUser(e.ChatMessage.UserId.ToString(), Shared.IDType.Twitch, amount);
                    MessageHandler.SendMessage(e, MessageHandler.ParseConfigString(Shared.ConfigHandler.Config["CommandResponses"]["Slots"]["Lose"].ToString(), e.ChatMessage, null, amount, -1, "[ " + emotes[combo / 9] + " | " + emotes[(combo / 3) % 3] + " | " + emotes[combo % 3] + " ]"));
                }
            }
            else
            {
                MessageHandler.SendMessage(e, MessageHandler.ParseConfigString(Shared.ConfigHandler.Config["CommandResponses"]["Errors"]["NotEnough"].ToString(), e.ChatMessage));
            }
        }

        public static List<String> Fishermen = new List<string> { };
        public static void Fish(OnMessageReceivedArgs e, string[] SegmentedMessage)
        {
            if (!Drops.IsLive()) { return; }
            if (SegmentedMessage.Length == 1)
            {
                int Cost= int.Parse(Shared.ConfigHandler.Config["Fish"]["Cost"]["Viewer"].ToString());
                if (e.ChatMessage.IsSubscriber) { Cost = int.Parse(Shared.ConfigHandler.Config["Fish"]["Cost"]["Subscriber"].ToString()); }
                if (Fishermen.Contains(e.ChatMessage.UserId))
                { MessageHandler.SendMessage(e, Shared.ConfigHandler.Config["CommandResponses"]["Fish"]["AlreadyFishing"].ToString(), null); return; }
                if (Shared.Data.Accounts.GetBalance(e.ChatMessage.UserId, Shared.IDType.Twitch) < Cost)
                { MessageHandler.SendMessage(e, Shared.ConfigHandler.Config["CommandResponses"]["Errors"]["NotEnough"].ToString(), null); return; }
                int TotalChance = 0;
                foreach (Newtonsoft.Json.Linq.JToken Item in Shared.ConfigHandler.Config["Fish"]["Items"])
                { TotalChance += int.Parse(Item["Chance"].ToString()); }
                int ChosenI = random.Next(0, TotalChance+1); Newtonsoft.Json.Linq.JToken ChosenItem=new Newtonsoft.Json.Linq.JObject();
                foreach (Newtonsoft.Json.Linq.JToken Item in Shared.ConfigHandler.Config["Fish"]["Items"])
                {
                    ChosenI-= int.Parse(Item["Chance"].ToString());
                    if (ChosenI <= 0) { ChosenItem = Item; break; }
                }
                int MinTime = int.Parse(Shared.ConfigHandler.Config["Fish"]["MinTime"].ToString());
                int MaxTime = int.Parse(Shared.ConfigHandler.Config["Fish"]["MaxTime"].ToString());
                int WaitTime = random.Next(MinTime,MaxTime);
                MessageHandler.SendMessage(e, Shared.ConfigHandler.Config["CommandResponses"]["Fish"]["GoneFishing"].ToString(),null);
                Fishermen.Add(e.ChatMessage.UserId);
                Shared.Data.Accounts.TakeUser(e.ChatMessage.UserId, Shared.IDType.Twitch,Cost);
                new Thread(() => Fishing(e, WaitTime, ChosenItem)).Start();
            }
        }

        static void Fishing(OnMessageReceivedArgs e,int WaitTime,Newtonsoft.Json.Linq.JToken Item)
        {
            System.Threading.Thread.Sleep(WaitTime * 60000);
            int Reward = int.Parse(Item["Reward"].ToString());
            Shared.Data.Accounts.GiveUser(e.ChatMessage.UserId, Shared.IDType.Twitch, Reward);
            MessageHandler.SendMessage(e, Shared.ConfigHandler.Config["CommandResponses"]["Fish"]["Fished"].ToString(), null, Reward, -1, Item["Name"].ToString());
            Fishermen.Remove(e.ChatMessage.UserId);
        }

    }
}
