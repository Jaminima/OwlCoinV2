using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OwlCoinV2.Backend
{
    public static class Init
    {
        public static Shared.Data.SQL SQLInstance = new Shared.Data.SQL("C:/Users/oscar/Desktop/OwlCoinV2/ExampleDatabase");
        public static void Start()
        {
            Shared.ConfigHandler.LoadConfig();
            TwitchBot.Bot.Start();
            DiscordBot.Bot.Start();
        }
    }
}
