using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Extensions
{
    public static class DateTimeExtensions
    {
        public static bool AreAnOfficialTrimester(this DateTime date, DateTime finalDate)
        {
            if (date.Year == finalDate.Year && finalDate.Month == (date.Month + 2)) return true;
            return false;
        }
        public static bool AreAnOfficialFourMonth(this DateTime date, DateTime finalDate)
        {
            if (date.Year == finalDate.Year && finalDate.Month == (date.Month + 3)) return true;
            return false;
        }
        public static bool AreAnOfficialSemester(this DateTime date, DateTime finalDate)
        {
            if (date.Year == finalDate.Year && finalDate.Month == (date.Month + 6)) return true;
            return false;
        }
    }
}
