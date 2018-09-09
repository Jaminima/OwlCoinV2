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
                WebRequest Req = WebRequest.Create("https://discordapp.com/api/v6/users/" + ID + "/profile");
                Req.Headers.Add("authorization", "NDg4MTM0MzIzNTczMDk2NDQ5.DnaNOA.A6M5N6TBbQO6PlblaOoIKqjgU9Y");
                Req.Method = "GET";
                WebResponse Res = Req.GetResponse();
                string D = new StreamReader(Res.GetResponseStream()).ReadToEnd();

                Newtonsoft.Json.Linq.JObject ProfileData = Newtonsoft.Json.Linq.JObject.Parse(D);
                foreach (Newtonsoft.Json.Linq.JObject Connection in ProfileData["connected_accounts"])
                {
                    if (Connection["type"].ToString() == "twitch")
                    {
                        try
                        {
                            Init.SQLInstance.Insert("UserData",
                                new String[] { IDVariant.ToString() + "ID","TwitchID" },
                                new String[] { ID.ToString(), Connection["id"].ToString() }
                            );
                            Response.Success = true;
                            Response.Message = "Created User Account and added Twitch";
                            
                        }
                        catch { Response.Message = "Error occured during creation"; }
                        CreateAccount(ID, IDVariant);
                        return Response;
                    }
                }
            }

            try
            {
                Init.SQLInstance.Insert("UserData",
                    new String[] { IDVariant.ToString() + "ID" },
                    new String[] { ID.ToString() }
                );
                Response.Success = true;
                Response.Message = "Created User Account";
                CreateAccount(ID, IDVariant);
            }
            catch { Response.Message = "Error occured during creation"; }

            return Response;
        }

        static void CreateAccount(string ID, IDType IDVariant)
        {
            string OwlCoinID = Init.SQLInstance.Select("UserData", "OwlCoinID", IDVariant.ToString() + "ID='" + ID+"'")[0];
            Init.SQLInstance.Insert("Accounts",new string[] { "OwlCoinID","Balance" },new string[] { OwlCoinID,"0" });
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

        public static Boolean UserExists(string ID, IDType IDVariant)
        {
            return Init.SQLInstance.Select("UserData", IDVariant.ToString() + "ID").Contains<String>(ID.ToString());
        }

    }
}
