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
            Shared.Data.UserData.CreateUser(e.GiftedSubscription.Id, Shared.IDType.Twitch);
            Shared.Data.Accounts.GiveUser(e.GiftedSubscription.Id, Shared.IDType.Twitch, 1000);
            Bot.TwitchC.SendMessage(e.Channel, "@" + e.GiftedSubscription.DisplayName + " Gifted a sub too @" + e.GiftedSubscription.MsgParamRecipientDisplayName + " and received 1000 Owlcoin!");
        }

        public static void Subbed(object sender, OnNewSubscriberArgs e)
        {
            Shared.Data.UserData.CreateUser(e.Subscriber.Id, Shared.IDType.Twitch);
            Shared.Data.Accounts.GiveUser(e.Subscriber.Id, Shared.IDType.Twitch, 1000);
            Bot.TwitchC.SendMessage(e.Channel, "@" + e.Subscriber.DisplayName + " Subbed for the first time and received 1000 Owlcoin!");
        }
        public static void ReSubbed(object sender,OnReSubscriberArgs e)
        {
            Shared.Data.UserData.CreateUser(e.ReSubscriber.Id, Shared.IDType.Twitch);
            Shared.Data.Accounts.GiveUser(e.ReSubscriber.Id, Shared.IDType.Twitch, 1000);
            Bot.TwitchC.SendMessage(e.Channel, "@" + e.ReSubscriber.DisplayName + " Resubbed and received 1000 Owlcoin!");
        }
        static List<String[]> Hosts = new List<string[]> { };
        public static void Hosting(object sender,OnHostingStartedArgs e)
        {
            if (Hosts.Contains(new string[] { e.HostingStarted.HostingChannel,DateTime.Now.DayOfYear.ToString() })) { return; }
            Shared.Data.UserData.CreateUser(e.HostingStarted.HostingChannel, Shared.IDType.Twitch);
            Shared.Data.Accounts.GiveUser(e.HostingStarted.HostingChannel, Shared.IDType.Twitch, 250);
            Bot.TwitchC.SendMessage(e.HostingStarted.TargetChannel, "@" + e.HostingStarted.HostingChannel + " Started hosting with "+e.HostingStarted.Viewers+" viewers and received 250 Owlcoin!");
            Hosts.Add(new string[] { e.HostingStarted.HostingChannel,DateTime.Now.DayOfYear.ToString() });
        }

    }
}
