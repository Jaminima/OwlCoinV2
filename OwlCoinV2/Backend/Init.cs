using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OwlCoinV2.Backend
{
    public static class Init
    {
        //public static Shared.Data.SQL SQLInstance = new Shared.Data.SQL("C:/Users/oscar/Desktop/OwlCoinV2/ExampleDatabase");
        public static Shared.Data.SQL SQLInstance = new Shared.Data.SQL("C:/Users/Administrator/Desktop/OwlCoinV2/Data/ExampleDatabase");
        public static void Start()
        {
            Shared.ConfigHandler.LoadConfig();
            //Backend.TwitchBot.Streamlabs.Alert.SendRequest("https://cdn.streamelements.com/uploads/833fb7fa-b031-461d-8cc1-2f81cae30bce.PNG", "https://cdn.streamelements.com/uploads/adc85888-65cb-4e6e-9486-0f44dab96457.mp3");
            TwitchBot.Bot.Start();
            DiscordBot.Bot.Start();
        }
    }
}
