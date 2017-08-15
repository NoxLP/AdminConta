using AdConta.ViewModel;
using System;
using System.Windows;
using System.Windows.Input;

namespace ModuloContabilidad
{
    public class Command_GuardarAsiento : ICommand
    {
        private TabExpTabAsientoVM _tab;

        public Command_GuardarAsiento(TabExpTabAsientoVM tab)
        {
            this._tab = tab;
        }

        public bool CanExecute(object parameter)
        {
            if(this._tab.Modificando) return true;
            return false;
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public void Execute(object parameter)
        {
            this._tab.OnGuardar();
        }
    }

    public class Command_DeshacerCambios : ICommand
    {
        private TabExpTabAsientoVM _tab;

        public Command_DeshacerCambios(TabExpTabAsientoVM tab)
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
            this._tab.OnDeshacerCambios();
        }
    }
}
