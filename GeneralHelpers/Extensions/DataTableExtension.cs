using System.Collections.Generic;
using System.Data;

namespace Extensions
{
    public static class DataTableExtension
    {
        public static T GetColumnMaxValue<T>(this DataTable table, int column, IComparer<T> comparer)
        {
            T max = default(T);
            for (int i = 0; i < table.Rows.Count; i++)
            {
                T data = (T)table.Rows[i][column];
                if ((comparer != null && comparer.Compare(data, max) > 0) ||
                    Comparer<T>.Default.Compare(data, max) > 0)
                    max = data;
            }
            return max;
        }

        public static decimal GetColumnSum(this DataTable table, int column)
        {
            if (table.Columns[column].DataType != typeof(decimal))
                return 0;

            decimal sum = 0;
            for (int i = 0; i < table.Rows.Count; i++)
            {
                sum += ((decimal)table.Rows[i][column]);
            }

            return sum;
        }
    }
}
