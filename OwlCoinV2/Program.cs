using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OwlCoinV2
{
    class Program
    {
        static void Main(string[] args)
        {
            Backend.Init.Start();
            AppDomain.CurrentDomain.UnhandledException+= new UnhandledExceptionEventHandler(ExceptionHandler);
            while (true) { Console.ReadLine(); }
        }

        static void ExceptionHandler(object sender, UnhandledExceptionEventArgs args)
        {
            Exception e = (Exception)args.ExceptionObject;
            System.IO.File.WriteAllText("./Error" + DateTime.Now.ToString() + ".log", e.ToString());
            Console.WriteLine("MyHandler caught : " + e.Message);
            Console.WriteLine("Runtime terminating: {0}", args.IsTerminating);
        }

    }
}
