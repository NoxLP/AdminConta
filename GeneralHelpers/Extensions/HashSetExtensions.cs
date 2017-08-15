using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Extensions
{
    public static class HashSetExtensions
    {
        public static HashSet<T> AddAndGetUpdatedHashSet<T>(this HashSet<T> source, T item)
        {
            source.Add(item);
            return source;
        }
    }
}
