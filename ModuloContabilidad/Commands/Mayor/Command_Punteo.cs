using AdConta.ViewModel;
using Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ModuloContabilidad
{
    public class Command_Punteo : ICommand
    {
        private aVMTabBase _tab;

        public Command_Punteo(aVMTabBase tab)
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
            TabMayorUC UC = Application.Current.MainWindow.FindFirstVisualChildOfType<TabMayorUC>();
            UC.ModifyPunteoColumn();
        }
    }
}
