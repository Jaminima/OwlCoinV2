using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwitchLib.Client.Events;
using System.Threading;

namespace OwlCoinV2.Backend.TwitchBot
{
    public static class Logger
    {
        public static void AddToLog(object sender, OnMessageReceivedArgs e)
        {
            System.IO.File.AppendAllLines("./Data/Logs/" + DateTime.Now.Day + "-" + DateTime.Now.Month + "-" + DateTime.Now.Year + "Chat.txt", new String[] { e.ChatMessage.Username + " -- " + e.ChatMessage.Message });
        }

        public static void AddToCommandLog(string Message)
        {
            System.IO.File.AppendAllLines("./Data/Logs/" + DateTime.Now.Day + "-" + DateTime.Now.Month + "-" + DateTime.Now.Year + "Commands.txt", new string[] { Message });
        }

    }
}
