using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AdConta.ViewModel;

namespace AdConta.ViewModel
{
    public class VMTabDiario : VMTabBase
    {
        public VMTabDiario()
        {
            base.Type = TabType.Diario;
        }

        #region datatablehelpers overrided methods
        public override T GetValueFromTable<T>(string column)
        {
            throw new NotImplementedException();
        }
        public override void SetValueToTable(string column, object value)
        {
            throw new NotImplementedException();
        }
        public override T ConvertFromDBVal<T>(object obj)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}