using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using System.Data;
using System.Windows.Input;
using System.Windows;
using System.Collections.ObjectModel;
using AdConta.ViewModel;
using AdConta.Models;
using ModuloContabilidad.Models;
using ModuloContabilidad.ObjModels;
using AdConta;

namespace ModuloContabilidad
{
    public class VMAsientoSimple : VMAsientoBase
    {
        public VMAsientoSimple()
        {
            base.IsWindowed = GlobalSettings.Properties.Settings.Default.ASIENTOSIMPLE_WINDOWED;
            base.Fecha = DateTime.Today;
            base.TabExpType = TabExpTabType.Simple;
            //this._model = new AsientoSimpleModel(base.TabComCod, true);
            this._MoveAsientoToWindow = new Command_MoveAsientoToWindow(this);
        }

        #region fields
        //private AsientoSimpleModel _model;
        #endregion

        #region properties
        public Visibility PinButtonVisibility { get; set; }
        //public ObservableCollection<Apunte> VMApuntes { get { return this._model.Asiento.Apuntes; } }
        #endregion

        #region commands
        private Command_MoveAsientoToWindow _MoveAsientoToWindow;
        #endregion

        #region commands props
        public ICommand MoveAsientoToWindow { get { return this._MoveAsientoToWindow; } }
        #endregion
    }

    /// <summary>
    /// Move asientoSimple to a window.
    /// </summary>
    public class Command_MoveAsientoToWindow : ICommand
    {
        private VMAsientoSimple _tab;

        public Command_MoveAsientoToWindow(VMAsientoSimple tab)
        {
            this._tab = tab;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public void Execute(object parameter)
        {
            if (!this._tab.IsWindowed)
            {
                this._tab.PinButtonVisibility = Visibility.Collapsed;
                this._tab.IsWindowed = true;

                (this._tab.ParentVM as aTabsWithTabExpVM).BottomTabbedExpanderItemsSource.Remove(this._tab);

                AsientosWindow w = new AsientosWindow();
                w.Name = "testWindow";
                w.AddExpanderUserControl(this._tab);
                w.Show();
                w.Focus();
            }
        }
    }
}
