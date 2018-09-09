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
                    string TheirID = SegmentedMessage[1]; Shared.IDType TheirIDType = Shared.IDType.Discord;
                    if (TheirID.StartsWith("<@")) { TheirID = TheirID.Replace("<@","").Replace(">",""); }
                    if (SegmentedMessage[2].ToLower() == "all") { SegmentedMessage[2] = Shared.Data.Accounts.GetBalance(Message.Author.Id.ToString(), Shared.IDType.Discord).ToString(); }

                    Shared.Data.EventResponse Response = Shared.Data.Accounts.PayUser(Message.Author.Id.ToString(),Shared.IDType.Discord,TheirID,TheirIDType,int.Parse(SegmentedMessage[2]));
                    //if (Response.Success)
                    //{
                        await Message.Channel.SendMessageAsync("<@"+Message.Author.Id+">"+Response.Message);
                    //}
                }

                if (Command == "owlcoin" || Command == "bal" || Command == "balance")
                {
                    if (SegmentedMessage.Length==2&&SegmentedMessage[1].StartsWith("<@")) {
                        SegmentedMessage[1] = SegmentedMessage[1].Replace("<@", "").Replace(">", "");
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
                    if (SegmentedMessage[1] != "all")
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
            }
        }

        static async void NotLongEnough(SocketMessage Message)
        {
            await Message.Channel.SendMessageAsync("You are missing parameters or have too many!");
        }

        static async void InvalidParameter(SocketMessage Message)
        {
            await Message.Channel.SendMessageAsync("You have one or more invalid parameter!");
        }
    }
}
