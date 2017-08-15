using AdConta.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32.TaskScheduler;
using ModuloContabilidad.TaskScheduler;
using Tarea = Microsoft.Win32.TaskScheduler.Task;

namespace ModuloContabilidad.ObjModels
{
    public class AsientoProgramado : IAsiento, IOwnerComunidad, IObjWithDLO<AsientoProgramadoDLO>, 
    {
        public AsientoProgramado(Asiento asiento, TareaInfo infoTarea)
        {
            this._Id = asiento.Id;
            this._IdOwnerComunidad = asiento.IdOwnerComunidad;
            this.Apuntes = asiento.Apuntes;
            this.FechaValor = asiento.FechaValor;
            this.Saldo = asiento.Saldo;
            this.InfoTarea = infoTarea;

            if (this.InfoTarea.EstaEsMachineQueEjecuta)
            {
                var tsw = new TaskSchedulerWrapper(GlobalSettings.Properties.Settings.Default.NOMBREAPPINTERNO);
                this.Tarea = tsw.GetTarea(this.InfoTarea.NombreTarea);
            }
            else this.Tarea = null;
        }
        public AsientoProgramado(int idComunidad, TareaInfo infoTarea)
        {
            this._Id = 0;
            this.Apuntes = new ObservableApuntesList(this);
            this.FechaValor = DateTime.Today;
            this.Saldo = 0;
            this.InfoTarea = infoTarea;

            if (this.InfoTarea.EstaEsMachineQueEjecuta)
            {
                var tsw = new TaskSchedulerWrapper(GlobalSettings.Properties.Settings.Default.NOMBREAPPINTERNO);
                this.Tarea = tsw.GetTarea(this.InfoTarea.NombreTarea);
            }
            else this.Tarea = null;
        }
        //public AsientoProgramado(SchTask tarea, Asiento asiento)
        //{
        //    this._Id = asiento.Id;
        //    this._IdOwnerComunidad = asiento.IdOwnerComunidad;
        //    this.Apuntes = asiento.Apuntes;
        //    this.FechaValor = asiento.FechaValor;
        //    this.Saldo = asiento.Saldo;
        //    this.Tarea = tarea;
        //}
        //public AsientoProgramado(SchTask tarea, int idComunidad)
        //{
        //    this._Id = 0;
        //    this.Apuntes = new ObservableApuntesList(this);
        //    this.FechaValor = DateTime.Today;
        //    this.Saldo = 0;
        //    this.Tarea = tarea;
        //}

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

        public TareaInfo InfoTarea { get; private set; }
        public Tarea Tarea { get; private set; }
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
        public AsientoProgramadoDLO GetDLO()
        {
            return new AsientoProgramadoDLO(this.Id, this.IdOwnerComunidad, this.FechaValor, this.Saldo);
        }
        #endregion
    }

    public class AsientoProgramadoDLO : IDataListObject
    {

    }
}
