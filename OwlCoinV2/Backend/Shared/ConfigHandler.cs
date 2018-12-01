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
        static DateTime LastSaved = DateTime.Now;

        public static void LoadConfig(bool WithLogin=false)
        {
            try
            {
                Config = Newtonsoft.Json.Linq.JObject.Parse(System.IO.File.ReadAllText("./Data/Config.json"));
                if (WithLogin) { LoginConfig = Newtonsoft.Json.Linq.JObject.Parse(System.IO.File.ReadAllText("./Data/Login.json")); }
            }
            catch { System.Threading.Thread.Sleep(50); LoadConfig(); }
        }

        public static void SaveConfig()
        {
            try
            {
                if ((int)((TimeSpan)(DateTime.Now - LastSaved)).TotalSeconds > 15)
                {
                    System.IO.File.WriteAllText("./Data/Config.json", Config.ToString());
                    System.IO.File.WriteAllText("./Data/Login.json", LoginConfig.ToString());
                    LastSaved = DateTime.Now;
                }
            }
            catch { System.Threading.Thread.Sleep(50); SaveConfig(); }
        }

    }
}
