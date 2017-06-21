using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using AdConta.Models;

namespace ModuloContabilidad.ObjModels
{
    /// <summary>
    /// Class for ledge account
    /// </summary>
    public class CuentaMayor : IObjModelBase, IOwnerComunidad, IOwnerEjercicio, IObjWithDLO<CuentaMayorDLO>
    {
        public CuentaMayor(string accountNumber, int id, int idComunidad, int idEjercicio, string nombre, bool cuentaFalsa = false)
        {
            this._Id = id;
            this.Codigo = accountNumber;
            this._IdOwnerComunidad = idComunidad;
            this._IdOwnerEjercicio = idEjercicio;
            this.CuentaFalsa = cuentaFalsa;
            this.Nombre = nombre;

            this._Grupo = new sGrupoContable(accountNumber);
            this._Subgrupo = new sSubgrupoContable(accountNumber);
        }
        
        #region fields
        private int _Id;
        private int _IdOwnerComunidad;
        private int _IdOwnerEjercicio;
        private int _NumCuenta;
        private string _Codigo;
        private sGrupoContable _Grupo;
        private sSubgrupoContable _Subgrupo;
        private int _Sufijo;
        #endregion

        #region properties
        public int Id { get { return this._Id; } }
        public int IdOwnerComunidad { get { return this._IdOwnerComunidad; } }
        public int IdOwnerEjercicio { get { return this._IdOwnerEjercicio; } }
        public int NumCuenta
        {
            get { return _NumCuenta; }
            set
            {
                if (value == this._NumCuenta ||
                    value < GlobalSettings.Properties.Settings.Default.MINCODCUENTAS ||
                    value > GlobalSettings.Properties.Settings.Default.MAXCODCUENTAS)
                    throw new AdConta.CustomException_ObjModels("Código de cuenta contable erróneo al intentar dar valor a Id en el objecto CuentaMayor");

                this._NumCuenta = value;
                this._Codigo = value.ToString();
                this._Grupo = new sGrupoContable();
                this._Subgrupo = new sSubgrupoContable();
                this._Grupo.SetGrupoByAccNumber(value);
                this._Subgrupo.SetSubgrupoByAccNumber(value);
                //Get total default digits
                int digits = GlobalSettings.Properties.Settings.Default.DIGITOSCUENTAS - 1;
                //Get the rest of the digits as a whole number
                this._Sufijo = value - (int)Math.Truncate((this.Grupo + this.Subgrupo) * Math.Pow(10, digits - 2));
                /*
                //Get first digit
                this._Grupo = (int)Math.Truncate(value / Math.Pow(10, digits)) * 100;
                //Get second and third digit
                this._SubGrupo = (int)Math.Truncate(value / Math.Pow(10, digits - 2)) % 100;
                */
            }
        }
        public string Codigo
        {
            get { return _Codigo; }
            set
            {
                if (value == this._Codigo) return;

                int account;

                if (value.Length > GlobalSettings.Properties.Settings.Default.DIGITOSCUENTAS ||
                    !int.TryParse(value, out account) ||
                    value.Substring(0, 1) == "0")
                {
                    throw new AdConta.CustomException_ObjModels("Código de cuenta contable erróneo al intentar dar valor a Codigo en el objecto CuentaMayor");
                    //MessageBox.Show("Número de cuenta contable incorrecto");
                    //return;
                }

                this._Codigo = value;
                this._Grupo = new sGrupoContable();
                this._Subgrupo = new sSubgrupoContable();
                this._Grupo.SetGrupoByAccNumber(value);
                this._Subgrupo.SetSubgrupoByAccNumber(value);
                /*
                this._Grupo = int.Parse(value.Substring(0, 1)) * 100;
                this._SubGrupo = int.Parse(value.Substring(1, 2));*/
                this._Sufijo = int.Parse(value.Substring(3));

                int sufDigits = GlobalSettings.Properties.Settings.Default.DIGITOSCUENTAS - 3;
                sufDigits = (int)Math.Truncate(Math.Pow(10, sufDigits));
                this._NumCuenta = (this.Grupo + this.Subgrupo) * sufDigits + this.Sufijo;                
            }
        }
        public string Nombre { get; set; }
        public int Grupo { get { return this._Grupo.Digits; } }
        public int Subgrupo { get { return this._Subgrupo.Digits; } }
        public int Sufijo { get { return this._Sufijo; } }
        public bool CuentaFalsa { get; set; }
        #endregion

        #region public methods
        public bool IsLastAccount()
        {
            return this.Id == GlobalSettings.Properties.Settings.Default.MAXCODCUENTAS;
        }
        public bool IsFirstAccount()
        {
            return this.Id == GlobalSettings.Properties.Settings.Default.MINCODCUENTAS;
        }
        public bool IsProveedor_Propietario(List<GrupoCuentas> cuentasProveedores_Cobros)
        {
            foreach (GrupoCuentas gc in cuentasProveedores_Cobros)
                if (gc.Contains(this)) return true;

            return false;
        }
        public static CuentaMayor GetCuentaDefault()
        {
            return new CuentaMayor(
                GlobalSettings.Properties.Settings.Default.CUENTADEFAULT, 0, 0, 0, GlobalSettings.Properties.Settings.Default.NOMBRECUENTADEFAULT, true);
        }
        #endregion

        #region DLO
        public CuentaMayorDLO GetDLO()
        {
            return new CuentaMayorDLO(NumCuenta, Id, IdOwnerComunidad, IdOwnerEjercicio, Nombre, Grupo, Subgrupo, Sufijo, CuentaFalsa);
        }
        #endregion
    }

    public class CuentaMayorDLO : IObjModelBase, IDataListObject
    {
        public CuentaMayorDLO() { }
        public CuentaMayorDLO(
            int accountNumber,
            int id,
            int idCdad,
            int idEjer,
            string nombre,
            int grupo,
            int subgrupo,
            int sufijo,
            bool cuentaFalsa)
        {
            if (cuentaFalsa) throw new AdConta.CustomException_DataListObjects(
                 "Error creando DLO de CuentaMayor. No se puede crear objeto DLO con una cuenta falsa.");

            this.NumCuenta = accountNumber;
            this.Id = id;
            this.IdOwnerComunidad = idCdad;
            this.IdOwnerEjercicio = idEjer;
            this.Nombre = nombre;
            this.Grupo = grupo;
            this.Subgrupo = subgrupo;
            this.Sufijo = sufijo;
        }
        public CuentaMayorDLO(
            int accountNumber,
            int id,
            int idCdad,
            int idEjer,
            string nombre)
        {
            this.NumCuenta = accountNumber;
            this.Id = id;
            this.IdOwnerComunidad = idCdad;
            this.IdOwnerEjercicio = idEjer;
            this.Nombre = nombre;

            var grupo = new sGrupoContable();
            var sgrupo = new sSubgrupoContable();
            grupo.SetGrupoByAccNumber(accountNumber);
            sgrupo.SetSubgrupoByAccNumber(accountNumber);
            this.Grupo = grupo.Digits;
            this.Subgrupo = sgrupo.Digits;
            //Get total default digits
            int digits = GlobalSettings.Properties.Settings.Default.DIGITOSCUENTAS - 1;
            //Get the rest of the digits as a whole number
            this.Sufijo = accountNumber - (int)Math.Truncate((this.Grupo + this.Subgrupo) * Math.Pow(10, digits - 2));
        }

        public int Id { get; private set; }
        public int IdOwnerComunidad { get; private set; }
        public int IdOwnerEjercicio { get; private set; }
        public int NumCuenta { get; private set; }
        public string Nombre { get; private set; }
        public int Grupo { get; private set; }
        public int Subgrupo { get; private set; }
        public int Sufijo { get; private set; }
    }
}
