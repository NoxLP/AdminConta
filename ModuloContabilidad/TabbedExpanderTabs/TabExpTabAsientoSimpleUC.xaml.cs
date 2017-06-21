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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Extensions;

namespace ModuloContabilidad
{
    /// <summary>
    /// Interaction logic for AsientoSimple.xaml
    /// </summary>
    public partial class AsientoSimple : UserControl
    {
        public AsientoSimple()
        {
            InitializeComponent();
        }

        private void TabExpCloseTabButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            //TabItem tab = button.FindFirstParentOfType<TabItem>();
            TabMayorUC mayorUC = button.FindFirstParentOfType<TabMayorUC>();
            VMAsientoSimple VM = this.DataContext as VMAsientoSimple;

            //if (VM.BaseTab.Type == TabType.Mayor)
            (VM.ParentVM as AdConta.ViewModel.aTabsWithTabExpVM).BottomTabbedExpanderItemsSource.Remove(VM);
            //TODO manage rest of tab types
        }
    }
}

/*
 *         public void AddUserControl(UserControl1 uc)
        {
            //this.AddLogicalChild(uc);
            this.grid.Children.Add(uc);
        }

        public void DeleteUserControl(UserControl1 uc)
        {
            //this.RemoveLogicalChild(uc);
            //this.RemoveVisualChild(uc);
            this.grid.Children.Remove(uc);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Class1 vm = (Class1)this.DataContext;

            if(!vm.IsWindowed)
            {
                vm.IsWindowed = true;
                UserControl1 uc = this;
                (App.Current.MainWindow as MainWindow).expander.Content = null;
                
                Window1 w = new Window1();
                w.Name = "testWindow";
                w.AddUserControl(uc);
                w.Show();
            }
            else
            {
                vm.IsWindowed = false;
                UserControl1 uc = this;
                Button b = sender as Button;
                Window1 w = FindVisualParent<Window1>(b);

                w.DeleteUserControl(uc);
                w.Close();

                (App.Current.MainWindow as MainWindow).expander.Content = uc;
            }
        }
*/

