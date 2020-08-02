using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClashOfSnakes
{
    static class Extensions
    {
        public static int posMod(this int a, int divisor)
        {
            if (divisor <= 0) throw new ArgumentOutOfRangeException();

            int tmp = a % divisor;
            return tmp < 0 ? tmp + divisor : tmp;
        }
    }
}
