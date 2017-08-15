using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdConta.Models
{
    public class Comunidad : IObjModelBase, IObjModelConCodigo, IObjWithDLO<ComunidadDLO>, IBaja
    {
        public Comunidad(int id, string cIF, bool baja, string nombre, int codigo, AutoCodigoData aCData, bool forceCIF)
        {
            this.Id = id;
            this._CIF = new NIFModel(cIF);
            if (this._CIF.NIF == null && forceCIF) this._CIF.ForceInvalidNIF(ref cIF);

            this.Baja = baja;
            this.Nombre = nombre;
            this.Codigo = new AutoCodigoNoOwner<Comunidad>(aCData, codigo);
        }
        //Comunidad(x.Id, x.CIF, x.Baja, x.Nombre, x.Codigo, this.ACData, true))
        #region fields
        private NIFModel _CIF;
        private CuentaBancaria _CuentaBancaria1;
        private CuentaBancaria _CuentaBancaria2;
        private CuentaBancaria _CuentaBancaria3;
        #endregion

        #region properties
        public int Id { get; private set; }

        public aAutoCodigoBase Codigo { get; private set; }
        public NIFModel CIF { get { return this._CIF; } }
        public bool Baja { get; private set; }
        public string Nombre { get; set; }
        public DireccionPostal Direccion { get; set; }

        public CuentaBancaria CuentaBancaria1 { get { return this._CuentaBancaria1; } }
        public CuentaBancaria CuentaBancaria2 { get { return this._CuentaBancaria2; } }
        public CuentaBancaria CuentaBancaria3 { get { return this._CuentaBancaria3; } }

        public Persona Presidente { get; set; }
        public Persona Secretario { get; set; }
        public Persona Tesorero { get; set; }
        public HashSet<int> Vocales { get; set; }

        public Ejercicio EjercicioActivo { get; set; }
        public Date FechaPunteo { get; set; }
        public Date UltimaFechaBanco { get; set; }
        #endregion

        #region helpers
        #endregion

        #region public methods
        public ComunidadDLO GetDLO()
        {
            return new ComunidadDLO(this.Id, this.Codigo.CurrentCodigo, this.CIF.NIF, this.Baja, this.Nombre, this.Direccion.GetDireccionSinCP(),
                this.CuentaBancaria1.AccountNumber, this.CuentaBancaria2.AccountNumber, this.CuentaBancaria3.AccountNumber, this.Presidente.Nombre,
                this.Secretario.Nombre, this.Tesorero.Nombre, this.FechaPunteo, this.UltimaFechaBanco);
        }

        public bool DarDeBaja(bool todoCerrado)
        {
            throw new NotImplementedException();
        }

        public bool RecuperarBaja(bool todoCerrado)
        {
            throw new NotImplementedException();
        }
        #endregion
    }

    public class ComunidadDLO : IDataListObject
    {
        public ComunidadDLO() { }
        public ComunidadDLO(int id, int codigo, string cIF, bool baja, string nombre, string direccion, string cuentaBancaria, string cuentaBancaria2,
            string cuentaBancaria3, string nombrePresidente, string nombreSecretario, string nombreTesorero, DateTime fechaPunteo, DateTime ultimaFechaBanco)
        {
            this.Id = id;
            this.Codigo = codigo;
            this.CIF = cIF;
            this.Baja = baja;
            this.Nombre = nombre;
            this.Direccion = direccion;
            this.CuentaBancaria1 = cuentaBancaria;
            this.CuentaBancaria2 = cuentaBancaria2;
            this.CuentaBancaria3 = cuentaBancaria3;
            this.NombrePresidente = nombrePresidente;
            this.NombreSecretario = nombreSecretario;
            this.NombreTesorero = nombreTesorero;
            this.FechaPunteo = fechaPunteo;
            this.UltimaFechaBanco = ultimaFechaBanco;
        }

        public int Id { get; private set; }
        public int Codigo { get; private set; }
        public string CIF { get; private set; }
        public bool Baja { get; private set; }
        public string Nombre { get; private set; }
        public string Direccion { get; private set; }
        public string CuentaBancaria1 { get; private set; }
        public string CuentaBancaria2 { get; private set; }
        public string CuentaBancaria3 { get; private set; }
        public string NombrePresidente { get; private set; }
        public string NombreSecretario { get; private set; }
        public string NombreTesorero { get; private set; }
        public DateTime FechaPunteo { get; private set; }
        public DateTime UltimaFechaBanco { get; private set; }
    }

}
