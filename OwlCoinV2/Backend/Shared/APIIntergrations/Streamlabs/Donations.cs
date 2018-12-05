using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;

namespace OwlCoinV2.Backend.Shared.APIIntergrations.Streamlabs
{
    public static class Donations
    {
        public static void CheckForNewDonation()
        {
            WebRequest Req = WebRequest.Create("https://streamlabs.com/api/v1.0/donations?access_token="+Alert.GetAuthCode()+"&limit=100");
            Req.Method = "GET";
            WebResponse Res; Newtonsoft.Json.Linq.JObject CurrentData,OldData=ConfigHandler.LoadConfig("./Data/Donations.dump");
            try
            {
                Res = Req.GetResponse();
                string SData = new StreamReader(Res.GetResponseStream()).ReadToEnd();
                if (SData == null) { return; }
                CurrentData = Newtonsoft.Json.Linq.JObject.Parse(SData);
            }
            catch (WebException E)
            {
                Console.WriteLine(E.Message);
                return;
            }
            ConfigHandler.SaveConfig("./Data/Donations.dump", CurrentData);
            for (int i = 0; i < OldData["data"].Count(); i++)
            {
                if (CurrentData["data"][i]["donation_id"].ToString() != OldData["data"][0]["donation_id"].ToString())
                {
                    Newtonsoft.Json.Linq.JToken CurrentDonation = CurrentData["data"][i];
                    double Amount = Math.Round(Double.Parse(CurrentDonation["amount"].ToString()), 2);
                    int Owc = (int)Math.Floor(Amount * int.Parse(Shared.ConfigHandler.Config["Rewards"]["Twitch"]["Donation"].ToString()));
                    TwitchLib.Api.Models.v5.Users.Users Users = TwitchBot.UserHandler.UserFromUsername(CurrentDonation["name"].ToString());
                    if (Users.Matches.Length != 0)
                    {
                        Data.Accounts.GiveUser(Users.Matches[0].Id, IDType.Twitch, Owc);
                        TwitchBot.MessageHandler.SendMessage(Shared.ConfigHandler.Config["ChannelName"].ToString(), Shared.ConfigHandler.Config["EventMessages"]["Donation"].ToString(), CurrentDonation["name"].ToString(), null, -1, Owc, Amount.ToString() + " " + CurrentDonation["currency"].ToString());
                    }
                }
                else { break; }
            }
        }
    }
}
