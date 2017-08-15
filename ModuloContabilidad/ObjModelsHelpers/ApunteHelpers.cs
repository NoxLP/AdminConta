using ModuloContabilidad.ObjModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AdConta;
using AdConta.ViewModel;

namespace ModuloContabilidad
{
    public class ApunteParaVistaMayor : Apunte
    {
        public ApunteParaVistaMayor(Apunte apunte, decimal saldoEnCuenta)
            : base(apunte.Id, apunte.IdOwnerComunidad, apunte.Asiento, apunte.Factura, apunte.OrdenEnAsiento, apunte.DebeHaber, apunte.Importe,
                  apunte.Concepto, apunte.Cuenta, apunte.Punteo)
        {
            this.Fecha = apunte.Asiento.Fecha;
            this.SaldoEnCuenta = saldoEnCuenta;
            this.NAsiento = apunte.Asiento.Codigo.CurrentCodigo;
        }

        public DateTime Fecha { get; private set; }
        public decimal SaldoEnCuenta { get; set; }
        public int NAsiento { get; private set; }
    }
}
