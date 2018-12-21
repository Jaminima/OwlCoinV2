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
        public static TwitchLib.Api.Models.v5.Users.Users UserFromUsername(string Username,int i=0)
        {
            if (i == 10) { return null; }
            try
            {
                Task<TwitchLib.Api.Models.v5.Users.Users> GetUName = new TwitchLib.Api.Sections.Users.V5Api(Bot.TwitchA).GetUserByNameAsync(Username);
                return Task.Run(async () => await new TwitchLib.Api.Sections.Users.V5Api(Bot.TwitchA).GetUserByNameAsync(Username)).Result;
            }
            catch (Exception E) { Console.WriteLine(E); return UserFromUsername(Username,i++); }
        }

        public static TwitchLib.Api.Models.v5.Users.User UserFromUserID(string UserID,int i=0)
        {
            if (i == 10) { return null; }
            try
            {
                Task<TwitchLib.Api.Models.v5.Users.User> GetUName = new TwitchLib.Api.Sections.Users.V5Api(Bot.TwitchA).GetUserByIDAsync(UserID);
                return Task.Run(async () => await new TwitchLib.Api.Sections.Users.V5Api(Bot.TwitchA).GetUserByIDAsync(UserID)).Result;
            }
            catch (Exception E) { Console.WriteLine(E); return UserFromUserID(UserID,i++); }
        }

    }
}
