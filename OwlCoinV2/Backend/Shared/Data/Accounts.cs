using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OwlCoinV2.Backend.Shared.Data
{
    public static class Accounts
    {
        public static void CreateAccount(string ID, IDType IDVariant)
        {
            string OwlCoinID = Init.SQLInstance.Select("UserData", "OwlCoinID", IDVariant.ToString() + "ID='" + ID + "'")[0];
            Init.SQLInstance.Insert("Accounts", new string[] { "OwlCoinID", "Balance" }, new string[] { OwlCoinID, "500" });
        }

        public static int GetBalance(string ID, IDType IDVariant)
        {
            string OwlCoinID = Init.SQLInstance.Select("UserData", "OwlCoinID", IDVariant.ToString() + "ID='" + ID + "'")[0];
            return int.Parse(Init.SQLInstance.Select("Accounts", "Balance", "OwlCoinID=" + OwlCoinID)[0]);
        }

        public static bool GiveUser(string ID, IDType IDVariant,int Amount)
        {
            Amount = Math.Abs(Amount);
            bool Response = false;

            UserData.CreateUser(ID, IDVariant);
            string OCID = Init.SQLInstance.Select("UserData", "OwlCoinID", IDVariant.ToString() + "ID='" + ID + "'")[0];
            int Bal = int.Parse(Init.SQLInstance.Select("Accounts", "Balance", "OwlCoinID=" + OCID)[0]);
            Bal += Amount;
            Init.SQLInstance.Update("Accounts", "OwlCoinID=" + OCID, "Balance=" + Bal);
            Response = true;
            return Response;
        }

        public static bool TakeUser(string ID, IDType IDVariant, int Amount)
        {
            Amount = Math.Abs(Amount);
            bool Response = false;

            UserData.CreateUser(ID, IDVariant);
            string OCID = Init.SQLInstance.Select("UserData", "OwlCoinID", IDVariant.ToString() + "ID='" + ID + "'")[0];
            int Bal = int.Parse(Init.SQLInstance.Select("Accounts", "Balance", "OwlCoinID=" + OCID)[0]);
            Bal -= Amount;
            if (Bal >= 0)
            {
                Init.SQLInstance.Update("Accounts", "OwlCoinID=" + OCID, "Balance=" + Bal);
                Response = true;
            }
            return Response;
        }

        public static EventResponse PayUser(string MyID, IDType MyIDType, string TheirID, IDType TheirIDType, int Amount)
        {
            Amount = Math.Abs(Amount);
            EventResponse Response = new EventResponse();

            UserData.CreateUser(TheirID, TheirIDType);

            if (UserData.UserExists(MyID, MyIDType) && UserData.UserExists(TheirID, TheirIDType))
            {
                string MyOCID = Init.SQLInstance.Select("UserData", "OwlCoinID", MyIDType.ToString() + "ID='" + MyID + "'")[0],
                    TheirOCID = Init.SQLInstance.Select("UserData", "OwlCoinID", TheirIDType.ToString() + "ID='" + TheirID + "'")[0];
                if (MyOCID == TheirOCID) { Response.Message="You can't pay yourself"; return Response; }
                int MyBal = int.Parse(Init.SQLInstance.Select("Accounts", "Balance", "OwlCoinID=" + MyOCID)[0]),
                    TheirBal = int.Parse(Init.SQLInstance.Select("Accounts", "Balance", "OwlCoinID=" + TheirOCID)[0]);
                if (MyBal >= Amount)
                {
                    TheirBal += Amount; MyBal -= Amount;
                    Init.SQLInstance.Update("Accounts", "OwlCoinID=" + MyOCID, "Balance=" + MyBal);
                    Init.SQLInstance.Update("Accounts", "OwlCoinID=" + TheirOCID, "Balance=" + TheirBal);
                    Response.Message = " Payment of "+Amount+" Owlcoin Complete, New Balance: "+MyBal+" Owlcoin!";
                    Response.Success = true;
                }
                else { Response.Message = "Not enough OwlCoin"; }
            }
            else { Response.Message = "The person you are trying to pay, doesnt have an account"; }

            return Response;
        }

    }
}
