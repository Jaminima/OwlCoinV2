using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OwlCoinV2.Backend.Shared
{
    public static class ConfigHandler
    {
        public static Newtonsoft.Json.Linq.JObject Config;

        public static void LoadConfig()
        {
            Config = Newtonsoft.Json.Linq.JObject.Parse(System.IO.File.ReadAllText("./Data/Config.json"));
        }

    }
}
