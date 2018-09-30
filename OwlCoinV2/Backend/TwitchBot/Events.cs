using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwitchLib.Client;
using TwitchLib.Client.Models;
using TwitchLib.Client.Events;

namespace OwlCoinV2.Backend.TwitchBot
{
    public static class Events
    {
        public static void SubGifted(object sender,OnGiftedSubscriptionArgs e)
        {
            string Id = UserHandler.UserFromUsername(e.GiftedSubscription.DisplayName).Matches[0].Id;
            Shared.Data.UserData.CreateUser(Id, Shared.IDType.Twitch);
            int Reward = int.Parse(Shared.ConfigHandler.Config["Rewards"]["Twitch"]["GiftedSub"].ToString());
            Shared.Data.Accounts.GiveUser(Id, Shared.IDType.Twitch, Reward);
            MessageHandler.SendMessage(e.Channel,e.GiftedSubscription.DisplayName, Shared.ConfigHandler.Config["EventMessages"]["GiftedSub"].ToString(), e.GiftedSubscription.MsgParamRecipientDisplayName, Reward);
        }

        public static void Subbed(object sender, OnNewSubscriberArgs e)
        {
            Shared.Data.UserData.CreateUser(e.Subscriber.UserId, Shared.IDType.Twitch);
            int Reward = int.Parse(Shared.ConfigHandler.Config["Rewards"]["Twitch"]["Subbed"].ToString());
            Shared.Data.Accounts.GiveUser(e.Subscriber.UserId, Shared.IDType.Twitch, Reward);
            MessageHandler.SendMessage(e.Channel, e.Subscriber.DisplayName, Shared.ConfigHandler.Config["EventMessages"]["Subbed"].ToString(),null, Reward);
        }
        public static void ReSubbed(object sender,OnReSubscriberArgs e)
        {
            Shared.Data.UserData.CreateUser(e.ReSubscriber.UserId, Shared.IDType.Twitch);
            int Reward = int.Parse(Shared.ConfigHandler.Config["Rewards"]["Twitch"]["ReSubbed"].ToString());
            Shared.Data.Accounts.GiveUser(e.ReSubscriber.UserId, Shared.IDType.Twitch, Reward);
            MessageHandler.SendMessage(e.Channel,e.ReSubscriber.DisplayName, Shared.ConfigHandler.Config["EventMessages"]["ReSubbed"].ToString(),null, Reward);
        }
        //static List<String[]> Hosts = new List<string[]> { };
        //public static void Hosting(object sender,OnBeingHostedArgs e)
        //{
        //    if (Hosts.Contains(new string[] { e.BeingHostedNotification.HostedByChannel,DateTime.Now.DayOfYear.ToString() })) { return; }
        //    Shared.Data.UserData.CreateUser(e.BeingHostedNotification.HostedByChannel, Shared.IDType.Twitch);
        //    Shared.Data.Accounts.GiveUser(e.BeingHostedNotification.HostedByChannel, Shared.IDType.Twitch, 250);
        //    Bot.TwitchC.SendMessage(e.BeingHostedNotification.Channel, "@" + e.BeingHostedNotification.HostedByChannel + " Started hosting with "+e.BeingHostedNotification.Viewers+" viewers and received 250 Owlcoin!");
        //    Hosts.Add(new string[] { e.BeingHostedNotification.HostedByChannel,DateTime.Now.DayOfYear.ToString() });
        //}

    }
}
