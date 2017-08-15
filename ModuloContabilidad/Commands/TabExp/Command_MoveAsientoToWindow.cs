using AdConta.ViewModel;
using System;
using System.Windows;
using System.Windows.Input;

namespace ModuloContabilidad
{
    /// <summary>
    /// Move asientoSimple to a window.
    /// </summary>
    public class Command_MoveAsientoToWindow : ICommand
    {
        private TabExpTabAsientoVM _tab;

        public Command_MoveAsientoToWindow(TabExpTabAsientoVM tab)
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
