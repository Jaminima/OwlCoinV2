using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OwlCoinV2.Backend.Shared
{
    public static class InputVerification
    {
        public static bool ContainsLetter(string Str)
        {
            foreach (char C in Str) { if (!"0123456789".Contains(C.ToString().ToLower())) { return true; } }
            return false;
        }
    }
}
