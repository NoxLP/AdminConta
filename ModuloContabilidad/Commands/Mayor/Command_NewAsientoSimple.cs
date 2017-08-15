using AdConta;
using AdConta.ViewModel;
using ModuloContabilidad.TabbedExpanderTabs;
using System;
using System.Windows.Input;

namespace ModuloContabilidad
{
    /// <summary>
    /// Create new AsientoSimple as a new tab in tabbedExpander or windowed, following app setting "ASIENTOSIMPLE_WINDOWED"
    /// </summary>
    public class Command_NewAsientoSimple : ICommand
    {
        private aVMTabBase _tab;

        public Command_NewAsientoSimple(aVMTabBase tab)
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
            //If !is windowed by default, add the usercontrol to the expander of the tab
            if (!GlobalSettings.Properties.Settings.Default.ASIENTOSIMPLE_WINDOWED)
            {
                VMTabMayor tab = this._tab as VMTabMayor;
                TabExpTabAsientoVM VM = new TabExpTabAsientoVM();
                VM.TabComCod = tab.TabCodigoComunidad;
                VM.ParentVM = tab;
                VM.TabExpType = TabExpTabType.Inferior_AsientoSimple;


                tab.AddAndSelectTabInTabbedExpander(VM, TabExpWhich.Bottom);
                //tab.TabbedExpanderSelectedIndex = tab.TabbedExpanderItemsSource.IndexOf(VM);
                //tab.PublicNotifyPropChanged("TabbedExpanderItemsSource");
                /*MainWindow w = App.Current.MainWindow as MainWindow;
                TabMayorUC mayorUC = w.AbleTabControl.RootTabControl.FindVisualChild<TabMayorUC>(x =>
                {
                    if (x is TabMayorUC)
                        return (x as TabMayorUC).DataContext == tab;
                    else return false;
                });
                mayorUC.TabExpItemsSource.Add(VM);
                mayorUC.TabExpSelectedIndex = mayorUC.TabExpItemsSource.IndexOf(VM);*/
            }
            //else create, show and focus a new window with the usercontrol as content
            else
            {
                TabExpInferiorTabAsientoUC ASUC = new TabExpInferiorTabAsientoUC();
                TabExpTabAsientoVM VM = new TabExpTabAsientoVM();
                VM.TabComCod = this._tab.TabCodigoComunidad;
                VM.ParentVM = this._tab as VMTabMayor;
                VM.TabExpType = TabExpTabType.Inferior_AsientoSimple;
                ASUC.DataContext = VM;
                AsientosWindow w = new AsientosWindow();

                w.RootAsGrid.Children.Add(ASUC);
                w.Show();
                w.Focus();
            }
        }
    }
}
