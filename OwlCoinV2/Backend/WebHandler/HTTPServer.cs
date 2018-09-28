using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace OwlCoinV2.Backend.WebHandler
{
    public static class HTTPServer
    {
        static HttpListener Listener = new HttpListener();
        public static void Start()
        {
            Listener.Prefixes.Add("http://+:1234/");
            Listener.Start();
            Listener.BeginGetContext(HandleRequest, null);
        }
        public static HttpListenerContext GetRequestData(IAsyncResult Request)
        {
            HttpListenerContext Data = Listener.EndGetContext(Request);
            Listener.BeginGetContext(HandleRequest, null);
            return Data;
        }

        public static void HandleRequest(IAsyncResult Request)
        {
            HttpListenerContext RequestData = HTTPServer.GetRequestData(Request);
            RecivedGET(RequestData);
        }

        static void RecivedGET(HttpListenerContext RequestData)
        {
            HttpListenerResponse Response = RequestData.Response;
            Response.StatusCode = 200;
            Response.ContentType = "application/json";
            string ResponseData = Handler(RequestData);
            byte[] ByteResponseData = Encoding.UTF8.GetBytes(ResponseData);
            Response.OutputStream.Write(ByteResponseData, 0, ByteResponseData.Length);
            Response.OutputStream.Close();
        }
        static DateTime LastLeaderboardPoll;
        static string LastLeaderboard = "";
        public static string Handler(HttpListenerContext RequestData)
        {
            if (RequestData.Request.Url.ToString().Contains("/Data/Leaderboard/"))
            {
                if ((DateTime.Now - LastLeaderboardPoll).Minutes > 5)
                {
                    List<String[]> LeaderBoard = Init.SQLInstance.ExecuteReaderBetter(Init.SQLInstance.GetCommand(@"SELECT UserData.TwitchID, Accounts.Balance
FROM UserData INNER JOIN Accounts ON UserData.OwlCoinID = Accounts.OwlCoinID
WHERE UserData.TwitchID<>''
ORDER BY Accounts.Balance DESC
"));
                    string ReturnData = "{\"Items\":" + LeaderBoard.Count + ",\"Data\":[";
                    for (int i = 0; i < LeaderBoard.Count && i < 15; i++)
                    {
                        try { ReturnData += "{\"UserName\":\"" + TwitchBot.UserHandler.UserFromUserID(LeaderBoard[i][0]).Name + "\",\"OwlCoin\":" + LeaderBoard[i][1] + "}"; } catch { }
                    }
                    ReturnData = ReturnData.Replace("}{", "},{");
                    ReturnData += "]}";
                    LastLeaderboard = ReturnData;
                    LastLeaderboardPoll = DateTime.Now;
                    return ReturnData;
                }
                else { return LastLeaderboard; }
                //return "{\"Items\":2,\"Data\":[{\"UserName\":\"Jaminima\",\"OwlCoin\":1000},{\"UserName\":\"Alex_T\",\"OwlCoin\":69}]}";
            }
            return null;
        }
    }
}
