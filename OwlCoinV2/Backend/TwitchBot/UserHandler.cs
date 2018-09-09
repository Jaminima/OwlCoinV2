using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwitchLib.Client;
using TwitchLib.Client.Models;
using TwitchLib.Client.Events;
using System.Net;
using System.Threading;

namespace OwlCoinV2.Backend.TwitchBot
{
    public static class UserHandler
    {
        public static List<String> WatchingTwitchIDs = new List<String> { };

        public static void HandleUserJoin(object sender,OnUserJoinedArgs e)
        {
            string UID = UserFromUsername(e.Username).Matches[0].Id;
            if (!WatchingTwitchIDs.Contains(UID)&&e.Username!= "owlcoinbot") { WatchingTwitchIDs.Add(UID); } else { return; }
            Shared.Data.UserData.CreateUser(UID, Shared.IDType.Twitch);
            Console.WriteLine(e.Username+"Joined");
        }

        public static void HandleUserLeft(object sender, OnUserLeftArgs e)
        {
            String UID = UserFromUsername(e.Username).Matches[0].Id;
            if (WatchingTwitchIDs.Contains(UID)) { WatchingTwitchIDs.Remove(UID); } else { return; }
            Console.WriteLine(e.Username+"Left");
        }

        public static TwitchLib.Api.Models.v5.Users.Users UserFromUsername(string Username)
        {
            Task<TwitchLib.Api.Models.v5.Users.Users> GetUName = new TwitchLib.Api.Sections.Users.V5Api(Bot.TwitchA).GetUserByNameAsync(Username);
            return Task.Run(async () => await new TwitchLib.Api.Sections.Users.V5Api(Bot.TwitchA).GetUserByNameAsync(Username)).Result;
        }

    }
}
