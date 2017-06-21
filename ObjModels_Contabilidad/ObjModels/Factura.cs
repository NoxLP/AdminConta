using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AdConta;
using AdConta.Models;
using Extensions;

namespace ModuloContabilidad.ObjModels
{
    public class Factura : IObjModelBase, IOwnerProveedor, IOwnerComunidad, IObjWithDLO<FacturaDLO>
    {
        #region constructors
        private Factura() { }

        public Factura(
            int id, 
            int idCdad, 
            int? idProv, 
            string nFactura, 
            DateTime? fecha,
            bool autoCalc,
            decimal subtotal,
            double puIi,
            double puIrpf,
            decimal ii,
            decimal irpf,
            decimal pendiente,
            string concepto,
            TipoPagoFacturas tipoPago = TipoPagoFacturas.Cheque,
            GastosPagosList<Gasto> gastos = null,
            GastosPagosList<Pago> pagos = null)
        {
            decimal II = subtotal.MultiplyDouble(puIi);
            decimal Irpf = subtotal.MultiplyDouble(puIrpf);

            if (ii != II || irpf != Irpf)
                throw new CustomException_ObjModels("Error al intentar crear objeto Factura en constructor. Factura no cuadrada");

            decimal total = subtotal + ii - irpf;

            if((gastos != null && gastos.Total != subtotal) ||
                (pagos != null && (total - pagos.Total) != pendiente))
                throw new CustomException_ObjModels("Error al intentar crear objeto Factura en constructor. Factura no cuadrada");

            this._Id = id;
            this._IdOwnerComunidad = idCdad;
            this._IdOwnerProveedor = idProv;
            this._NFactura = nFactura;
            this.Fecha = (DateTime) (fecha ?? DateTime.Today);
            this.AutoCalc = autoCalc;
            this._Subtotal = subtotal;
            this._PerUnitIGICIVA = puIi;
            this._IGICIVA = ii;
            this._PerUnitIRPF = puIrpf;
            this._IRPF = irpf;
            this._Pendiente = pendiente;
            this.Concepto = concepto;
            this.TipoPago = tipoPago;
            this._GastosFra = gastos;
            this._PagosFra = pagos;
        }
        #endregion
        
        #region fields
        private int _Id;
        private int? _IdOwnerProveedor;
        private int _IdOwnerComunidad;
        private string _NFactura;

        private GastosPagosList<Gasto> _GastosFra;
        private GastosPagosList<Pago> _PagosFra;
        
        private decimal _Subtotal;
        private double _PerUnitIGICIVA;
        private decimal _IGICIVA;
        private double _PerUnitIRPF;
        private decimal _IRPF;
#pragma warning disable CS0169
        private decimal _Total;
#pragma warning restore CS0169
        private decimal _Pendiente;
        #endregion

        #region properties
        public int Id { get { return this._Id; } }
        public int? IdOwnerProveedor { get { return this._IdOwnerProveedor; } }
        public int IdOwnerComunidad { get { return this._IdOwnerComunidad; } }

        public string NFactura { get { return this._NFactura; } }

        public ReadOnlyGastosPagosList<Gasto> GastosFra { get { return this._GastosFra.AsReadOnly(); } }
        public ReadOnlyGastosPagosList<Pago> PagosFra { get { return this._PagosFra.AsReadOnly(); } }

        public DateTime Fecha { get; set; }

        public bool AutoCalc { get; set; }
        public decimal Subtotal { get { return this._Subtotal; } }
        public double PerUnitIGICIVA { get { return this._PerUnitIGICIVA; } }
        public decimal IGICIVA { get { return this._IGICIVA; } }
        public double PerUnitIRPF { get { return this._PerUnitIRPF; } }
        public decimal IRPF { get { return this._IRPF; } }
        public decimal TotalImpuestos { get { return this.IGICIVA - this.IRPF; } }
        
        public decimal Total { get { return (this.Subtotal + IGICIVA - IRPF); } }
        public decimal Pendiente { get { return this._Pendiente; } }
        
        public string Concepto { get; set; }
        public TipoPagoFacturas TipoPago { get; set; }
        #endregion

        /*#region helpers        
        public bool FacturaLiteralCuadrada(decimal subtotal, double perUnitII, double perUnitIRPF)
        {
            decimal II = subtotal.MultiplyDouble(perUnitII);
            decimal Irpf = subtotal.MultiplyDouble(perUnitIRPF);

            return (subtotal + II - Irpf) == this.Total;
        }
        public bool FacturaLiteralCuadrada(decimal subtotal, decimal igiciva, decimal irpf)
        {
            return (subtotal + igiciva - irpf) == this.Total;
        }
        public bool GastosCuadranFactura(decimal gastos, decimal subtotal, double perUnitII, double perUnitIRPF)
        {
            decimal II = subtotal.MultiplyDouble(perUnitII);
            decimal Irpf = subtotal.MultiplyDouble(perUnitIRPF);

            return (subtotal + II - Irpf) == this.Total;
        }

        private void ReCalculate()
        {
            this._IGICIVA = this.Subtotal.MultiplyDouble(this.PerUnitIGICIVA);
            this._IRPF = this.Subtotal.MultiplyDouble(this.PerUnitIRPF);
            this._Total = this.Subtotal + this.IGICIVA - this.IRPF;
            this._Pendiente = this.Total - this.PagosFra.Total;
        }
        #endregion*/

        #region public methods
        public ErrorCuadreFactura FacturaCuadrada(
            decimal subtotal, 
            double perUnitIGICIVA, 
            decimal igiciva, 
            double perUnitIRPF, 
            decimal irpf,
            decimal total,
            decimal pendiente,
            GastosPagosList<Gasto> gastos, 
            GastosPagosList<Pago> pagos)
        {
            decimal II = subtotal.MultiplyDouble(perUnitIGICIVA);
            decimal Irpf = subtotal.MultiplyDouble(perUnitIRPF);

            if (igiciva != II) return ErrorCuadreFactura.ErrorEnCalculoIGICIVA;
            else if (irpf != Irpf) return ErrorCuadreFactura.ErrorEnCalculoIRPF;

            decimal Total = subtotal + igiciva - irpf;

            if (total != Total) return ErrorCuadreFactura.ErrorEnTotal;
            else if (gastos != null && gastos.Total != subtotal) return ErrorCuadreFactura.GastosNoCoincidenConSubtotal;
            else if (pagos != null && (total - pagos.Total) != pendiente) return ErrorCuadreFactura.PendienteDescuadrado;
            
            return ErrorCuadreFactura.None;
        }
        public bool TrySetGastosPagos(GastosPagosList<Gasto> gastos, GastosPagosList<Pago> pagos)
        {
            decimal pendiente = gastos.Total - pagos.Total;

            if (this._Pendiente == pendiente || (gastos.Total + this.TotalImpuestos) != this.Total) return false;

            this._GastosFra = gastos;
            this._PagosFra = pagos;
            this._Pendiente = pendiente;
            
            return true;
        }
        public bool TryAddPago(Pago pago)
        {
            if (pago.Importe > this.Pendiente) return false;

            this._PagosFra.Add(pago);
            this._Pendiente = this.GastosFra.Total - this.PagosFra.Total;
            return true;
        }
        #endregion

        #region DLO
        public FacturaDLO GetDLO()
        {
            return new FacturaDLO(Id, IdOwnerProveedor, IdOwnerComunidad, NFactura, Fecha, Concepto, Total, Pendiente, TipoPago);
        }
        #endregion
    }

    public class FacturaDLO : IObjModelBase, IDataListObject
    {
        public FacturaDLO() { }
        public FacturaDLO(
            int id,
            int? idProv,
            int idCdad,
            string nFactura,
            DateTime fecha,
            string concepto,
            decimal total,
            decimal pendiente,
            TipoPagoFacturas tipoPago)
        {
            this.Id = id;
            this.IdOwnerProveedor = idProv;
            this.IdOwnerComunidad = idCdad;
            this.NFactura = nFactura;
            this.Fecha = fecha;
            this.Concepto = concepto;
            this.Total = total;
            this.Pendiente = pendiente;
            this.TipoPago = tipoPago;
        }

        public int Id { get; private set; }
        public int? IdOwnerProveedor { get; private set; }
        public int IdOwnerComunidad { get; private set; }
        public string NFactura { get; private set; }
        public DateTime Fecha { get; private set; }
        public string Concepto { get; private set; }
        public decimal Total { get; private set; }
        public decimal Pendiente { get; private set; }
        public TipoPagoFacturas TipoPago { get; private set; }
    }
}
