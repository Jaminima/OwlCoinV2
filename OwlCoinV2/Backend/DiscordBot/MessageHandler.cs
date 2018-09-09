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

                Shared.Data.UserData.CreateUser(Message.Author.Id.ToString(),Shared.IDType.Discord);

                Command = Command.Remove(0, Prefix.Length);

                if (Command == "echo")
                {
                    await Message.Channel.SendMessageAsync(Message.Content.Remove(0, Command.Length + Prefix.Length + 1));
                }
                if (Command == "pay")
                {
                    if (SegmentedMessage.Length != 3) { NotLongEnough(Message); return; }
                    string TheirID = SegmentedMessage[1]; Shared.IDType TheirIDType = Shared.IDType.Discord;
                    if (TheirID.StartsWith("<@")) { TheirID = TheirID.Replace("<@","").Replace(">",""); }

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

            }
        }

        static async void NotLongEnough(SocketMessage Message)
        {
            await Message.Channel.SendMessageAsync("You are missing parameters or have too many!");
        }

    }
}
