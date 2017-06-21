using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AdConta.Models;

namespace ModuloContabilidad.ObjModels
{
    public class GrupoCuentas : IObjModelBase, IOwnerComunidadNullable
    {
        #region constructors
        public GrupoCuentas(int id, int idComunidad, string accountNumber)
        {
            this.Id = id;
            this.IdOwnerComunidad = idComunidad;
            this._Grupo = new sGrupoContable(accountNumber);
            this._Subgrupo = new sSubgrupoContable(accountNumber);
        }
        public GrupoCuentas(int id, int idComunidad, int accountNumber)
        {
            this.Id = id;
            this.IdOwnerComunidad = idComunidad;
            this._Grupo = new sGrupoContable(accountNumber);
            this._Subgrupo = new sSubgrupoContable(accountNumber);
        }
        #endregion

        #region fields
        private sGrupoContable _Grupo;
        private sSubgrupoContable _Subgrupo;
        #endregion

        #region properties
        public int Id { get; private set; }
        public int? IdOwnerComunidad { get; private set; }

        public int Grupo { get { return this._Grupo.Digits; } }
        public int Subgrupo { get { return this._Subgrupo.Digits; } }
        #endregion

        #region public methods
        public bool Contains(ref CuentaMayor acc)
        {
            return (acc.Grupo == this.Grupo) && (acc.Subgrupo == this.Subgrupo);
        }
        public bool Contains(CuentaMayor acc)
        {
            return (acc.Grupo == this.Grupo) && (acc.Subgrupo == this.Subgrupo);
        }
        public bool Contains(ref string acc)
        {
            return (sGrupoContable.GetGrupoDigitsFromString(ref acc) == this.Grupo) && 
                (sSubgrupoContable.GetSubgrupoDigitsFromString(ref acc) == this.Subgrupo);
        }
        #endregion
    }
}
