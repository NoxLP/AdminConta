using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AdConta.ViewModel;
using AdConta;
using TabbedExpanderCustomControl;

namespace ModuloContabilidad
{
    public class VMTabbedExpDiario : TabExpTabItemBaseVM, IPublicNotify
    {
        #region fields
        private bool _IsSelected;
#pragma warning disable CS0414
        private bool _Expandible = true;
#pragma warning restore CS0414
        #endregion

        #region properties
        public override TabExpTabType TabExpType { get { return TabExpTabType.Diario; } }
        public override string Header { get { return "Vista diario"; } }
        public double DGridHeight { get; }

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
        #endregion

        #region PropertyChanged
        public void PublicNotifyPropChanged(string propName)
        {
            base.NotifyPropChanged(propName);
        }
        #endregion
    }
}
