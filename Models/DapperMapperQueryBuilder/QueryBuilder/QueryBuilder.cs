using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;
using System.Dynamic;
using System.Reflection;
using MQBStatic;
using Exceptions;
using Extensions;
using AdConta.Models;
using System.Threading;
using System.Text.RegularExpressions;

namespace QBuilder
{
    public class QueryBuilder : StringSQLBuilder
    {
        public QueryBuilder()
        {
            _Parameters = new ExpandoObject() as IDictionary<string, object>;
        }

        #region fields
        private IDictionary<string, object> _Parameters;
        #endregion

        #region properties
        public ExpandoObject Parameters { get { return this._Parameters as ExpandoObject; } }
        #endregion

        #region helpers
        public int CountNumberOfOcurrencesInQuery(string stringToCount)
        {
            //https://stackoverflow.com/questions/15577464/how-to-count-of-sub-string-occurrences
            return Regex.Matches(this.Query, Regex.Escape("UPDATE")).Count;
        }
        public static string MakeParameter<T, TMember>(Expression<Func<T, TMember>> expression)
        {
            //http://stackoverflow.com/questions/273941/get-property-name-and-type-using-lambda-expression
            var member = expression.Body as MemberExpression;
            if (member != null)
                return member.Member.Name.PutAhead("@");

            throw new CustomException_StringSQLBuilder($"QBuilder.MakeParameter:Expression {expression} is not a member access");
        }
        /// <summary>
        /// Add "@" to the parameter name.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public void StoreParameter(string name, object value)
        {
            _Parameters.Add($"@{name}", value);
        }
        /// <summary>
        /// Only works with c# built-in objects. Name of the params is: "@{paramsPrefix}{param.ToString}".
        /// </summary>
        /// <param name="paramsObjects"></param>
        public void StoreParameters(IEnumerable<object> paramsObjects, string paramsPrefix)
        {
            foreach (object param in paramsObjects) _Parameters.Add($"@{paramsPrefix}{param}", param);
        }
        public void StoreParametersFrom<T>(T obj, string paramSuffix = "")
        {
            Type t = typeof(T);

            foreach (PropertyInfo pInfo in _QBPropertyInfos[t]) this._Parameters.Add(pInfo.Name + paramSuffix, pInfo.GetValue(obj));
            foreach (FieldInfo fInfo in _QBFieldInfos[t]) this._Parameters.Add(RemoveFieldsUnderscore(fInfo.Name) + paramSuffix, fInfo.GetValue(obj));
        }
        public void StoreParametersFrom<T>(T obj, IEnumerable<string> columns, string paramSuffix = "")
        {
            Type t = typeof(T);

            foreach (PropertyInfo pInfo in _QBPropertyInfos[t].Where(propertyInfo => columns.Contains(propertyInfo.Name)))
                this._Parameters.Add(pInfo.Name + paramSuffix, pInfo.GetValue(obj));
            foreach (FieldInfo fInfo in _QBFieldInfos[t].Where(propertyInfo => columns.Contains(propertyInfo.Name)))
                this._Parameters.Add(RemoveFieldsUnderscore(fInfo.Name) + paramSuffix, fInfo.GetValue(obj));
        }
        public void StoreParametersFrom<T>(IEnumerable<T> objects, IEnumerable<string> columns, string paramSuffix = "")
        {
            Type t = typeof(T);
            List<T> objectsList = objects.ToList();
            SemaphoreSlim sphr = new SemaphoreSlim(3);
            Parallel.For(0, objects.Count(), i =>
            {
                foreach (PropertyInfo pInfo in _QBPropertyInfos[t].Where(propertyInfo => columns.Contains(propertyInfo.Name)))
                {
                    sphr.Wait();
                    this._Parameters.Add(pInfo.Name + i.ToString() + paramSuffix, pInfo.GetValue(objectsList[i]));
                    sphr.Release();
                }
                foreach (FieldInfo fInfo in _QBFieldInfos[t].Where(propertyInfo => columns.Contains(propertyInfo.Name)))
                {
                    sphr.Wait();
                    this._Parameters.Add(RemoveFieldsUnderscore(fInfo.Name) + i.ToString() + paramSuffix, fInfo.GetValue(objectsList[i]));
                    sphr.Release();
                }
            });
        }
        #endregion

        #region public methods
        public string GetObject<T>(int id)
        {
            Type t = typeof(T);
            Select(t);
            From(t);
            Where(new SQLCondition("Id", "@Id"));
            StoreParameter("Id", id);
            return Query;
        }
        public string GetObject<T>(int id, string tableName)
        {
            Type t = typeof(T);
            Select(t);
            From(tableName);
            Where(new SQLCondition("Id", "@Id"));
            StoreParameter("Id", id);
            return Query;
        }
        public string GetObject<T>(IEnumerable<int> ids)
        {
            Type t = typeof(T);
            Select(t);
            From(t);
            Append("WHERE Id IN( ");

            int i = 0;
            foreach (int id in ids)
            {
                Query = Query.Append("@Id", i.ToString(), ",");
                StoreParameter($"Id{i.ToString()}", id);
                i++;
            }
            CloseBrackets();
            return Query;
        }
        public string GetObject<T>(IEnumerable<int> ids, string tableName)
        {
            Type t = typeof(T);
            Select(t);
            From(tableName);
            Append("WHERE Id IN( ");

            int i = 0;
            foreach (int id in ids)
            {
                Query = Query.Append("@Id", i.ToString(), ",");
                StoreParameter($"Id{i.ToString()}", id);
                i++;
            }
            CloseBrackets();
            return Query;
        }
        public string InsertObject<T>(T obj)
        {
            Type t = typeof(T);
            InsertInto(t);
            InsertFirstColumns(t);
            CloseBrackets();
            InsertValues(t);
            CloseBrackets();
            StoreParametersFrom<T>(obj);
            return Query;
        }
        public string InsertObject<T>(T obj, string tableName)
        {
            Type t = typeof(T);
            InsertInto(tableName);
            InsertFirstColumns(t);
            CloseBrackets();
            InsertValues(t);
            CloseBrackets();
            StoreParametersFrom<T>(obj);
            return Query;
        }
        public string InsertObject<T>(IEnumerable<T> objs)
        {
            Type t = typeof(T);
            InsertInto(t);
            InsertFirstColumns(t);
            CloseBrackets();
            InsertValues();
                        
            int i = 0;
            foreach (T obj in objs)
            {
                OpenBrackets();
                InsertValues(t, i.ToString());
                StoreParametersFrom(obj, i.ToString());
                CloseBrackets();
                Comma();
                i++;
            }
            Query = Query.Remove(Query.Length);
            return Query;
        }
        public string InsertObject<T>(IEnumerable<T> objs, string tableName)
        {
            Type t = typeof(T);
            InsertInto(tableName);
            InsertFirstColumns(t);
            CloseBrackets();
            InsertValues();

            int i = 0;
            foreach (T obj in objs)
            {
                OpenBrackets();
                InsertValues(t, i.ToString());
                StoreParametersFrom(obj, i.ToString());
                CloseBrackets();
                Comma();
                i++;
            }
            Query = Query.Remove(Query.Length);
            return Query;
        }
        public string UpdateObject<T>(T obj) where T : IObjModelBase
        {
            Type t = typeof(T);
            base.Update(t);
            UpdateSet(t);
            Where(new SQLCondition("Id", "@Id"));
            StoreParametersFrom(obj);
            return Query;
        }
        public string UpdateObject<T>(T obj, string tableName) where T : IObjModelBase
        {
            Type t = typeof(T);
            base.Update(tableName);
            UpdateSet(t);
            Where(new SQLCondition("Id", "@Id"));
            StoreParametersFrom(obj);
            return Query;
        }
        public string DeleteObject<T>(T obj) where T : IObjModelBase
        {
            Type t = typeof(T);
            DeleteFrom(t);
            Where(new SQLCondition("Id", "@Id"));
            _Parameters.Add("Id", obj.Id);
            return Query;
        }
        public string DeleteObject<T>(T obj, string tableName) where T : IObjModelBase
        {
            Type t = typeof(T);
            DeleteFrom(tableName);
            Where(new SQLCondition("Id", "@Id"));
            _Parameters.Add("Id", obj.Id);
            return Query;
        }
        #endregion
    }

}
