using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModuloContabilidad.ObjModels
{
    public class AsientoPorFechaComparer : Comparer<Asiento>
    {
        public override int Compare(Asiento x, Asiento y)
        {
            if (x == null || y == null)
            {
                if (x == null && y == null) return 0;
                if (y == null) return 1;
                return -1;
            }
            if (x.Fecha == y.Fecha) return 0;
            if (x.Fecha > y.Fecha) return 1;
            return -1;
        }
    }
}
