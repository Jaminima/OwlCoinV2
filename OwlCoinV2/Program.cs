using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OwlCoinV2
{
    class Program
    {
        static void Main(string[] args)
        {
            //Backend.Shared.Data.UserData.CreateUser(4,Backend.Shared.IDType.Twitch);
            //Backend.Shared.Data.UserData.AddID(4, Backend.Shared.IDType.Twitch,6,Backend.Shared.IDType.Discord);
            Backend.Init.Start();
            while (true) { Console.ReadLine(); }
        }
    }
}
