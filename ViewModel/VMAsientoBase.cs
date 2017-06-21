using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using TabbedExpanderCustomControl;

namespace AdConta.ViewModel
{
    public class VMAsientoBase : TabExpTabItemBaseVM, IPublicNotify
    {
        #region fields
        private TabExpTabType _Type;
        private int _TabComCod = 0;
#pragma warning disable CS0414
        private bool _Expandible = true;
#pragma warning restore CS0414

        private bool _IsSelected;
        private bool _IsWindowed;

        private int _Asiento;
        private DateTime _Fecha;
        private string _CuentaBase;
        private int _SaldoDebe;
        private int _SaldoHaber;
        private int _Descuadre;
        #endregion

        #region properties
        public override TabExpTabType TabExpType
        {
            get { return this._Type; }
            set { this._Type = value; }
        }
        public int TabComCod
        {
            get { return this._TabComCod; }
            set { this._TabComCod = value; }
        }
        public virtual double DGridHeight { get; }

        public bool IsSelected
        {
            get { return _IsSelected; }
            set
            {
                if (this._IsSelected != value)
                {
                    _IsSelected = value;
                    this.NotifyPropChanged("IsSelected");
                }
            }
        }
        public bool IsWindowed
        {
            get { return this._IsWindowed; }
            set
            {
                if (this._IsWindowed != value)
                {
                    this._IsWindowed = value;
                    this.NotifyPropChanged("IsWindowed");
                }
            }
        }

        public int Asiento
        {
            get { return this._Asiento; }
            set
            {
                if (this._Asiento != value)
                {
                    this._Asiento = value;
                }
            }
        }
        public DateTime Fecha
        {
            get { return this._Fecha; }
            set
            {
                if (this._Fecha != value)
                {
                    this._Fecha = value;
                }
            }
        }
        public string CuentaBase
        {
            get { return this._CuentaBase; }
            set
            {
                if (this._CuentaBase != value)
                {
                    this._CuentaBase = value;
                }
            }
        }
        public int SaldoDebe
        {
            get { return this._SaldoDebe; }
            set
            {
                if (this._SaldoDebe != value)
                {
                    this._SaldoDebe = value;
                }
            }
        }
        public int SaldoHaber
        {
            get { return this._SaldoHaber; }
            set
            {
                if (this._SaldoHaber != value)
                {
                    this._SaldoHaber = value;
                }
            }
        }
        public int Descuadre
        {
            get { return this._Descuadre; }
            set
            {
                if (this._Descuadre != value)
                {
                    this._Descuadre = value;
                }
            }
        }
        #endregion

        #region PropertyChanged
        public void PublicNotifyPropChanged(string propName)
        {
            base.NotifyPropChanged(propName);
        }
        #endregion
    }
}