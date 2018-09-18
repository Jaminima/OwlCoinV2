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
            if (TheirID.StartsWith("@")) { TheirID = TheirID.Replace("@", "");
                try { TheirID = UserHandler.UserFromUsername(TheirID).Matches[0].Id; }
                catch { Bot.TwitchC.SendMessage(e.ChatMessage.Channel, "@" + e.ChatMessage.Username + " That user doesnt exist!"); return; } }
            if (Shared.InputVerification.ContainsLetter(SegmentedMessage[2])&&SegmentedMessage[2].ToLower()!="all"){ MessageHandler.InvalidParameter(e); return; }
            else if (SegmentedMessage[2].ToLower()=="all"){ SegmentedMessage[2] = Shared.Data.Accounts.GetBalance(e.ChatMessage.UserId.ToString(), Shared.IDType.Twitch).ToString(); }
            int N = int.Parse(SegmentedMessage[2]);
            if (N < 0) { MessageHandler.NegativeValue(e); return; }


            Shared.Data.EventResponse Response = Shared.Data.Accounts.PayUser(e.ChatMessage.UserId.ToString(), Shared.IDType.Twitch, TheirID, TheirIDType, N);
            //if (Response.Success)
            //{
            Bot.TwitchC.SendMessage(e.ChatMessage.Channel, "@" + e.ChatMessage.Username + " " + Response.Message);
            //}
        }

        public static void OwlCoin(OnMessageReceivedArgs e, string[] SegmentedMessage)
        {
            if (SegmentedMessage.Length == 2 && SegmentedMessage[1].StartsWith("@"))
            {
                SegmentedMessage[1] = SegmentedMessage[1].Replace("@", "");
                String TheirID;
                try { TheirID = UserHandler.UserFromUsername(SegmentedMessage[1]).Matches[0].Id; } catch { Bot.TwitchC.SendMessage(e.ChatMessage.Channel, "@" + e.ChatMessage.Username + " That user doesnt exist!"); return; }
                Shared.Data.UserData.CreateUser(TheirID, Shared.IDType.Twitch);
                Bot.TwitchC.SendMessage(e.ChatMessage.Channel, "@" + e.ChatMessage.Username + " @" + SegmentedMessage[1] + " has " + Shared.Data.Accounts.GetBalance(TheirID, Shared.IDType.Twitch) + " Owlcoin!");
                return;
            }
            else
            {
                Shared.Data.UserData.CreateUser(e.ChatMessage.UserId, Shared.IDType.Twitch);
                Bot.TwitchC.SendMessage(e.ChatMessage.Channel, "@" + e.ChatMessage.Username + " you have " + Shared.Data.Accounts.GetBalance(e.ChatMessage.UserId, Shared.IDType.Twitch) + " Owlcoin!");
            }
        }

        public static void SongRequest(OnMessageReceivedArgs e, string[] SegmentedMessage)
        {
            int MyBal = Shared.Data.Accounts.GetBalance(e.ChatMessage.UserId, Shared.IDType.Twitch);
            int Required = 500;
            if (e.ChatMessage.IsSubscriber) { Required = 250; }
            if (MyBal >= Required)
            {
                MyBal -= Required;
                Shared.Data.Accounts.TakeUser(e.ChatMessage.UserId, Shared.IDType.Twitch, Required);
                Bot.TwitchC.SendMessage(e.ChatMessage.Channel, "!sr" + e.ChatMessage.Message.Remove(0,Shared.ConfigHandler.Config["Prefix"].ToString().Length+1));
                //Bot.TwitchC.SendMessage(e.ChatMessage.Channel, "@" + e.ChatMessage.Username + " song requested!");
            }
            else { Bot.TwitchC.SendMessage(e.ChatMessage.Channel, "@" + e.ChatMessage.Username + ", you need " + Required + " Owlcoins to request a song!"); }
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
            amount = Math.Abs(amount);
            if (amount < 100) { Bot.TwitchC.SendMessage(e.ChatMessage.Channel, "@" + e.ChatMessage.Username + " Minimum bet is 100 owlcoin!"); return; }
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

        public static void Help(OnMessageReceivedArgs e, string[] SegmentedMessage)
        {
            Bot.TwitchC.SendMessage(e.ChatMessage.Channel, "@" + e.ChatMessage.Username + " Commands are available here: https://pastebin.com/H60Ydn1s");
        }

        public static void AccountAge(OnMessageReceivedArgs e, string[] SegmentedMessage)
        {
            DateTime CreatedAt;
            if (SegmentedMessage.Length == 2 && SegmentedMessage[1].StartsWith("@"))
            {
                SegmentedMessage[1] = SegmentedMessage[1].Replace("@", "");
                CreatedAt = UserHandler.UserFromUsername(SegmentedMessage[1]).Matches[0].CreatedAt;
                Bot.TwitchC.SendMessage(e.ChatMessage.Channel, "@" + e.ChatMessage.Username + " @" + SegmentedMessage[1] + " account is " + AgeString(CreatedAt) + " old!");
                return;
            }
            CreatedAt = UserHandler.UserFromUserID(e.ChatMessage.UserId).CreatedAt;
            Bot.TwitchC.SendMessage(e.ChatMessage.Channel, "@" + e.ChatMessage.Username + " your account is " + AgeString(CreatedAt) + " old!");
        }

        static string AgeString(DateTime CreatedAt)
        {
            string Age = "";
            int Years = DateTime.Now.Year - CreatedAt.Year;
            int Months = DateTime.Now.Month - CreatedAt.Month;
            int Days = DateTime.Now.Day - CreatedAt.Day;
            int Hours = DateTime.Now.Hour - CreatedAt.Hour;
            if (Hours < 0) { Hours += 23; Days -= 1; }

            if (Years != 0) { if (Years > 1) { Age = Age + Years + " Years "; } else { Age = Age + Years + " Year "; } }
            if (Months != 0) { if (Months > 1) { Age = Age + Months + " Months "; } else { Age = Age + Months + " Month "; } }
            if (Days != 0) { if (Days > 1) { Age = Age + Days + " Days "; } else { Age = Age + Days + " Day "; } }
            if (Age.Length != 0) { Age = Age + "and "; }
            if (Hours > 1) { Age = Age + Hours + " Hours(ish)"; } else { Age = Age + Hours + " Hour(ish)"; }

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
                        Bot.TwitchC.SendMessage(e.ChatMessage.Channel, "@" + e.ChatMessage.Username + " Cant send requests that quickly! Please wait "+(MinsLeft)+" "+Mins+" before trying again.");
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
            if (TSinceLast < 120&&LastReq!="0") { Bot.TwitchC.SendMessage(e.ChatMessage.Channel, "@" + e.ChatMessage.Username + " Cant send another alert so soon! Please wait "+(120-TSinceLast)+" Seconds before requesting again."); return; }
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
            else { Bot.TwitchC.SendMessage(e.ChatMessage.Channel, "@" + e.ChatMessage.Username + " Not enough owlcoin!"); }
        }

        public static void Slots(OnMessageReceivedArgs e,string[] SegmentedMessage)
        {
            if (SegmentedMessage.Length != 2) { MessageHandler.NotLongEnough(e); return; }
            int coins, amount;
            coins = amount = Shared.Data.Accounts.GetBalance(e.ChatMessage.UserId.ToString(), Shared.IDType.Twitch);
            if (SegmentedMessage[1].ToLower() != "all")
            {
                if (!int.TryParse(SegmentedMessage[1], out amount)) { MessageHandler.InvalidParameter(e); return; }
            }
            amount = Math.Abs(amount);
            if (amount < 100) { Bot.TwitchC.SendMessage(e.ChatMessage.Channel, "@" + e.ChatMessage.Username + " Minimum bet is 100 owlcoin!"); return; }
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
                    Bot.TwitchC.SendMessage(e.ChatMessage.Channel,"@" + e.ChatMessage.Username + ", you got [ " + emotes[combo] + " | " + emotes[combo] + " | " + emotes[combo] + " ] and won " + amount + " Owlcoins, you now have " + (coins + amount) + " Owlcoins!");
                }
                else
                {
                    combo = Enumerable.Range(1, 25).Where(x => x != 13).ElementAt(random.Next(24));
                    Shared.Data.Accounts.TakeUser(e.ChatMessage.UserId.ToString(), Shared.IDType.Twitch, amount);
                    Bot.TwitchC.SendMessage(e.ChatMessage.Channel, "@" + e.ChatMessage.Username + ", you got [ " + emotes[combo / 9] + " | " + emotes[(combo / 3) % 3] + " | " + emotes[combo % 3] + " ] and lost " + amount + " Owlcoins, you now have " + (coins - amount) + " Owlcoins!");
                }
            }
            else
            {
                Bot.TwitchC.SendMessage(e.ChatMessage.Channel, "@" + e.ChatMessage.Username + ", you only have " + coins + " Owlcoins");
            }
        }

    }
}
