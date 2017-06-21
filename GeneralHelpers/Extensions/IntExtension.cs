using System;

namespace Extensions
{
    public static class IntExtension
    {
        public static long CantorPair(this int nbr, int nbr2)
        {
            return ((nbr + nbr2) * (nbr + nbr2 + 1)) / 2 + nbr2;
        }

        public static int[] ReverseCantor(this long nbr)
        {
            int[] pair = new int[2];
            long t = (long)Math.Floor((-1D + Math.Sqrt(1D + 8 * nbr)) / 2D);
            long x = t * (t + 3) / 2 - nbr;
            long y = nbr - t * (t + 1) / 2;
            pair[0] = (int)x;
            pair[1] = (int)y;
            return pair;
        }
    }
}
