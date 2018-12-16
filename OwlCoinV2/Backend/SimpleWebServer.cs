﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net;

namespace OwlCoinV2.Backend
{
    public static class SimpleWebServer
    {
        static DateTime UpSince = DateTime.Now;
        static HttpListener Listener = new HttpListener();
        public static void Start()
        {
            Listener.Prefixes.Add("http://+:774/");
            Listener.Start();
            Listener.BeginGetContext(HandleRequest, null);
        }

        public static void HandleRequest(IAsyncResult Request)
        {
            new Thread(() => RequestThread(Listener.EndGetContext(Request))).Start();
            Listener.BeginGetContext(HandleRequest, null);
        }

        public static void RequestThread(HttpListenerContext Context)
        {
            HttpListenerResponse Response = Context.Response;
            Response.StatusCode = 200; // Indicate the status as 200, ie alive
            Response.ContentType = "application/json";
            byte[] ByteResponseData = Encoding.UTF8.GetBytes(Handler(Context).ToString());
            try
            {
                Response.OutputStream.Write(ByteResponseData, 0, ByteResponseData.Length);
                Response.OutputStream.Close();
            }
            catch { Console.WriteLine("Unable to send response too " + Context.Request.RemoteEndPoint); }
        }

        public static Newtonsoft.Json.Linq.JToken Handler(HttpListenerContext Context)
        {
            string[] URLPath = Context.Request.Url.PathAndQuery.ToLower().Split("/".ToCharArray());
            if (URLPath[1] == "queue")
            {
                return NightBotReplacement.Init.Queue.ToJson();
            }
            else if (URLPath.Length == 2)
            {
                return Newtonsoft.Json.Linq.JToken.Parse("{'UpSince':'" + UpSince.ToString() + "'}");
            }
            return Newtonsoft.Json.Linq.JToken.Parse("{'Message':'That is not a known address'}");
        }

    }
}
