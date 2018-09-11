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
            Backend.TwitchBot.Streamlabs.Alert.SendRequest("https://cdn.streamelements.com/uploads/833fb7fa-b031-461d-8cc1-2f81cae30bce.PNG", "https://cdn.streamelements.com/uploads/c9541de6-4306-4c3b-ae2f-147a078f6d91.ogg");
            //TwitchBot.Bot.Start();
            //DiscordBot.Bot.Start();
        }
    }
}
