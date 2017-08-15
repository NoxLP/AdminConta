using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModuloContabilidad.ObjModels
{
    public class ApunteSaldoEnCuentaComparer : IComparer<Tuple<Apunte, decimal>>
    {
        public int Compare(Tuple<Apunte, decimal> x, Tuple<Apunte, decimal> y)
        {
            if (x == null || y == null)
            {
                if (x == null && y == null) return 0;
                if (y == null) return 1;
                return -1;
            }
            if (x.Item1.Asiento.Fecha == y.Item1.Asiento.Fecha) return 0;
            if (x.Item1.Asiento.Fecha > y.Item1.Asiento.Fecha) return 1;
            return -1;
        }
    }
}
