using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AdConta;
using AdConta.Models;

namespace ModuloContabilidad.ObjModels
{
    public class GastosPagosBase : IObjModelBase, IOwnerComunidad, IOwnerProveedor
    {
        public GastosPagosBase(int id, int idComunidad, int? idProveedor, int? idFactura, DateTime? fecha)
        {
            this._Id = id;
            this._Fecha = (DateTime) fecha;
            this._IdOwnerComunidad = idComunidad;
            this._IdOwnerProveedor = idProveedor;
            this._IdOwnerFactura = idFactura;
        }

        public struct sImporteCuenta
        {
            public sImporteCuenta(int id, decimal importe, string cuenta, string concepto, bool cuentaActiva = true)
            {
                this.Importe = importe;
                this.Cuenta = cuenta;
                this.Concepto = concepto;
                this.Id = id;
                this.CuentaActiva = cuentaActiva;
            }

            public int Id { get; set; }
            public decimal Importe { get; set; }
            public string Cuenta { get; set; }
            public string Concepto { get; set; }
            //TODO: Hay que hacer algo con esta variable: GastosPagosBase debe actuar con ella de alguna forma => usuario debe resolver
            /// <summary>
            /// False si la cuenta ha sido borrada en la Comunidad y ejercicio
            /// </summary>
            public bool CuentaActiva { get; set; }
        }

        /*public GastosPagosBase(
            int id,
            List<sImporteCuenta> cuentasAcreedoras,
            List<sImporteCuenta> cuentasDeudoras,
            Date fecha,
            string concepto,
            decimal importe)
        {
            this._Id = id;

            this._CuentasAcreedoras = cuentasAcreedoras;
            this._CuentasDeudoras = cuentasDeudoras;
            this._Fecha = fecha;
            this.Concepto = concepto;
            this._ImporteTotal = importe;
            this.CalculateCurrentTotal();
        }*/

        #region fields
        private int _Id;
        private int _IdOwnerComunidad;
        private int? _IdOwnerProveedor;
        private int? _IdOwnerFactura;

        private List<sImporteCuenta> _CuentasAcreedoras = new List<sImporteCuenta>();
        private List<sImporteCuenta> _CuentasDeudoras = new List<sImporteCuenta>();

        private DateTime _Fecha;
        private decimal _Importe;
        #endregion

        #region properties
        public int Id { get { return this._Id; } }
        public int IdOwnerComunidad { get { return this._IdOwnerComunidad; } }
        public int? IdOwnerProveedor { get { return this._IdOwnerProveedor; } }
        public int? IdOwnerFactura { get { return this._IdOwnerFactura; } }

        public ReadOnlyCollection<sImporteCuenta> CuentasAcreedoras
        {
            get { return this._CuentasAcreedoras.AsReadOnly(); }
        }
        public ReadOnlyCollection<sImporteCuenta> CuentasDeudoras
        {
            get { return this._CuentasDeudoras.AsReadOnly(); }
        }

        public DateTime Fecha { get { return this._Fecha; } }
        public decimal Importe { get { return this._Importe; } }
        #endregion

        #region helpers
        private bool CalculaTotal(ref List<sImporteCuenta> deudoras, ref List<sImporteCuenta> acreedoras)
        {
            decimal SaldoDeudor = 0;
            decimal SaldoAcreedor = 0;

            if(deudoras != null)
                foreach (sImporteCuenta gc in deudoras)
                    SaldoDeudor += gc.Importe;

            if(acreedoras != null)
                foreach (sImporteCuenta gc in acreedoras)
                    SaldoAcreedor += gc.Importe;

            if (SaldoDeudor != SaldoAcreedor) return false;

            this._Importe = SaldoDeudor;
            return true;
        }
        #endregion

        #region public methods
        public bool SetImportesCuentas(List<sImporteCuenta> deudores, List<sImporteCuenta> acreedores)
        {
            if (!this.CalculaTotal(ref deudores, ref acreedores)) return false;

            this._CuentasDeudoras = deudores;
            this._CuentasAcreedoras = acreedores;
            return true;
        }

        /*public bool AddCuentaDeudora(sImporteCuenta gc)
        {
            if (this.CuentasDeudoras.Contains(gc)) return false;

            this._CuentasDeudoras.Add(gc);
            this._ImporteTotal += gc.Importe;
            return true;
        }
        public bool AddCuentaAcreedora(sImporteCuenta gc)
        {
            if (this.CuentasAcreedoras.Contains(gc)) return false;

            this._CuentasAcreedoras.Add(gc);
            //this._ImporteTotal -= gc.Importe;
            return true;
        }
        public bool SetCuentasDeudoras(List<sImporteCuenta> gcList)
        {
            //foreach (sImporteCuenta gc in gcList) if (this.CuentasDeudoras.Contains(gc)) return false;

            if (this.CalculateTotal(ref gcList, ref this._CuentasAcreedoras))
            {
                this._CuentasDeudoras = gcList;
                return true;
            }

            return false;
        }
        public bool SetCuentasAcreedoras(List<sImporteCuenta> gcList)
        {
            //foreach (sImporteCuenta gc in gcList) if (this.CuentasAcreedoras.Contains(gc)) return false;

            if (this.CalculateTotal(ref this._CuentasDeudoras, ref gcList))
            {
                this._CuentasAcreedoras = gcList;
                return true;
            }

            return false;
        }
        /*public bool RemoveCuentaDeudora(sImporteCuenta gc)
        {
            if (!this.CuentasDeudoras.Contains(gc)) return false;

            int index = this.CuentasDeudoras.IndexOf(gc);
            this._CuentasDeudoras.RemoveAt(index);
            this._ImporteTotal -= gc.Importe;
            return true;
        }
        public bool RemoveCuentaAcreedora(sImporteCuenta gc)
        {
            if (!this.CuentasAcreedoras.Contains(gc)) return false;

            int index = this.CuentasAcreedoras.IndexOf(gc);
            this._CuentasAcreedoras.RemoveAt(index);
            //this.CalculateCurrentTotal();
            return true;
        }*/
        #endregion

    }
}
