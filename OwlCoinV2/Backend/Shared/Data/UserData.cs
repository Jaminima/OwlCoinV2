using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;

namespace OwlCoinV2.Backend.Shared.Data
{
    public static class UserData
    {
        public static EventResponse CreateUser(string ID,IDType IDVariant)
        {
            EventResponse Response = new EventResponse();
            bool UserExists=false;
            if (UserData.UserExists(ID,IDVariant))
            { UserExists = true; }

            if (UserExists) { Response.Message = "User already exists"; return Response; }

            if (IDVariant == IDType.Discord)
            {
                try
                {
                    foreach (Newtonsoft.Json.Linq.JObject Connection in GetConnections(ID)["connected_accounts"])
                    {
                        if (Connection["type"].ToString() == "twitch")
                        {
                            if (UserData.UserExists(Connection["id"].ToString(), IDType.Twitch))
                            {
                                if (Init.SQLInstance.Select("UserData", "DiscordID", "TwitchID=\"" + Connection["id"] + "\"")[0] == "")
                                {
                                    Init.SQLInstance.Update("UserData", "TwitchID=\"" + Connection["id"] + "\"", "DiscordID=\"" + ID.ToString() + "\"");
                                    return Response;
                                }
                            }
                            else
                            {
                                try
                                {
                                    Init.SQLInstance.Insert("UserData",
                                        new String[] { IDVariant.ToString() + "ID", "TwitchID" },
                                        new String[] { ID.ToString(), Connection["id"].ToString() }
                                    );
                                    Response.Success = true;
                                    Response.Message = "Created User Account and added Twitch";
                                    Accounts.CreateAccount(ID, IDVariant);
                                }
                                catch { Response.Message = "Error occured during creation"; }
                                return Response;
                            }
                        }
                    }
                }
                catch { }
            }

            try
            {
                Init.SQLInstance.Insert("UserData",
                    new String[] { IDVariant.ToString() + "ID" },
                    new String[] { ID.ToString() }
                );
                Response.Success = true;
                Response.Message = "Created User Account";
                Accounts.CreateAccount(ID, IDVariant);
            }
            catch { Response.Message = "Error occured during creation"; }

            return Response;
        }

        public static EventResponse MergeAccounts(string DiscordID)
        {
            EventResponse Response = new EventResponse();
            if (Init.SQLInstance.Select("UserData", "TwitchID", "DiscordID=\"" + DiscordID + "\"")[0] == "")
            {
                foreach (Newtonsoft.Json.Linq.JObject Connection in GetConnections(DiscordID)["connected_accounts"])
                {
                    if (Connection["type"].ToString() == "twitch")
                    {
                        if (UserData.UserExists(Connection["id"].ToString(), IDType.Twitch))
                        {
                            if (Init.SQLInstance.Select("UserData", "DiscordID", "TwitchID=\"" + Connection["id"] + "\"")[0] == "")
                            {
                                Accounts.GiveUser(Connection["id"].ToString(), IDType.Twitch, Accounts.GetBalance(DiscordID, IDType.Discord));
                                Init.SQLInstance.Delete("Accounts", "OwlCoinID=" + Init.SQLInstance.Select("UserData", "OwlCoinID", "DiscordID=\"" + DiscordID + "\"")[0]);
                                Init.SQLInstance.Delete("UserData", "DiscordID=\"" + DiscordID + "\"");
                                Init.SQLInstance.Update("UserData", "TwitchID=\"" + Connection["id"] + "\"", "DiscordID=\"" + DiscordID.ToString() + "\"");
                            }
                        }
                    }
                }
            }
            return Response;
        }

        public static EventResponse AddID(string CurrentID, IDType CurrentIDVariant,string NewID, IDType NewIDVariant)
        {
            EventResponse Response = new EventResponse();

            bool UserExists = false;
            if (UserData.UserExists(CurrentID,CurrentIDVariant))
            { UserExists = true; }

            if (!UserExists) { Response.Message = "User doesnt exist"; return Response; }

            try
            {
                Init.SQLInstance.Update("UserData",CurrentIDVariant.ToString()+"ID="+CurrentID,NewIDVariant.ToString()+"ID="+NewID);
                Response.Success = true;
                Response.Message = "Set " + NewIDVariant.ToString() + "ID";
            }
            catch { Response.Message = "Error occured while setting " + NewIDVariant.ToString() + "ID"; }

            return Response;
        }

        static Newtonsoft.Json.Linq.JObject GetConnections(string ID)
        {
            WebRequest Req = WebRequest.Create("https://discordapp.com/api/v6/users/" + ID + "/profile");
            Req.Headers.Add("authorization", ConfigHandler.Config["DiscordBot"]["AuthToken"].ToString());
            Req.Method = "GET";
            WebResponse Res = Req.GetResponse();
            string D = new StreamReader(Res.GetResponseStream()).ReadToEnd();
            Newtonsoft.Json.Linq.JObject ProfileData = Newtonsoft.Json.Linq.JObject.Parse(D);
            return ProfileData;
        }

        public static Boolean UserExists(string ID, IDType IDVariant)
        {
            return Init.SQLInstance.Select("UserData", IDVariant.ToString() + "ID").Contains<String>(ID.ToString());
        }

    }
}
