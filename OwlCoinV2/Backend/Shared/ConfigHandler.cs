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
                Config = LoadConfig("./Data/Config");
                if (WithLogin) { LoginConfig = LoadConfig("./Data/Login"); }
            }
            catch { System.Threading.Thread.Sleep(50); LoadConfig(); }
        }
        public static Newtonsoft.Json.Linq.JObject LoadConfig(string Path)
        {
            return Newtonsoft.Json.Linq.JObject.Parse(System.IO.File.ReadAllText(Path+".json"));
        }

        public static void SaveConfig()
        {
            try
            {
                //if ((int)((TimeSpan)(DateTime.Now - LastSaved)).TotalSeconds > 15)
                //{
                    SaveConfig("./Data/Config", Config);
                    SaveConfig("./Data/Login", LoginConfig);
                    LastSaved = DateTime.Now;
                //}
            }
            catch { System.Threading.Thread.Sleep(50); SaveConfig(); }
        }
        public static void SaveConfig(string Path,Newtonsoft.Json.Linq.JObject Data)
        {
            System.IO.File.WriteAllText(Path+".json", Data.ToString());
        }

    }
}
