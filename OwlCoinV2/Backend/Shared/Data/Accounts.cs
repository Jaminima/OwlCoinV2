using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OwlCoinV2.Backend.Shared.Data
{
    public static class Accounts
    {
        public static int GetBalance(string ID, IDType IDVariant)
        {
            UserData.CreateUser(ID, IDVariant);
            Newtonsoft.Json.Linq.JToken R = UserData.GetUser(ID, IDVariant);
            if (R["Status"].ToString() != "200") { return -1; }
            Newtonsoft.Json.Linq.JToken User = R["Data"];
            return int.Parse(User["Account"]["Balance"].ToString());
        }

        public static bool GiveUser(string ID, IDType IDVariant,int Amount)
        {
            UserData.CreateUser(ID, IDVariant);
            Amount = Math.Abs(Amount);
            bool Response = false;
            
            Newtonsoft.Json.Linq.JToken R = UserData.GetUser(ID, IDVariant);
            if (R["Status"].ToString() != "200") { return false; }
            Newtonsoft.Json.Linq.JToken User = R["Data"];
            Dictionary<string,string> Headers = new Dictionary<string,string> { };
            Headers.Add("Value", Amount.ToString());
            Newtonsoft.Json.Linq.JToken Resp=WebRequests.POST("/account/give/" + User["UserId"].ToString(), Headers);
            Response = Resp["Status"].ToString()=="200";
            return Response;
        }

        public static bool TakeUser(string ID, IDType IDVariant, int Amount)
        {
            UserData.CreateUser(ID, IDVariant);
            Amount = Math.Abs(Amount);
            bool Response = false;

            Newtonsoft.Json.Linq.JToken R = UserData.GetUser(ID, IDVariant);
            if (R["Status"].ToString() != "200") { return false; }
            Newtonsoft.Json.Linq.JToken User = R["Data"];
            Dictionary<string, string> Headers = new Dictionary<string, string> { };
            Headers.Add("Value", Amount.ToString());
            Newtonsoft.Json.Linq.JToken Resp = WebRequests.POST("/account/take/" + User["UserId"].ToString(), Headers);
            Response = Resp["Status"].ToString() == "200";
            return Response;
        }

        public static bool SetUser(string ID, IDType IDVariant, int Amount)
        {
            UserData.CreateUser(ID, IDVariant);
            Amount = Math.Abs(Amount);
            bool Response = false;

            Newtonsoft.Json.Linq.JToken R = UserData.GetUser(ID, IDVariant);
            if (R["Status"].ToString() != "200") { return false; }
            Newtonsoft.Json.Linq.JToken User = R["Data"];
            Dictionary<string, string> Headers = new Dictionary<string, string> { };
            Headers.Add("Value", Amount.ToString());
            Newtonsoft.Json.Linq.JToken Resp = WebRequests.POST("/account/set/" + User["UserId"].ToString(), Headers);
            Response = Resp["Status"].ToString() == "200";
            return Response;
        }

        public static EventResponse PayUser(string MyID, IDType MyIDType, string TheirID, IDType TheirIDType, int Amount)
        {
            Amount = Math.Abs(Amount);
            EventResponse Response = new EventResponse();

            UserData.CreateUser(TheirID, TheirIDType);

            if (UserData.UserExists(MyID, MyIDType) && UserData.UserExists(TheirID, TheirIDType))
            {
                Newtonsoft.Json.Linq.JToken MyUser = UserData.GetUser(MyID, MyIDType),
                    TheirUser=UserData.GetUser(TheirID,TheirIDType);
                if (MyUser["Status"].ToString() != "200" || TheirUser["Status"].ToString() != "200") { Response.Message = Shared.ConfigHandler.Config["CommandResponses"]["Errors"]["WhoKnows"].ToString(); return Response; }
                MyUser = MyUser["Data"]; TheirUser = TheirUser["Data"];
                if (MyUser["UserId"].ToString() == TheirUser["UserId"].ToString()) { Response.Message= Shared.ConfigHandler.Config["CommandResponses"]["Errors"]["Self"].ToString(); return Response; }
                int MyBal = int.Parse(MyUser["Account"]["Balance"].ToString()),
                    TheirBal = int.Parse(TheirUser["Account"]["Balance"].ToString());
                if (MyBal >= Amount)
                {
                    Dictionary<string, string> Headers = new Dictionary<string, string> { };
                    Headers.Add("Value", Amount.ToString());
                    WebRequests.POST("/account/take/"+MyUser["UserId"],Headers);
                    WebRequests.POST("/account/give/"+TheirUser["UserId"],Headers);
                    Response.Message = " Payment of "+Amount+" Owlcoin Complete, New Balance: "+MyBal+" Owlcoin!";
                    Response.Success = true;
                }
                else { Response.Message = Shared.ConfigHandler.Config["CommandResponses"]["Errors"]["NotEnough"].ToString(); }
            }
            else { Response.Message = Shared.ConfigHandler.Config["CommandResponses"]["Errors"]["NoUser"].ToString(); }

            return Response;
        }

    }
}
