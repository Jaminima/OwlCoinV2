using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OwlCoinV2.Backend.Shared
{
    public static class ConfigHandler
    {
        public static Newtonsoft.Json.Linq.JObject Config,LoginConfig;

        public static void LoadConfig()
        {
            Config = Newtonsoft.Json.Linq.JObject.Parse(System.IO.File.ReadAllText("./Data/Config.json"));
            LoginConfig = Newtonsoft.Json.Linq.JObject.Parse(System.IO.File.ReadAllText("./Data/Login.json"));
        }

        public static void SaveConfig()
        {
            System.IO.File.WriteAllText("./Data/Config.json",Config.ToString());
            System.IO.File.WriteAllText("./Data/Login.json", LoginConfig.ToString());
        }

    }
}
