using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AdConta;
using AdConta.Models;
using System.Collections.ObjectModel;

namespace ModuloContabilidad.ObjModels
{
    public class AsientoPredefinido : IAsiento, IOwnerComunidad, IObjWithDLO<AsientoPredefinidoDLO>
    {
        public AsientoPredefinido(Asiento asiento)
        {
            this._Id = asiento.Id;
            this._IdOwnerComunidad = asiento.IdOwnerComunidad;
            this.Apuntes = asiento.Apuntes;
            this.FechaValor = asiento.FechaValor;
            this.Saldo = asiento.Saldo;
        }
        public AsientoPredefinido(int idComunidad)
        {
            this._Id = 0;
            this.Apuntes = new ObservableApuntesList(this);
            this.FechaValor = DateTime.Today;
            this.Saldo = 0;
        }

        #region fields
        private int _Id;
        private int _IdOwnerComunidad;
        #endregion

        #region properties
        public int Id { get { return this._Id; } }
        public int IdOwnerComunidad { get { return this._IdOwnerComunidad; } }
        public Apunte this[int i] { get { return this.Apuntes[i]; } }
        public ObservableApuntesList Apuntes { get; private set; }
        public DateTime FechaValor { get; private set; }        
        public decimal Saldo { get; private set; }
        public bool Abierto { get { return true; } }
        #endregion

        #region helpers
        /// <summary>
        /// Set this.Balance property as the accounting balance of the apuntes currently stored in this._Apuntes.
        /// </summary>
        public void CalculaSaldo()
        {
            Saldo = Apuntes.SumaDebe - Apuntes.SumaHaber;
        }
        #endregion

        #region public methods
        public void SetApuntesList(IEnumerable<Apunte> apuntes)
        {
            this.Apuntes = new ObservableApuntesList(this, apuntes);
            CalculaSaldo();
        }
        #endregion

        #region DLO
        public AsientoPredefinidoDLO GetDLO()
        {
            return new AsientoPredefinidoDLO(this.Id, this.IdOwnerComunidad, this.FechaValor, this.Saldo);
        }
        #endregion
    }

    public class AsientoPredefinidoDLO : IDataListObject
    {
        public AsientoPredefinidoDLO() { }
        public AsientoPredefinidoDLO(int id, int idComunidad, DateTime fechaValor, decimal saldo)
        {
            this.Id = id;
            this.IdOwnerComunidad = idComunidad;
            this.FechaValor = fechaValor;
            this.Saldo = saldo;
        }

        public int Id { get; private set; }
        public int IdOwnerComunidad { get; private set; }
        public DateTime FechaValor { get; private set; }
        public decimal Saldo { get; private set; }
    }
}
