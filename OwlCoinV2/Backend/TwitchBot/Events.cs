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
            Shared.Data.Accounts.GiveUser(Id, Shared.IDType.Twitch, 1000);
            Bot.TwitchC.SendMessage(e.Channel, "@" + e.GiftedSubscription.DisplayName + " Gifted a sub too @" + e.GiftedSubscription.MsgParamRecipientDisplayName + " and received 1000 Owlcoin!");
        }

        public static void Subbed(object sender, OnNewSubscriberArgs e)
        {
            Shared.Data.UserData.CreateUser(e.Subscriber.UserId, Shared.IDType.Twitch);
            Shared.Data.Accounts.GiveUser(e.Subscriber.UserId, Shared.IDType.Twitch, 1000);
            Bot.TwitchC.SendMessage(e.Channel, "@" + e.Subscriber.DisplayName + " Subbed for the first time and received 1000 Owlcoin!");
        }
        public static void ReSubbed(object sender,OnReSubscriberArgs e)
        {
            Shared.Data.UserData.CreateUser(e.ReSubscriber.UserId, Shared.IDType.Twitch);
            Shared.Data.Accounts.GiveUser(e.ReSubscriber.UserId, Shared.IDType.Twitch, 1000);
            Bot.TwitchC.SendMessage(e.Channel, "@" + e.ReSubscriber.DisplayName + " Resubbed and received 1000 Owlcoin!");
        }
        static List<String[]> Hosts = new List<string[]> { };
        public static void Hosting(object sender,OnBeingHostedArgs e)
        {
            if (Hosts.Contains(new string[] { e.BeingHostedNotification.HostedByChannel,DateTime.Now.DayOfYear.ToString() })) { return; }
            Shared.Data.UserData.CreateUser(e.BeingHostedNotification.HostedByChannel, Shared.IDType.Twitch);
            Shared.Data.Accounts.GiveUser(e.BeingHostedNotification.HostedByChannel, Shared.IDType.Twitch, 250);
            Bot.TwitchC.SendMessage(e.BeingHostedNotification.Channel, "@" + e.BeingHostedNotification.HostedByChannel + " Started hosting with "+e.BeingHostedNotification.Viewers+" viewers and received 250 Owlcoin!");
            Hosts.Add(new string[] { e.BeingHostedNotification.HostedByChannel,DateTime.Now.DayOfYear.ToString() });
        }

    }
}
