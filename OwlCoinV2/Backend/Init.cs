using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OwlCoinV2.Backend
{
    public static class Init
    {
        public static void Start()
        {
            Shared.ConfigHandler.LoadConfig();
            Twitch.Bot.Start();
        }
    }
}
