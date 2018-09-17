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
            //Console.WriteLine(System.IO.Directory.GetCurrentDirectory());
            //Console.WriteLine(System.IO.File.Exists("./Data\\ExampleDatabase.accdb"));
            //string[] S = System.IO.Directory.GetFiles("./Data");
            while (true) { Console.ReadLine(); }
        }
    }
}
