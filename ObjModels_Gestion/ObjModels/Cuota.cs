using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AdConta.Models;
using AdConta;

namespace ModuloGestion.ObjModels
{
    public class Cuota : IObjModelBase, IOwnerComunidad, IOwnerPropietario, IOwnerFinca
    {
        private Cuota() { }
        public Cuota(
            int id, 
            int idOwnerFinca, 
            int idOwnerComunidad, 
            int idOwnerPropietario,
            Ejercicio ejercicio,
            Concepto concepto,
            Date mes,
            Date caducidad,
            decimal importeTotal,
            CobrosDict cobros = null,
            SituacionReciboCobroEntaCta situacion = SituacionReciboCobroEntaCta.Normal,
            DevolucionesList devoluciones = null)
        {
            this._Id = id;
            this._IdOwnerFinca = idOwnerFinca;
            this._IdOwnerComunidad = idOwnerComunidad;
            this._IdOwnerPropietario = idOwnerPropietario;
            this._Ejercicio = ejercicio;
            this._Concepto = concepto;
            this._Mes = mes;
            this._Caducidad = caducidad;
            this._ImporteTotal = importeTotal;
            this._Cobros = cobros;
            this._Situacion = situacion;
            this._Devoluciones = devoluciones;
        }

        #region fields
        private int _Id;
        private int _IdOwnerFinca;
        private int _IdOwnerComunidad;
        private int _IdOwnerPropietario;
        private Ejercicio _Ejercicio;

        private Concepto _Concepto;

        private Date _Mes;
        private Date _Caducidad;
        private decimal _ImporteTotal;

        private CobrosDict _Cobros;
        private SituacionReciboCobroEntaCta _Situacion;
        private DevolucionesList _Devoluciones;        
        #endregion

        #region properties
        public int Id { get { return this._Id; } }
        public int IdOwnerFinca { get { return this._IdOwnerFinca; } }
        public int IdOwnerComunidad { get { return this._IdOwnerComunidad; } }
        /// <summary>
        /// OJO ¿Id propietario de la finca, o del que tiene que pagar la cuota(pagadores)?
        /// </summary>
        public int IdOwnerPropietario { get { return this._IdOwnerPropietario; } }
        public Ejercicio Ejercicio { get { return this._Ejercicio; } }

        public Concepto Concepto { get { return this._Concepto; } }
        
        public Date Mes { get { return this._Mes; } }        
        public Date Caducidad { get { return this._Caducidad; } }        
        public decimal ImporteTotal { get { return this._ImporteTotal; } }

        public CobrosDict Cobros { get { return this._Cobros; } }
        public SituacionReciboCobroEntaCta Situacion { get { return this._Situacion; } }
        public DevolucionesList Devoluciones { get { return this._Devoluciones; } }
        #endregion

        #region public methods
        public decimal GetDeuda()
        {
            return this.ImporteTotal - this.Cobros.Total + this.Devoluciones.Total + this.Devoluciones.TotalGastos;
        }
        public decimal GetDeuda(Date fechaIngresos)
        {
            decimal deuda = this.ImporteTotal;

            foreach(KeyValuePair<int,Cobro> cobro in this.Cobros.GetEnumerable())
            {
                if (cobro.Value.Fecha <= fechaIngresos) deuda -= cobro.Value.Importe;
            }
            foreach(IngresoDevuelto ingreso in this.Devoluciones.GetIngresosDevueltosEnumerable())
            {
                if (ingreso.Fecha <= fechaIngresos) deuda = deuda + ingreso.Importe + ingreso.Gastos;
            }

            return deuda;
        }
        /*public decimal GetDeuda(Date fechaInicial, Date fechaIngresos)
        {
            decimal deuda = this.ImporteTotal;

            foreach (sCobro cobro in this.Cobros.GetEnumerable())
            {
                if (cobro.Fecha >= fechaInicial && cobro.Fecha <= fechaIngresos)
                    deuda -= cobro.Importe;
            }

            return deuda;
        }*/
        #endregion
    }

}
