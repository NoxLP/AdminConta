using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdConta.ViewModel
{
    /// <summary>
    /// Abstract class with helpers for properties to get and set data from datatable field.
    /// </summary>
    public class DataTableHelperVMBase : ViewModelBase
    {
        /// <summary>
        /// Gets value of type T from datatable column using ConvertFromDBVal. Doesn't check if value is of type T. Only one row supposed(f.i. tab cdades).
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="column"></param>
        /// <returns></returns>
        public virtual T GetValueFromTable<T>(string column)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// Gets value of type T from datatable column and row using ConvertFromDBVal. Doesn't check if value is of type T. Only one row supposed(f.i. tab cdades).
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="column"></param>
        /// <param name="row"></param>
        /// <returns></returns>
        public virtual T GetValueFromTable<T>(string column, int row)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// Set value of datatable column. Only one row supposed(f.i. tab cdades).
        /// </summary>
        /// <param name="column"></param>
        /// <param name="value"></param>
        public virtual void SetValueToTable(string column, object value)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// Convert DBVal to T. Doesn't check if data value is of type T.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public virtual T ConvertFromDBVal<T>(object obj)
        {
            if (obj == null || obj == DBNull.Value)
            {
                return default(T); // returns the default value for the type
            }
            else
            {
                return (T)obj;
            }
        }
    }
}
