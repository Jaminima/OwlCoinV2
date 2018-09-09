using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwitchLib.Client.Events;
using System.Threading;

namespace OwlCoinV2.Backend.TwitchBot
{
    public static class ChatLogger
    {
        public static void AddToLog(object sender, OnMessageReceivedArgs e)
        {
            new Thread(() => System.IO.File.AppendAllLines("./Data/Logs/" + DateTime.Now.Day + "-" + DateTime.Now.Month + "-" + DateTime.Now.Year + ".txt", new String[] { e.ChatMessage.Username + " -- " + e.ChatMessage.Message })).Start();
        }
    }
}
