using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using ModuloContabilidad;
using AdConta.ViewModel;

namespace ModuloContabilidad
{
    /// <summary>
    /// Interaction logic for AsientosWindow.xaml
    /// </summary>
    public partial class AsientosWindow : CustomChromeLibrary.CustomChromeWindow
    {
        public AsientosWindow()
        {
            InitializeComponent();
        }

        public void AddExpanderUserControl(VMAsientoSimple vm)
        {
            this.CPresenter.Content = vm;
            //DataGrid DGrid = this.CPresenter.FindVisualChild<DataGrid>(x => (x as DataGrid).Name == "DGridAsiento");

            //DGrid.SetBinding(DGrid.Height, new Binding("DGridHeight"));
        }

        public void MoveUserControlToAbleTabcontrol(object sender, EventArgs e)
        {
            //AsientoSimple ASUC = this.CPresenter.Content as AsientoSimple;//FindFirstVisualChildren<AsientoSimple>();
            VMAsientoSimple VM = this.CPresenter.Content as VMAsientoSimple;
            VM.PinButtonVisibility = Visibility.Visible;
            VM.IsWindowed = false;
            //ViewModel.TabsWithTabbedExpVM baseT = VM.BaseTab;// as ViewModel.VMTabBase;

            this.CPresenter.Content = null;
            /*if (baseT.Type == TabType.Mayor)
            {
                MainWindow w = App.Current.MainWindow as MainWindow;
                TabMayorUC mayorUC = w.AbleTabControl.RootTabControl.FindVisualChild<TabMayorUC>(x =>
                {
                    if (x is TabMayorUC)
                        return (x as TabMayorUC).DataContext == baseT;
                    else return false;
                });
                mayorUC.TabExpItemsSource.Add(VM);
                ASUC.PinButton.Visibility = Visibility.Visible;
                VM.IsWindowed = false;
                (baseT as VMTabMayor).TabbedExpanderItemsSource.Add(VM);
                int index = mayorUC.TabExpItemsSource.IndexOf(VM);
                mayorUC.TabExpSelectedIndex = mayorUC.TabExpItemsSource.IndexOf(VM);
                -----------------------------------------------------------------------------------
                VMTabMayor mayorVM = baseT as VMTabMayor;
                mayorVM.AddAndSelectTabInTabbedExpander(VM);
            }

            //TODO else to manage AbleTabControl.VMTabDiario*/

            (VM.ParentVM as aTabsWithTabExpVM).AddAndSelectTabInTabbedExpander(VM, AdConta.TabExpWhich.Bottom);
            this.Close();
        }
    }
}
