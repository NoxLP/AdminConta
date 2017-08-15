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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Collections.Specialized;
using AdConta.ViewModel;
using System.Windows.Controls.Primitives;
using Extensions;

namespace ModuloContabilidad
{
    /// <summary>
    /// Interaction logic for TabMayorUC.xaml
    /// </summary>
    public partial class TabMayorUC : UserControl
    {
        public TabMayorUC()
        {
            InitializeComponent();

            this._TabDGridPrincipalInitColumnsCount = this.TabDGridPrincipal.Columns.Count - 1;
        }

        #region fields
        private readonly int _TabDGridPrincipalInitColumnsCount;
        #endregion

        #region helpers
        private void DefineDGridColumns(ref DataGridCheckBoxColumn principalCol, ref DataGridTextColumn statusCol)
        {
            principalCol.Header = "Punteo";
            principalCol.Width = this.TabDGridPrincipal.Columns.First(x => (string)x.Header == "Debe").ActualWidth;//new DataGridLength(widthPercent, DataGridLengthUnitType.Star);
            principalCol.MinWidth = 35.0;
            principalCol.Binding = new Binding("Punteo") { Source = (this.DataContext as VMTabMayor).Apuntes };
            principalCol.HeaderStyle = Application.Current.Resources["DGridHeaderStyle"] as Style;
            principalCol.ElementStyle = Application.Current.Resources["DGridCenterCheckBCellStyle"] as Style;

            statusCol.Header = "Saldo Punteado";
            statusCol.Width = this.TabDGridStatus.Columns.First(x => (string)x.Header == "Debe").ActualWidth;//new DataGridLength(widthPercent, DataGridLengthUnitType.Star);
            statusCol.MinWidth = 35.0;
            statusCol.Binding = new Binding("StatusGridSaldoPunteado") { Source = this.DataContext as VMTabMayor, StringFormat = "C" };
            statusCol.HeaderStyle = Application.Current.Resources["DGridHeaderStyle"] as Style;
            statusCol.ElementStyle = Application.Current.Resources["StatusDGridRightCellStyle"] as Style;
        }

        public void ModifyPunteoColumn()
        {
            //TODO HAY QUE GUARDAR EL PUNTEO EN LA BASE DE DATOS
            if (this.TabDGridPrincipal.Columns.Count == this._TabDGridPrincipalInitColumnsCount + 1)
            {
                DataGridCheckBoxColumn col = new DataGridCheckBoxColumn();
                DataGridTextColumn statusCol = new DataGridTextColumn();
                DefineDGridColumns(ref col, ref statusCol);

                /*int index = this.TabDGridPrincipal.Columns.IndexOf(
                    this.TabDGridPrincipal.Columns.First(x => (string)x.Header == "Recibo"));
                this.TabDGridPrincipal.Columns[index].Visibility = Visibility.Collapsed;
                this.TabDGridPrincipal.Columns[index + 1].Visibility = Visibility.Collapsed;*/

                this.TabDGridPrincipal.Columns[this._TabDGridPrincipalInitColumnsCount - 1].Visibility = Visibility.Collapsed;
                this.TabDGridPrincipal.Columns[this._TabDGridPrincipalInitColumnsCount].Visibility = Visibility.Collapsed;
                this.TabDGridStatus.Columns[this._TabDGridPrincipalInitColumnsCount - 1].Visibility = Visibility.Collapsed;
                this.TabDGridStatus.Columns[this._TabDGridPrincipalInitColumnsCount].Visibility = Visibility.Collapsed;

                this.TabDGridPrincipal.Columns.Insert(this._TabDGridPrincipalInitColumnsCount - 1, col);
                this.TabDGridStatus.Columns.Insert(this._TabDGridPrincipalInitColumnsCount - 1, statusCol);
            }
            else
            {
                int index = this.TabDGridPrincipal.Columns.IndexOf(
                    this.TabDGridPrincipal.Columns.First(x => (string)x.Header == "Recibo"));
                this.TabDGridPrincipal.Columns[index].Visibility = Visibility.Visible;
                this.TabDGridPrincipal.Columns[index + 1].Visibility = Visibility.Visible;
                this.TabDGridStatus.Columns[index].Visibility = Visibility.Visible;
                this.TabDGridStatus.Columns[index + 1].Visibility = Visibility.Visible;

                DataGridCheckBoxColumn col = this.TabDGridPrincipal.Columns.First(x => (string)x.Header == "Punteo") as DataGridCheckBoxColumn;
                DataGridTextColumn statusCol = this.TabDGridStatus.Columns.First(x => (string)x.Header == "Saldo Punteado") as DataGridTextColumn;

                if (col != null)
                {
                    this.TabDGridPrincipal.Columns.Remove(col);
                    this.TabDGridStatus.Columns.Remove(statusCol);
                }

                ((VMTabMayor)this.DataContext).PublicNotifyPropChanged("StatusGridSaldoPunteado");
            }
        }
        #endregion

        #region user control events
        /// <summary>
        /// UserControl OnLoaded event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RootTabMayor_Loaded(object sender, RoutedEventArgs e)
        {
            /*VMTabMayor DC = (this.DataContext as VMTabMayor);
            if (DC.TabbedExpanderItemsSource.Count == 0)
                DC.TabbedExpanderItemsSource.Add(new VMTabbedExpDiario());*/
            //DC.ExpanderRowHeight = new GridLength(1, GridUnitType.Pixel);
        }
        /// <summary>
        /// Click event to add "punteado" column to this.TabDGridPrincipal
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TabButPunt_Click(object sender, RoutedEventArgs e)
        {

            //TODO: HAY QUE PASAR ESTO AL VIEWMODEL

            if (this.TabDGridPrincipal.Columns.Count == this._TabDGridPrincipalInitColumnsCount + 1)
            {
                DataGridCheckBoxColumn col = new DataGridCheckBoxColumn();
                DataGridTextColumn statusCol = new DataGridTextColumn();
                DefineDGridColumns(ref col, ref statusCol);

                /*int index = this.TabDGridPrincipal.Columns.IndexOf(
                    this.TabDGridPrincipal.Columns.First(x => (string)x.Header == "Recibo"));
                this.TabDGridPrincipal.Columns[index].Visibility = Visibility.Collapsed;
                this.TabDGridPrincipal.Columns[index + 1].Visibility = Visibility.Collapsed;*/

                this.TabDGridPrincipal.Columns[this._TabDGridPrincipalInitColumnsCount - 1].Visibility = Visibility.Collapsed;
                this.TabDGridPrincipal.Columns[this._TabDGridPrincipalInitColumnsCount].Visibility = Visibility.Collapsed;
                this.TabDGridStatus.Columns[this._TabDGridPrincipalInitColumnsCount - 1].Visibility = Visibility.Collapsed;
                this.TabDGridStatus.Columns[this._TabDGridPrincipalInitColumnsCount].Visibility = Visibility.Collapsed;

                this.TabDGridPrincipal.Columns.Insert(this._TabDGridPrincipalInitColumnsCount - 1, col);
                this.TabDGridStatus.Columns.Insert(this._TabDGridPrincipalInitColumnsCount - 1, statusCol);
            }
            else
            {
                int index = this.TabDGridPrincipal.Columns.IndexOf(
                    this.TabDGridPrincipal.Columns.First(x => (string)x.Header == "Recibo"));
                this.TabDGridPrincipal.Columns[index].Visibility = Visibility.Visible;
                this.TabDGridPrincipal.Columns[index + 1].Visibility = Visibility.Visible;
                this.TabDGridStatus.Columns[index].Visibility = Visibility.Visible;
                this.TabDGridStatus.Columns[index + 1].Visibility = Visibility.Visible;

                DataGridCheckBoxColumn col = this.TabDGridPrincipal.Columns.First(x => (string)x.Header == "Punteo") as DataGridCheckBoxColumn;
                DataGridTextColumn statusCol = this.TabDGridStatus.Columns.First(x => (string)x.Header == "Saldo Punteado") as DataGridTextColumn;

                if (col != null)
                {
                    this.TabDGridPrincipal.Columns.Remove(col);
                    this.TabDGridStatus.Columns.Remove(statusCol);
                }
            }
        }

        private void MayorDGridCell_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            ((VMTabMayor)this.DataContext).MayorDGridCell_MouseLeftButtonUp(sender, e);
            e.Handled = true;
        }
        #endregion
    }
}
