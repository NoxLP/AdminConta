using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Extensions
{
    public static class DecimalExtensions
    {
        public static decimal MultiplyDouble(this decimal dec, double other)
        {
            decimal dOther = (decimal)other;

            return dec * dOther;
        }
    }
}
