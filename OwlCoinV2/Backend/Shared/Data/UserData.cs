using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OwlCoinV2.Backend.Shared.Data
{
    public static class UserData
    {
        public static EventResponse CreateUser(int ID,IDType IDVariant)
        {
            EventResponse Response = new EventResponse();
            bool UserExists=false;
            if (UserData.UserExists(ID,IDVariant))
            { UserExists = true; }

            if (UserExists) { Response.Message = "User already exists"; return Response; }

            try
            {
                Init.SQLInstance.Insert("UserData",
                    new String[] { IDVariant.ToString() + "ID" },
                    new String[] { ID.ToString() }
                );
                Response.Success = true;
                Response.Message = "Created User Account";
            }
            catch { Response.Message = "Error occured during creation"; }

            return Response;
        }

        public static EventResponse AddID(int CurrentID, IDType CurrentIDVariant,int NewID, IDType NewIDVariant)
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

        public static Boolean UserExists(int ID, IDType IDVariant)
        {
            return Init.SQLInstance.Select("UserData", IDVariant.ToString() + "ID").Contains<String>(ID.ToString());
        }

    }
}
