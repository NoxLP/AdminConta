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
using System.ComponentModel;
using AdConta.ViewModel;
using Extensions;

namespace AdConta
{
    /// <summary>
    /// Interaction logic for TabCdadUC.xaml
    /// </summary>
    public partial class TabCdadUC : UserControl, INotifyPropertyChanged
    {
        public TabCdadUC()
        {
            InitializeComponent();
        }

        private void Tog_MouseEnter(object sender, MouseEventArgs e)
        {
            //this.TabCdadTabControl.ItemsPanel.
            /*int i = 0;
            object o = e.OriginalSource;
            Grid grid = this.TabCdadTabControl.FindVisualChild<Grid>(x => (x as Grid).Name == "TabCGridCol");
            grid.ColumnDefinitions[0].Width = GridLength.Auto;*/
            (this.DataContext as VMTabCdad).TabPanelWidth = double.NaN;
            e.Handled = true;
        }

        private void Tog_MouseLeave(object sender, MouseEventArgs e)
        {
            /*Grid grid = this.TabCdadTabControl.FindVisualChild<Grid>(x => (x as Grid).Name == "TabCGridCol");
            grid.ColumnDefinitions[0].Width = new GridLength(5);*/
            (this.DataContext as VMTabCdad).TabPanelWidth = 10;
            e.Handled = true;
        }

        private void ToggleButton_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.Primitives.ToggleButton tog = sender as System.Windows.Controls.Primitives.ToggleButton;
            tog.IsChecked = true;
            NotifyPropChanged("IsChecked");

            this.TabCdadTabControl.SelectedIndex = this.TabCdadTabControl.Items.IndexOf(tog.FindFirstParentOfType<TabItem>());
        }

        #region PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        protected void NotifyPropChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion
    }
}
