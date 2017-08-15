using System;
using System.Linq.Expressions;

namespace Extensions
{
    public static class TypeExtension
    {
        //https://stackoverflow.com/questions/2093230/how-to-check-that-i-can-sum-values-of-given-type
        public static bool IsSummable<T>(this T obj)
        {
            Type type = typeof(T);
            try
            {
                ParameterExpression paramA = Expression.Parameter(type, "a"), paramB = Expression.Parameter(type, "b");
                BinaryExpression addExpression = Expression.Add(paramA, paramB);
                var add = Expression.Lambda(addExpression, paramA, paramB).Compile();
                var v = Activator.CreateInstance(type);
                add.DynamicInvoke(v, v);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
