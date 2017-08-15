using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ModuloContabilidad
{
    public class Command_CerrarTabExpTabAsiento : ICommand
    {
        private TabExpTabAsientoVM _tab;

        public Command_CerrarTabExpTabAsiento(TabExpTabAsientoVM tab)
        {
            this._tab = tab;
        }

        public bool CanExecute(object parameter)
        {
            if (this._tab.Modificando) return true;
            return false;
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public void Execute(object parameter)
        {
            this._tab.OnCerrarTabExpTab();
        }
    }
}
