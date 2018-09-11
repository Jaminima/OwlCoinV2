using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OwlCoinV2.Backend.Shared
{
    class Duel
    {
        public string duelee;
        public string target;
        public int amount;
        public DateTime timestamp;

        public Duel(string duelee, string target, int amount)
        {
            this.duelee = duelee;
            this.target = target;
            this.amount = amount;
            timestamp = DateTime.Now;
        }
    }
}
