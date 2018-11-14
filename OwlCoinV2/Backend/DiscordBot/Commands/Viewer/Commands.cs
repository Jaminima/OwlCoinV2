using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;
using System.Threading;

namespace OwlCoinV2.Backend.DiscordBot.Commands.Viewer
{
    public static class Commands
    {
        static Random random = new Random();

        public static async Task Pay(SocketMessage Message,string[] SegmentedMessage)
        {
            if (SegmentedMessage.Length != 3) { MessageHandler.NotLongEnough(Message); return; }
            string TheirID; Shared.IDType TheirIDType = Shared.IDType.Discord;
            TheirID = MessageHandler.GetDiscordID(SegmentedMessage[1]);
            //if (TheirID.StartsWith("<@")) { TheirID = TheirID.Replace("<@","").Replace(">",""); }
            int Amount = 0;
            if (SegmentedMessage[2].ToLower() == "all") { Amount = Shared.Data.Accounts.GetBalance(Message.Author.Id.ToString(), Shared.IDType.Discord); }
            else if (SegmentedMessage[2].ToLower().EndsWith("k"))
            {
                if (!int.TryParse(SegmentedMessage[2].ToLower().Replace("k", ""), out Amount)) { MessageHandler.InvalidParameter(Message); return; }
                Amount *= 1000;
            }
            else { if (!int.TryParse(SegmentedMessage[2].ToLower(), out Amount)) { MessageHandler.InvalidParameter(Message); return; } }

            Shared.Data.EventResponse Response = Shared.Data.Accounts.PayUser(Message.Author.Id.ToString(), Shared.IDType.Discord, TheirID, TheirIDType, Amount);
            if (Response.Success) { await MessageHandler.SendMessage(Message, Shared.ConfigHandler.Config["CommandResponses"]["Pay"]["PaymentComplete"].ToString(),TheirID,Amount, Shared.Data.Accounts.GetBalance(Message.Author.Id.ToString(), Shared.IDType.Discord)); }
            else { await MessageHandler.SendMessage(Message, MessageHandler.ParseConfigString(Response.Message,Message.Author)); }
        }

        public static async Task OwlCoin(SocketMessage Message, string[] SegmentedMessage)
        {
            if (SegmentedMessage.Length == 2/*&&SegmentedMessage[1].StartsWith("<@")*/)
            {
                SegmentedMessage[1] = MessageHandler.GetDiscordID(SegmentedMessage[1]);
                if (Shared.InputVerification.ContainsLetter(SegmentedMessage[1])) {await Message.Channel.SendMessageAsync("<@" + Message.Author.Id + "> That user doesnt exist!"); return; }
                Shared.Data.UserData.CreateUser(SegmentedMessage[1], Shared.IDType.Discord);
                await MessageHandler.SendMessage(Message, Shared.ConfigHandler.Config["CommandResponses"]["Bal"]["Other"].ToString(), SegmentedMessage[1], Shared.Data.Accounts.GetBalance(SegmentedMessage[1], Shared.IDType.Discord));
            }
            else
            {
                Shared.Data.UserData.CreateUser(Message.Author.Id.ToString(), Shared.IDType.Discord);
                await MessageHandler.SendMessage(Message, Shared.ConfigHandler.Config["CommandResponses"]["Bal"]["Self"].ToString(), null, Shared.Data.Accounts.GetBalance(Message.Author.Id.ToString(), Shared.IDType.Discord));
            }
        }

        public static async Task Roulette(SocketMessage Message, string[] SegmentedMessage)
        {
            int MinBet = int.Parse(Shared.ConfigHandler.Config["Roulette"]["MinBet"].ToString());
            if (SegmentedMessage.Length != 2) { MessageHandler.NotLongEnough(Message); return; }
            int coins, amount;
            coins = amount = Shared.Data.Accounts.GetBalance(Message.Author.Id.ToString(), Shared.IDType.Discord);
            if (SegmentedMessage[1].ToLower().EndsWith("k"))
            {
                if (!int.TryParse(SegmentedMessage[1].ToLower().Replace("k", ""), out amount)) { MessageHandler.InvalidParameter(Message); return; }
                amount *= 1000;
            }
            else if (SegmentedMessage[1].ToLower() != "all")
            {
                if (!int.TryParse(SegmentedMessage[1], out amount)) { MessageHandler.InvalidParameter(Message); return; }
            }
            if (amount < MinBet) { await MessageHandler.SendMessage(Message,Shared.ConfigHandler.Config["CommandResponses"]["Errors"]["BetTooLow"].ToString()); return; }
            if (amount <= coins)
            {
                if (random.Next(100) < int.Parse(Shared.ConfigHandler.Config["GambleWinChance"].ToString()))
                {
                    Shared.Data.Accounts.GiveUser(Message.Author.Id.ToString(), Shared.IDType.Discord, amount);
                    await MessageHandler.SendMessage(Message, Shared.ConfigHandler.Config["CommandResponses"]["Roulette"]["Win"].ToString(), null, amount);
                }
                else
                {
                    Shared.Data.Accounts.TakeUser(Message.Author.Id.ToString(), Shared.IDType.Discord, amount);
                    await MessageHandler.SendMessage(Message, Shared.ConfigHandler.Config["CommandResponses"]["Roulette"]["Lose"].ToString(), null, amount);
                }
            }
            else
            {
                await MessageHandler.SendMessage(Message, Shared.ConfigHandler.Config["CommandResponses"]["Errors"]["NotEnough"].ToString());
            }
        }

        public static async Task Slots(SocketMessage Message,string[] SegmentedMessage)
        {
            int MinBet = int.Parse(Shared.ConfigHandler.Config["Slots"]["MinBet"].ToString());
            if (SegmentedMessage.Length != 2) { MessageHandler.NotLongEnough(Message); return; }
            int coins, amount;
            coins = amount = Shared.Data.Accounts.GetBalance(Message.Author.Id.ToString(), Shared.IDType.Discord);
            if (SegmentedMessage[1].ToLower().EndsWith("k"))
            {
                if (!int.TryParse(SegmentedMessage[1].ToLower().Replace("k", ""), out amount)) { MessageHandler.InvalidParameter(Message); return; }
                amount *= 1000;
            }
            else if (SegmentedMessage[1].ToLower() != "all")
            {
                if (!int.TryParse(SegmentedMessage[1], out amount)) { MessageHandler.InvalidParameter(Message); return; }
            }
            if (amount < MinBet) { await MessageHandler.SendMessage(Message, Shared.ConfigHandler.Config["CommandResponses"]["Errors"]["BetTooLow"].ToString()); return; }
            if (amount <= coins)
            {
                string[] emotes = Shared.ConfigHandler.Config["Slots"]["Discord"].Select(e => e.ToString()).ToArray();
                int roll = random.Next(100);
                int combo = random.Next(2);
                if (roll < 10)
                {
                    amount *= 4;
                    combo = 2;
                }
                if (roll < 10)
                {
                    Shared.Data.Accounts.GiveUser(Message.Author.Id.ToString(), Shared.IDType.Discord, amount);
                    await MessageHandler.SendMessage(Message, MessageHandler.ParseConfigString(Shared.ConfigHandler.Config["CommandResponses"]["Slots"]["Win"].ToString(), Message.Author, null, amount, -1, "[ " + emotes[combo] + " | " + emotes[combo] + " | " + emotes[combo] + " ]"));
                }
                else
                {
                    combo = Enumerable.Range(1, 25).Where(x => x != 13).ElementAt(random.Next(24));
                    Shared.Data.Accounts.TakeUser(Message.Author.Id.ToString(), Shared.IDType.Discord, amount);
                    await MessageHandler.SendMessage(Message, MessageHandler.ParseConfigString(Shared.ConfigHandler.Config["CommandResponses"]["Slots"]["Lose"].ToString(), Message.Author, null, amount, -1, "[ " + emotes[combo / 9] + " | " + emotes[(combo / 3) % 3] + " | " + emotes[combo % 3] + " ]"));
                }
            }
            else
            {
                await MessageHandler.SendMessage(Message, Shared.ConfigHandler.Config["CommandResponses"]["Errors"]["NotEnough"].ToString());
            }
        }

        public static async Task Help(SocketMessage Message, string[] SegmentedMessage)
        {
            //if (await Moderator.Commands.IsMod(Message))
            //{
            //    await MessageHandler.SendMessage(Message, MessageHandler.ParseConfigString(Shared.ConfigHandler.Config["CommandResponses"]["Help"]["Discord"]["Moderator"].ToString(), Message.Author));
            //}
            await MessageHandler.SendMessage(Message, MessageHandler.ParseConfigString(Shared.ConfigHandler.Config["CommandResponses"]["Help"]["Discord"]["Viewer"].ToString(), Message.Author));
        }

        public static async Task Notifications(SocketMessage Message,string[] SegmentedMessage)
        {
            if (SegmentedMessage.Length != 2) { MessageHandler.NotLongEnough(Message); return; }
            Shared.ConfigHandler.LoadConfig();
            Newtonsoft.Json.Linq.JArray JA = Newtonsoft.Json.Linq.JArray.Parse(Shared.ConfigHandler.Config["Notifications"]["DiscordUsers"].ToString());
            if (SegmentedMessage[1].ToLower() == "on")
            {
                if (JA.ToString().Contains(Message.Author.Id.ToString())) {
                    await MessageHandler.SendMessage(Message, Shared.ConfigHandler.Config["CommandResponses"]["Notifications"]["Already"].ToString(),null,-1,-1,"On"); }
                else
                {
                    await MessageHandler.SendMessage(Message, Shared.ConfigHandler.Config["CommandResponses"]["Notifications"]["Set"].ToString(), null, -1, -1, "On");
                    JA.Add(Message.Author.Id.ToString());
                }
            }
            else if (SegmentedMessage[1].ToLower() == "off")
            {
                if (!JA.ToString().Contains(Message.Author.Id.ToString()))
                {
                    await MessageHandler.SendMessage(Message, Shared.ConfigHandler.Config["CommandResponses"]["Notifications"]["Already"].ToString(), null, -1, -1, "Off");
                }
                else
                {
                    await MessageHandler.SendMessage(Message, Shared.ConfigHandler.Config["CommandResponses"]["Notifications"]["Set"].ToString(), null, -1, -1, "Off");
                    for (int i = 0; i < JA.Count; i++) { if (JA[i].ToString() == Message.Author.Id.ToString()) { JA.RemoveAt(i); break; } }
                }
            }
            Shared.ConfigHandler.Config["Notifications"]["DiscordUsers"] = JA;
            Shared.ConfigHandler.SaveConfig();
        }

    }
}
