using System;

namespace ClashOfSnakes
{
    static class Extensions
    {
        /// <summary>
        /// Calculates positive remainder of a divided by divisor. Standard modulo (%) was not suitable for negative numbers.
        /// </summary>
        /// <param name="dividend"></param>
        /// <param name="divisor"></param>
        /// <returns>The ramainder, is not negative</returns>
        public static int PosMod(this int dividend, int divisor)
        {
            if (divisor <= 0) throw new ArgumentOutOfRangeException();

            int tmp = dividend % divisor;
            return tmp < 0 ? tmp + divisor : tmp;
        }
    }
}
