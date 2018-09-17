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
            Shared.Data.Accounts.GiveUser(e.GiftedSubscription.Id, Shared.IDType.Twitch, 2500);
            //Bot.TwitchC.SendMessage(e.Channel, "@" + e.GiftedSubscription.DisplayName + " Gifted a sub too @" + e.GiftedSubscription.MsgParamRecipientDisplayName + "!");
            Console.WriteLine("SubGifted");
        }

        public static void Subbed(object sender, OnNewSubscriberArgs e)
        {
            Shared.Data.UserData.CreateUser(e.Subscriber.Id, Shared.IDType.Twitch);
            Shared.Data.Accounts.GiveUser(e.Subscriber.Id, Shared.IDType.Twitch, 2500);
            Console.WriteLine("Subbed");
        }
        public static void ReSubbed(object sender,OnReSubscriberArgs e)
        {
            Shared.Data.UserData.CreateUser(e.ReSubscriber.Id, Shared.IDType.Twitch);
            Shared.Data.Accounts.GiveUser(e.ReSubscriber.Id, Shared.IDType.Twitch, 2500);
            Console.WriteLine("ReSub");
        }

        public static void Hosting(object sender,OnHostingStartedArgs e)
        {
            Shared.Data.UserData.CreateUser(e.HostingStarted.TargetChannel, Shared.IDType.Twitch);
            Shared.Data.Accounts.GiveUser(e.HostingStarted.TargetChannel, Shared.IDType.Twitch, 250);
            Console.WriteLine("Hosting");
        }

    }
}
