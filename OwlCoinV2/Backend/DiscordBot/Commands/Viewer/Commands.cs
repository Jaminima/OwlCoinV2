using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;

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
            if (Shared.InputVerification.ContainsLetter(TheirID)) { await Message.Channel.SendMessageAsync("<@" + Message.Author.Id + "> That user doesnt exist!"); return; }
            //if (TheirID.StartsWith("<@")) { TheirID = TheirID.Replace("<@","").Replace(">",""); }
            if (SegmentedMessage[2].ToLower() == "all") { SegmentedMessage[2] = Shared.Data.Accounts.GetBalance(Message.Author.Id.ToString(), Shared.IDType.Discord).ToString(); }
            if (Shared.InputVerification.ContainsLetter(SegmentedMessage[2])) { MessageHandler.InvalidParameter(Message); return; };

            Shared.Data.EventResponse Response = Shared.Data.Accounts.PayUser(Message.Author.Id.ToString(), Shared.IDType.Discord, TheirID, TheirIDType, int.Parse(SegmentedMessage[2]));
            //if (Response.Success)
            //{
            await Message.Channel.SendMessageAsync("<@" + Message.Author.Id + ">" + Response.Message);
            //}
        }

        public static async Task OwlCoin(SocketMessage Message, string[] SegmentedMessage)
        {
            if (SegmentedMessage.Length == 2/*&&SegmentedMessage[1].StartsWith("<@")*/)
            {
                SegmentedMessage[1] = MessageHandler.GetDiscordID(SegmentedMessage[1]);
                if (Shared.InputVerification.ContainsLetter(SegmentedMessage[1])) {await Message.Channel.SendMessageAsync("<@" + Message.Author.Id + "> That user doesnt exist!"); return; }
                Shared.Data.UserData.CreateUser(SegmentedMessage[1], Shared.IDType.Discord);
                await Message.Channel.SendMessageAsync("<@" + Message.Author.Id + "> <@" + SegmentedMessage[1] + "> has " + Shared.Data.Accounts.GetBalance(SegmentedMessage[1], Shared.IDType.Discord) + " Owlcoin!");
            }
            else
            {
                Shared.Data.UserData.CreateUser(Message.Author.Id.ToString(), Shared.IDType.Discord);
                await Message.Channel.SendMessageAsync("<@" + Message.Author.Id + "> you have " + Shared.Data.Accounts.GetBalance(Message.Author.Id.ToString(), Shared.IDType.Discord) + " Owlcoin!");
            }
        }

        public static async Task Roulette(SocketMessage Message, string[] SegmentedMessage)
        {
            if (SegmentedMessage.Length != 2) { MessageHandler.NotLongEnough(Message); return; }
            int coins, amount;
            coins = amount = Shared.Data.Accounts.GetBalance(Message.Author.Id.ToString(), Shared.IDType.Discord);
            if (SegmentedMessage[1].ToLower() != "all")
            {
                if (!int.TryParse(SegmentedMessage[1], out amount)) { MessageHandler.InvalidParameter(Message); return; }
            }
            if (amount < 100) { await Message.Channel.SendMessageAsync("<@" + Message.Author.Id + "> Minimum bet is 100 owlcoin!"); return; }
            if (amount <= coins)
            {
                if (random.Next(100) < int.Parse(Shared.ConfigHandler.Config["GambleWinChance"].ToString()))
                {
                    Shared.Data.Accounts.GiveUser(Message.Author.Id.ToString(), Shared.IDType.Discord, amount);
                    await Message.Channel.SendMessageAsync("<@" + Message.Author.Id + "> won " + amount + " Owlcoins in roulette and now has " + (coins + amount) + " Owlcoins!");
                }
                else
                {
                    Shared.Data.Accounts.TakeUser(Message.Author.Id.ToString(), Shared.IDType.Discord, amount);
                    await Message.Channel.SendMessageAsync("<@" + Message.Author.Id + "> lost " + amount + " Owlcoins in roulette and now has " + (coins - amount) + " Owlcoins!");
                }
            }
            else
            {
                await Message.Channel.SendMessageAsync("<@" + Message.Author.Id + ">, you only have " + coins + " Owlcoins");
            }
        }

        public static async Task Slots(SocketMessage Message,string[] SegmentedMessage)
        {
            if (SegmentedMessage.Length != 2) { MessageHandler.NotLongEnough(Message); return; }
            int coins, amount;
            coins = amount = Shared.Data.Accounts.GetBalance(Message.Author.Id.ToString(), Shared.IDType.Discord);
            if (SegmentedMessage[1].ToLower() != "all")
            {
                if (!int.TryParse(SegmentedMessage[1], out amount)) { MessageHandler.InvalidParameter(Message); return; }
            }
            if (amount < 100) { await Message.Channel.SendMessageAsync("<@" + Message.Author.Id + "> Minimum bet is 100 owlcoin!"); return; }
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
                    await Message.Channel.SendMessageAsync("<@" + Message.Author.Id + ">, you got [" + emotes[combo] + "|" + emotes[combo] + "|" + emotes[combo] + "] and won " + amount + " Owlcoins, you now have " + (coins + amount) + " Owlcoins!");
                }
                else
                {
                    combo = Enumerable.Range(1, 25).Where(x => x != 13).ElementAt(random.Next(24));
                    Shared.Data.Accounts.TakeUser(Message.Author.Id.ToString(), Shared.IDType.Discord, amount);
                    await Message.Channel.SendMessageAsync("<@" + Message.Author.Id + ">, you got [" + emotes[combo / 9] + "|" + emotes[(combo / 3) % 3] + "|" + emotes[combo % 3] + "] and lost " + amount + " Owlcoins, you now have " + (coins - amount) + " Owlcoins!");
                }
            }
            else
            {
                await Message.Channel.SendMessageAsync("<@" + Message.Author.Id + ">, you only have " + coins + " Owlcoins");
            }
        }

        public static async Task Help(SocketMessage Message, string[] SegmentedMessage)
        {
            await Message.Channel.SendMessageAsync("<@" + Message.Author.Id + "> Commands are available here: https://pastebin.com/wfF6m3nq\nHave a bug? Report it here: https://goo.gl/forms/gDfHd701agXEieYu2");
        }

    }
}
