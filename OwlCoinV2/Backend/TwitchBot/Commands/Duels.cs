using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OwlCoinV2.Backend.TwitchBot.Commands
{
    class Duels
    {
        public static SortedSet<Shared.Duel> duels = new SortedSet<Shared.Duel>(Comparer<Shared.Duel>.Create((l, r) => l.target.CompareTo(r.target)));
        public static void Handler()
        {
            while (true)
            {
                duels.RemoveWhere(duel => DateTime.Now.Subtract(duel.timestamp).Minutes >= 2);
                System.Threading.Thread.Sleep(10000);
            }
        }
    }
}
