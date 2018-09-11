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
        static Random random = new Random();

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

                Shared.Data.UserData.CreateUser(Message.Author.Id.ToString(),Shared.IDType.Discord);

                Command = Command.Remove(0, Prefix.Length);

                //if (Command == "echo")
                //{
                //    if (SegmentedMessage.Length < 2) { NotLongEnough(Message); return; }
                //    await Message.Channel.SendMessageAsync(Message.Content.Remove(0, Command.Length + Prefix.Length + 1));
                //}
                if (Command == "pay")
                {
                    if (SegmentedMessage.Length != 3) { NotLongEnough(Message); return; }
                    string TheirID = GetDiscordID(SegmentedMessage[1]); Shared.IDType TheirIDType = Shared.IDType.Discord;
                    //if (TheirID.StartsWith("<@")) { TheirID = TheirID.Replace("<@","").Replace(">",""); }
                    if (SegmentedMessage[2].ToLower() == "all") { SegmentedMessage[2] = Shared.Data.Accounts.GetBalance(Message.Author.Id.ToString(), Shared.IDType.Discord).ToString(); }

                    Shared.Data.EventResponse Response = Shared.Data.Accounts.PayUser(Message.Author.Id.ToString(),Shared.IDType.Discord,TheirID,TheirIDType,int.Parse(SegmentedMessage[2]));
                    //if (Response.Success)
                    //{
                        await Message.Channel.SendMessageAsync("<@"+Message.Author.Id+">"+Response.Message);
                    //}
                }

                if (Command == "owlcoin" || Command == "bal" || Command == "balance")
                {
                    if (SegmentedMessage.Length==2/*&&SegmentedMessage[1].StartsWith("<@")*/) {
                        SegmentedMessage[1] = GetDiscordID(SegmentedMessage[1]);//.Replace("<@", "").Replace(">", "");
                        Shared.Data.UserData.CreateUser(SegmentedMessage[1], Shared.IDType.Discord);
                        await Message.Channel.SendMessageAsync("<@" + Message.Author.Id + "> <@"+SegmentedMessage[1]+"> has " + Shared.Data.Accounts.GetBalance(SegmentedMessage[1], Shared.IDType.Discord) + " Owlcoin!");
                    }
                    else {
                        Shared.Data.UserData.CreateUser(Message.Author.Id.ToString(), Shared.IDType.Discord);
                        await Message.Channel.SendMessageAsync("<@" + Message.Author.Id + "> you have " + Shared.Data.Accounts.GetBalance(Message.Author.Id.ToString(), Shared.IDType.Discord) + " Owlcoin!"); }
                }

                if (Command == "gamble" || Command == "roulette")
                {
                    if (SegmentedMessage.Length != 2) { NotLongEnough(Message); return; }
                    int coins, amount;
                    coins = amount = Shared.Data.Accounts.GetBalance(Message.Author.Id.ToString(), Shared.IDType.Discord);
                    if (SegmentedMessage[1].ToLower() != "all")
                    {
                        if (!int.TryParse(SegmentedMessage[1], out amount)) { InvalidParameter(Message); return; }
                    }

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

                if (Command == "duel")
                {
                    if (SegmentedMessage.Length != 3) { NotLongEnough(Message); return; }
                    string TheirID = GetDiscordID(SegmentedMessage[1]);
                    if (TheirID == Message.Author.Id.ToString())
                    {
                        await Message.Channel.SendMessageAsync("<@" + Message.Author.Id + ">, You can't duel yourself");
                        return;
                    }
                    int amount, myCoins, theirCoins;
                    myCoins = amount = Shared.Data.Accounts.GetBalance(Message.Author.Id.ToString(), Shared.IDType.Discord);
                    theirCoins = Shared.Data.Accounts.GetBalance(TheirID, Shared.IDType.Discord);
                    if (SegmentedMessage[2].ToLower() != "all")
                    {
                        if (!int.TryParse(SegmentedMessage[2], out amount)) { InvalidParameter(Message); return; }
                    }
                    if (amount <= myCoins)
                    {
                        if (amount <= theirCoins)
                        {
                            if (Duels.duels.Add(new Shared.Duel(Message.Author.Id.ToString(), TheirID, amount)))
                            {
                                await Message.Channel.SendMessageAsync("<@" + TheirID + ">, <@" + Message.Author.Id + "> wants to duel you for " + amount + " Owlcoins, you can !accept or !deny within 2 minutes");
                            }
                            else
                            {
                                await Message.Channel.SendMessageAsync("<@" + Message.Author.Id + ">, this person is already in a duel");
                            }
                        }
                        else
                        {
                            await Message.Channel.SendMessageAsync("<@" + TheirID + "> only has " + theirCoins + " Owlcoins");
                        }
                    }
                    else
                    {
                        await Message.Channel.SendMessageAsync("<@" + Message.Author.Id + ">, you only have " + myCoins + " Owlcoins");
                    }
                }

                if (Command == "accept")
                {
                    Shared.Duel duel = Duels.duels.SingleOrDefault(d => d.target == Message.Author.Id.ToString());
                    if (duel == null)
                    {
                        await Message.Channel.SendMessageAsync("<@" + Message.Author.Id + ">, You are not in a duel");
                        return;
                    }
                    Duels.duels.Remove(duel);
                    int targetCoins = Shared.Data.Accounts.GetBalance(duel.target, Shared.IDType.Discord);
                    int dueleeCoins = Shared.Data.Accounts.GetBalance(duel.duelee, Shared.IDType.Discord);
                    if (targetCoins < duel.amount)
                    {
                        await Message.Channel.SendMessageAsync("<@" + duel.target + "> only has " + targetCoins + " Owlcoins");
                    }
                    if (dueleeCoins < duel.amount)
                    {
                        await Message.Channel.SendMessageAsync("<@" + duel.duelee + "> only has " + dueleeCoins + " Owlcoins");
                    }
                    Shared.Data.EventResponse Response;
                    if (random.Next(100) < 50)
                    {
                        Response = Shared.Data.Accounts.PayUser(duel.target, Shared.IDType.Discord, duel.duelee, Shared.IDType.Discord, duel.amount);
                        await Message.Channel.SendMessageAsync("<@" + duel.duelee + "> won " + duel.amount + " Owlcoins in a duel against <@" + duel.target + "> and now has " + (dueleeCoins + duel.amount) + " Owlcoins!");
                    }
                    else
                    {
                        Response = Shared.Data.Accounts.PayUser(duel.duelee, Shared.IDType.Discord, duel.target, Shared.IDType.Discord, duel.amount);
                        await Message.Channel.SendMessageAsync("<@" + duel.target + "> won " + duel.amount + " Owlcoins in a duel against <@" + duel.duelee + "> and now has " + (targetCoins + duel.amount) + " Owlcoins!");
                    }
                    if (!Response.Success)
                    {
                        Console.WriteLine(Response.Message);
                    }
                }

                if (Command == "deny")
                {
                    if(Duels.duels.RemoveWhere(duel => duel.target == Message.Author.Id.ToString()) == 1)
                    {
                        await Message.Channel.SendMessageAsync("<@" + Message.Author.Id + ">, You have denied the duel");
                    }
                    else
                    {
                        await Message.Channel.SendMessageAsync("<@" + Message.Author.Id + ">, You are not in a duel");
                    }
                }

                if (Command == "slots")
                {
                    if (SegmentedMessage.Length != 2) { NotLongEnough(Message); return; }
                    int coins, amount;
                    coins = amount = Shared.Data.Accounts.GetBalance(Message.Author.Id.ToString(), Shared.IDType.Discord);
                    if (SegmentedMessage[1].ToLower() != "all")
                    {
                        if (!int.TryParse(SegmentedMessage[1], out amount)) { InvalidParameter(Message); return; }
                    }
                    if (amount <= coins)
                    {
                        string[] emotes = Shared.ConfigHandler.Config["DiscordSlotsEmotes"].Select(e => e.ToString()).ToArray();
                        int roll = random.Next(100);
                        int combo = random.Next(2);
                        if (roll < 10)
                        {
                            amount *= 2;
                            combo = 2;
                        }
                        if (roll < 35)
                        {
                            Shared.Data.Accounts.GiveUser(Message.Author.Id.ToString(), Shared.IDType.Discord, amount);
                            await Message.Channel.SendMessageAsync("<@" + Message.Author.Id + ">, you got [" + emotes[combo] + "|" + emotes[combo] + "|" + emotes[combo] + "] and won " + amount + " Owlcoins, you now have " + (coins + amount) + " Owlcoins!");
                        }
                        else
                        {
                            combo = Enumerable.Range(1, 25).Where(x => x != 13).ElementAt(random.Next(24));
                            Shared.Data.Accounts.TakeUser(Message.Author.Id.ToString(), Shared.IDType.Discord, amount);
                            await Message.Channel.SendMessageAsync("<@" + Message.Author.Id + ">, you got [" + emotes[combo/9] + "|" + emotes[(combo/3)%3] + "|" + emotes[combo%3] + "] and lost " + amount + " Owlcoins, you now have " + (coins - amount) + " Owlcoins!");
                        }
                    }
                    else
                    {
                        await Message.Channel.SendMessageAsync("<@" + Message.Author.Id + ">, you only have " + coins + " Owlcoins");
                    }
                }
            }
        }

        static async void NotLongEnough(SocketMessage Message)
        {
            await Message.Channel.SendMessageAsync("<@" + Message.Author.Id + ">, you are missing parameters or have too many!");
        }

        static async void InvalidParameter(SocketMessage Message)
        {
            await Message.Channel.SendMessageAsync("<@" + Message.Author.Id + ">, you have one or more invalid parameter!");
        }

        static string GetDiscordID(string message)
        {
            if (message.StartsWith("<@"))
            {
                message = message.Replace("<@", "").Replace(">", "").Replace("!", "");
            }
            return message;
        }
    }
}
