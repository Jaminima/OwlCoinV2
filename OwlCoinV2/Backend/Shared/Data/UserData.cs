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
        public static EventResponse CreateUser(string ID, IDType IDVariant)
        {
            EventResponse Response = new EventResponse();
            bool UserExists = false;
            if (UserData.UserExists(ID, IDVariant))
            { UserExists = true; }

            if (UserExists) { Response.Message = "User already exists"; return Response; }

            if (IDVariant == IDType.Discord)
            {
                foreach (Newtonsoft.Json.Linq.JObject Connection in GetConnections(ID)["connected_accounts"])
                {
                    if (Connection["type"].ToString() == "twitch")
                    {
                        if (UserData.UserExists(Connection["id"].ToString(), IDType.Twitch))
                        {
                            Newtonsoft.Json.Linq.JToken R = UserData.GetUser(Connection["id"].ToString(), IDType.Twitch);
                            if (R["Status"].ToString() != "200") { Response.Message = Shared.ConfigHandler.Config["CommandResponses"]["Errors"]["WhoKnows"].ToString(); return Response; }
                            Newtonsoft.Json.Linq.JToken User = R["Data"];
                            if (User["DiscordId"].ToString() == "")
                            {
                                User["DiscordId"] = ID.ToString();
                                WebRequests.POST("/update/user", null, User.ToString());
                                return Response;
                            }
                        }
                        else
                        {
                            Dictionary<string, string> Headers1 = new Dictionary<string, string> { };
                            Headers1.Add("DiscordId", ID.ToString()); Headers1.Add("TwitchId", Connection["id"].ToString());
                            WebRequests.POST("/create/user", Headers1);
                            Response.Success = true;
                            Response.Message = "Created User Account and added Twitch";
                            return Response;
                        }
                    }
                }
            }
            Dictionary<string, string> Headers2 = new Dictionary<string, string> { };
            Headers2.Add(IDVariant.ToString() + "Id", ID.ToString());
            WebRequests.POST("/create/user", Headers2);
            Response.Success = true;
            Response.Message = "Created User Account";

            return Response;
        }

        public static EventResponse MergeAccounts(string DiscordID)
        {
            EventResponse Response = new EventResponse();
            Newtonsoft.Json.Linq.JToken R = UserData.GetUser(DiscordID, Shared.IDType.Twitch);
            if (R["Status"].ToString() != "200") { Response.Message = Shared.ConfigHandler.Config["CommandResponses"]["Errors"]["WhoKnows"].ToString(); return Response; }
            Newtonsoft.Json.Linq.JToken User = R["Data"];
            if (User["TwitchId"].ToString() == "")
            {
                foreach (Newtonsoft.Json.Linq.JObject Connection in GetConnections(DiscordID)["connected_accounts"])
                {
                    if (Connection["type"].ToString() == "twitch")
                    {
                        if (UserData.UserExists(Connection["id"].ToString(), IDType.Twitch))
                        {
                            R = UserData.GetUser(Connection["id"].ToString(), IDType.Twitch);
                            if (R["Status"].ToString() != "200") { Response.Message = Shared.ConfigHandler.Config["CommandResponses"]["Errors"]["WhoKnows"].ToString(); return Response; }
                            User = R["Data"];
                            if (User["DiscordId"].ToString() == "")
                            {
                                Accounts.GiveUser(Connection["id"].ToString(), IDType.Twitch, Accounts.GetBalance(DiscordID, IDType.Discord));
                                R = UserData.GetUser(DiscordID, Shared.IDType.Discord);
                                if (R["Status"].ToString() != "200") { Response.Message = Shared.ConfigHandler.Config["CommandResponses"]["Errors"]["WhoKnows"].ToString(); return Response; }
                                User = R["Data"];
                                WebRequests.POST("/delete/user/"+User["UserId"].ToString());
                                R = UserData.GetUser(Connection["id"].ToString(), IDType.Twitch);
                                if (R["Status"].ToString() != "200") { Response.Message = Shared.ConfigHandler.Config["CommandResponses"]["Errors"]["WhoKnows"].ToString(); return Response; }
                                User = R["Data"];
                                User["DiscordId"] = DiscordID.ToString();
                                WebRequests.POST("/update/user", null, User.ToString());
                            }
                        }
                        break;
                    }
                }
            }
            return Response;
        }

        public static EventResponse AddID(string CurrentID, IDType CurrentIDVariant, string NewID, IDType NewIDVariant)
        {
            EventResponse Response = new EventResponse();

            bool UserExists = false;
            if (UserData.UserExists(CurrentID, CurrentIDVariant))
            { UserExists = true; }

            if (!UserExists) { Response.Message = "User doesnt exist"; return Response; }

            Newtonsoft.Json.Linq.JToken R = UserData.GetUser(CurrentID, CurrentIDVariant);
            if (R["Status"].ToString() != "200") { Response.Message = Shared.ConfigHandler.Config["CommandResponses"]["Errors"]["WhoKnows"].ToString(); return Response; }
            Newtonsoft.Json.Linq.JToken User = R["Data"];
            User[NewIDVariant.ToString() + "Id"] = NewID;
            Newtonsoft.Json.Linq.JToken D = WebRequests.POST("/update/user", null, User.ToString());

            if (D["Status"].ToString() == "200")
            {
                Response.Success = true;
                Response.Message = "Set " + NewIDVariant.ToString() + "ID";
            }
            else { Response.Message = "Error occured while setting " + NewIDVariant.ToString() + "ID"; }

            return Response;
        }

        static Newtonsoft.Json.Linq.JObject GetConnections(string ID)
        {
            WebRequest Req = WebRequest.Create("https://discordapp.com/api/v6/users/" + ID + "/profile");
            Req.Headers.Add("authorization", ConfigHandler.LoginConfig["DiscordBot"]["AuthToken"].ToString());
            Req.Method = "GET";
            WebResponse Res = Req.GetResponse();
            string D = new StreamReader(Res.GetResponseStream()).ReadToEnd();
            Newtonsoft.Json.Linq.JObject ProfileData = Newtonsoft.Json.Linq.JObject.Parse(D);
            return ProfileData;
        }

        public static Boolean UserExists(string ID, IDType IDVariant)
        {
            return GetUser(ID, IDVariant)["Status"].ToString() == "200";
        }

        public static Newtonsoft.Json.Linq.JToken GetUser(string ID, IDType IDVariant)
        {
            Dictionary<string, string> Headers = new Dictionary<string, string> { };
            Headers.Add(IDVariant.ToString() + "Id", ID);
            Newtonsoft.Json.Linq.JToken Response = WebRequests.POST("/user", Headers,"",false);
            return Response;
        }

    }
}
