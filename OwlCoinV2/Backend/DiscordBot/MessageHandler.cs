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
            new Thread(async () => await Proccesor(Message)).Start();
            return null;
        }

        public static async Task Proccesor(SocketMessage Message)
        {
            
            string Command = Message.Content.Split(" ".ToCharArray())[0].ToLower();
            string Prefix = Shared.ConfigHandler.Config["Prefix"].ToString();
            if (Command.StartsWith(Prefix))
            {

                Shared.Data.UserData.CreateUser(Message.Author.Id.ToString(),Shared.IDType.Discord);

                Command = Command.Remove(0, Prefix.Length);

                if (Command == "echo")
                {
                    await Message.Channel.SendMessageAsync(Message.Content.Remove(0, Command.Length + Prefix.Length + 1));
                }

            }
        }
    }
}
