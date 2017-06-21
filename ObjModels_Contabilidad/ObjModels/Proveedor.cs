using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AdConta;
using AdConta.Models;

namespace ModuloContabilidad.ObjModels
{
    public class Proveedor : Persona, IObjWithDLO<ProveedorDLO>
    {
        #region constructors
        public Proveedor(int id, int idPersona, string nif, string nombre, bool forceInvalidNIF = false) 
            : base(idPersona, nif, nombre, forceInvalidNIF)
        {
            this._IdProveedor = id;
        }

        public Proveedor(
            CuentaMayor CuentaContProveedor, 
            CuentaMayor CuentaContGasto,
            CuentaMayor CuentaContPago,
            int id,
            string Razon,
            double IGICIVA,
            double IRPF,
            TipoPagoFacturas DefTPagoFacturas,
            int idPersona, string nif, string nombre, bool forceInvalidNIF = false) : base(idPersona, nif, nombre, forceInvalidNIF)
        {
            this._CuentaContableProveedor = CuentaContableProveedor;
            this._CuentaContableGasto = CuentaContGasto;
            this._CuentaContablePago = CuentaContPago;
            this._IdProveedor = id;
            this.RazonSocial = Razon;
            this.IGICIVAPercent = IGICIVA;
            this.IRPFPercent = IRPF;
            this.DefaultTipoPagoFacturas = DefTPagoFacturas;
        }
        #endregion
        
        #region fields
        private int _IdProveedor;
        
        private CuentaMayor _CuentaContableGasto;
        private CuentaMayor _CuentaContablePago;
        private CuentaMayor _CuentaContableProveedor;
        #endregion

        #region properties
        public int IdProveedor { get { return this._IdProveedor; } }
        public string RazonSocial { get; set; }

        public CuentaMayor CuentaContableGasto { get { return this._CuentaContableGasto; } }
        public CuentaMayor CuentacontablePago { get { return this._CuentaContablePago; } }
        public CuentaMayor CuentaContableProveedor { get { return this._CuentaContableProveedor; } }

        public double IGICIVAPercent { get; set; }
        public double IRPFPercent { get; set; }
        public TipoPagoFacturas DefaultTipoPagoFacturas { get; set; }
        #endregion

        #region DLO
        public ProveedorDLO GetDLO()
        {
            return new ProveedorDLO(Id, Nombre, NIF.NIF, Direccion.GetDireccionSinCP(), CuentaBancaria.AccountNumber, Telefono1.Numero, Email, RazonSocial,
                CuentaContableGasto.NumCuenta.ToString(), CuentacontablePago.NumCuenta.ToString(), CuentaContableProveedor.NumCuenta.ToString());
        }
        #endregion
    }

    public class ProveedorDLO : IObjModelBase, IDataListObject
    {
        public ProveedorDLO() { }
        public ProveedorDLO(
            int id,
            string nombre,
            string nIF,
            string direccion,
            string cuentaBancaria,
            string telefono,
            string email,
            string razonSocial,
            string cuentaGasto,
            string cuentaPago,
            string cuentaProveedor)
        {
            this.Id = id;
            this.Nombre = nombre;
            this.NIF = nIF;
            this.Direccion = direccion;
            this.CuentaBancaria = cuentaBancaria;
            this.Telefono = telefono;
            this.Email = email;
            this.RazonSocial = razonSocial;
            this.CuentaGasto = cuentaGasto;
            this.CuentaPago = cuentaPago;
            this.CuentaProveedor = cuentaProveedor;
        }

        public int Id { get; private set; }
        public string Nombre { get; private set; }
        public string NIF { get; private set; }
        public string Direccion { get; private set; }
        public string CuentaBancaria { get; private set; }
        public string Telefono { get; private set; }
        public string Email { get; private set; }
        public string RazonSocial { get; private set; }
        public string CuentaGasto { get; private set; }
        public string CuentaPago { get; private set; }
        public string CuentaProveedor { get; private set; }
    }
}
