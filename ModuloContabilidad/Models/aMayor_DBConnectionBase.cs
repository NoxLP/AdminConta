using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AdConta.Models;
using ModuloContabilidad.ObjModels;

namespace ModuloContabilidad.Models
{
    public abstract class aMayor_DBConnectionBase : aDBConnectionBase
    {
        public virtual void CreateTable(string tableName, string cod, CuentaMayor acc)
        {
            throw new NotImplementedException();
        }        
    }
}