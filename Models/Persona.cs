using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper.Contrib.Extensions;

namespace AdConta.Models
{
    public class Persona : IObjModelBase
    {
        public Persona(int id, string nif, string nombre, bool forceInvalidNIF = false)
        {
            this._Id = id;
            this._NIF = new NIFModel(nif);
            this.Nombre = nombre;

            if (!this._NIF.IsValid && forceInvalidNIF)
                this._NIF.ForceInvalidNIF(ref nif);
        }

        #region fields
        private int _Id;
        private NIFModel _NIF;

        private CuentaBancaria _CuentaBancaria;
        #endregion

        #region properties
        public int Id { get { return this._Id; } }
        public NIFModel NIF { get { return this._NIF; } }
        public string Nombre { get; protected set; }

        public bool EsPropietario { get; set; }
        public bool EsProveedor { get; set; }
        public bool EsPagador { get; set; }
        public bool EsCopropietario { get; set; }

        public DireccionPostal Direccion { get; set; }

        public CuentaBancaria CuentaBancaria
        {
            get { return this._CuentaBancaria; }
            set { this._CuentaBancaria = value; }
        }

        public sTelefono Telefono1 { get; set; }
        public sTelefono Telefono2 { get; set; }
        public sTelefono Telefono3 { get; set; }
        public string Fax { get; set; }
        public string Email { get; set; }
        public string Notas { get; set; }
        #endregion

        #region copy
        public void CopyProtectedOrWorseToThis(ref Persona objToCopy)
        {
            this._Id = objToCopy.Id;
            this._NIF = objToCopy.NIF;
            this.Nombre = objToCopy.Nombre;
        }
        public void CopyId(ref Persona objToCopy)
        {
            this._Id = objToCopy.Id;
        }
        #endregion
    }

}
