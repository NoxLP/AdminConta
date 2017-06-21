using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AdConta;
using AdConta.Models;

namespace ModuloContabilidad.ObjModels
{
    public class Pago : GastosPagosBase
    {
        public Pago(int id, int idComunidad, int idProveedor, int? idFactura, DateTime? fecha) : base(id, idComunidad, idProveedor, idFactura, fecha)
        { }
    }

}
