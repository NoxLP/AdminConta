using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Data;
using System.Reflection;
using System.Collections.ObjectModel;
using AdConta.Models;

namespace AdConta.ViewModel
{
    public enum Mayor_SearchType : int { Fecha = 0, Concepto, Importe, ImporteDebe, ImporteHaber, Recibo, Factura }

    public class VMTabMayor : TabsWithTabbedExpVM
    {
        public VMTabMayor()
        {
            base.Type = TabType.Mayor;
            base.InitializeComcod((Application.Current.MainWindow.DataContext as VMMain).LastComCod);
            this._model = new TabMayorModel(base.TabComCod);
            this._StatusGridSource = new DataTable();
            this.PopulateStatusGrid();
            this._TabbedExpanderItemsSource = new ObservableCollection<iTabbedExpanderItemVM>();

            this._NextRecord = new Command_NextRecord_Mayor(this);
            this._PrevRecord = new Command_PrevRecord_Mayor(this);
            this._NewAS = new Command_NewAsientoSimple(this);
        }

        #region fields
        private TabMayorModel _model;
        private Mayor_SearchType _SearchType;

        private DataTable _StatusGridSource;
        private ObservableCollection<iTabbedExpanderItemVM> _TabbedExpanderItemsSource;
        private int _TabbedExpanderSelectedIndex;


        #endregion

        #region properties
        public string Nombre { get; set; }
        public string AccountCod
        {
            get { return this._model.CurrentAccount.Codigo; }
        }
        public DataView DView { get { return this._model.DView; } }
        public DataView StatusGridSource { get { return this._StatusGridSource.DefaultView; } }
        //TODO cuando se cambie la cuenta Y el punteo esté activado, hay que hacer propchanged aquí en StatusGridSaldoPunteado
        public decimal StatusGridSaldoPunteado { get { return this.GetSaldoPunteado(); } }
        public override ObservableCollection<iTabbedExpanderItemVM> TabbedExpanderItemsSource
        {
            get { return this._TabbedExpanderItemsSource; }
            set
            {
                if (this._TabbedExpanderItemsSource != value)
                {
                    this._TabbedExpanderItemsSource = value;
                    this.PublicNotifyPropChanged("TabbedExpanderItemsSource");
                }
            }
        }
        public override int TabbedExpanderSelectedIndex
        {
            get { return this._TabbedExpanderSelectedIndex; }
            set
            {
                if (this._TabbedExpanderSelectedIndex != value)
                {
                    this._TabbedExpanderSelectedIndex = value;
                    this.PublicNotifyPropChanged("TabbedExpanderSelectedIndex");
                }
            }
        }

        #endregion

        #region commands
        private Command_NextRecord_Mayor _NextRecord;
        private Command_PrevRecord_Mayor _PrevRecord;
        private Command_NewAsientoSimple _NewAS;
        #endregion

        #region commands props
        public ICommand NextRecord { get { return this._NextRecord; } }
        public ICommand PrevRecord { get { return this._PrevRecord; } }
        public ICommand NewAs { get { return this._NewAS; } }
        #endregion

        #region helpers
        private void PopulateStatusGrid()
        {
            this._StatusGridSource.Clear();
            //this._StatusGridSource = this._model.DTable.Clone();
            this._StatusGridSource.Columns.Clear();
            this._StatusGridSource.Columns.Add("NAsiento", typeof(int));
            this._StatusGridSource.Columns.Add("Fecha", typeof(DateTime));
            this._StatusGridSource.Columns.Add("Concepto", typeof(string));
            this._StatusGridSource.Columns.Add("Debe", typeof(decimal));
            this._StatusGridSource.Columns.Add("Haber", typeof(decimal));
            this._StatusGridSource.Columns.Add("Saldo", typeof(decimal));
            this._StatusGridSource.Columns.Add("Recibo", typeof(string));
            this._StatusGridSource.Columns.Add("Factura", typeof(string));

            int MaxNAs;
            DateTime MaxDate;
            this.GetMaxFromColumn("NAsiento", out MaxNAs);
            this.GetMaxFromColumn("Fecha", out MaxDate);

            this._StatusGridSource.Rows.Clear();
            this._StatusGridSource.Rows.Add(new object[] {
                MaxNAs,
                MaxDate,
                "Sumas y Saldos",
                this.GetColumnSum("Debe"),
                this.GetColumnSum("Haber"),
                this.GetValueFromTable<int>("Saldo", this._model.DTable.Rows.Count - 1),
                "",
                ""
            });

            this.NotifyPropChanged("StatusGridSource");
        }
        private void GetMaxFromColumn(string column, out int maxResult)
        {
            if (this._model.CurrentAccount.IsFakeAccount || this._model.DTable.Rows.Count == 0)
            {
                maxResult = 0;
                return;
            }

            int max = (int)this._model.DTable.Rows[0][column];
            int temp;

            for (int i = 0; i < this._model.DTable.Rows.Count; i++)
            {
                temp = GetValueFromTable<int>(column, i);
                if (temp > max) max = temp;
            }

            maxResult = max;
        }
        private void GetMaxFromColumn(string column, out DateTime maxResult)
        {
            if (this._model.CurrentAccount.IsFakeAccount || this._model.DTable.Rows.Count == 0)
            {
                maxResult = new DateTime(DateTime.Today.Year, 1, 1);
                return;
            }

            DateTime max = (DateTime)this._model.DTable.Rows[0][column];
            DateTime temp;

            for (int i = 0; i < this._model.DTable.Rows.Count; i++)
            {
                temp = GetValueFromTable<DateTime>(column, i);
                if (temp > max) max = temp;
            }

            maxResult = max;
        }
        private decimal GetColumnSum(string column)
        {
            if (this._model.CurrentAccount.IsFakeAccount || this._model.DTable.Rows.Count == 0)
                return 0;

            int sum = 0;

            for (int i = 0; i < this._model.DTable.Rows.Count; i++)
                sum += GetValueFromTable<int>(column, i);

            return sum;
        }
        private decimal GetSaldoPunteado()
        {
            if (this._model.CurrentAccount.IsFakeAccount || this._model.DTable.Rows.Count == 0)
                return 0;

            int sum = 0;

            for (int i = 0; i < this._model.DTable.Rows.Count; i++)
            {
                if (GetValueFromTable<bool>("Punteo", i))
                {
                    sum += GetValueFromTable<int>("Debe", i);
                    sum -= GetValueFromTable<int>("Haber", i);
                }
            }

            return sum;
        }
        #endregion

        #region datatablehelpers overridden methods
        /// <summary>
        /// Gets value of type T from datatable column using ConvertFromDBVal. Doesn't check if value is of type T. Only one row supposed(f.i. tab cdades).
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="column"></param>
        /// <param name="row"></param>
        /// <returns></returns>
        public override T GetValueFromTable<T>(string column, int row)
        {
            return ConvertFromDBVal<T>(this._model.DTable.Rows[row][column]);
        }
        /// <summary>
        /// Set value of datatable column. Only one row supposed(f.i. tab cdades).
        /// </summary>
        /// <param name="column"></param>
        /// <param name="value"></param>
        public override void SetValueToTable(string column, object value)
        {
            this._model.DTable.Rows[0][column] = value;
        }
        #endregion

        #region common commands overridden methods
        /// <summary>
        /// Order model to get data of new account cod.
        /// </summary>
        /// <param name="newCod"></param>
        internal override void OnChangedCod(int newCod)
        {
            if (!this._model.ChangeAcc(newCod, base.TabComCod))
            {
                //Fake account

            }
            else
            {

            }
        }

        internal override bool IsFirstAccount()
        {
            return this._model.CurrentAccount.IsFirstAccount();
        }
        internal override bool IsLastAccount()
        {
            return this._model.CurrentAccount.IsLastAccount();
        }
        #endregion
    }

    /// <summary>
    /// Create new AsientoSimple as a new tab in tabbedExpander or windowed, following app setting "ASIENTOSIMPLE_WINDOWED"
    /// </summary>
    public class Command_NewAsientoSimple : ICommand
    {
        private VMTabBase _tab;

        public Command_NewAsientoSimple(VMTabBase tab)
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
            if (!Properties.Settings.Default.ASIENTOSIMPLE_WINDOWED)
            {
                VMTabMayor tab = this._tab as VMTabMayor;
                AsientosUC.VMAsientoSimple VM = new AsientosUC.VMAsientoSimple();
                VM.TabComCod = tab.TabComCod;
                VM.BaseTab = tab;
                VM.Type = ExpanderTabType.Simple;


                tab.TabbedExpanderItemsSource.Add(VM);
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
                tab.TabbedExpanderSelectedIndex = tab.TabbedExpanderItemsSource.IndexOf(VM);
            }
            //else create, show and focus a new window with the usercontrol as content
            else
            {
                AsientosUC.AsientoSimple ASUC = new AsientosUC.AsientoSimple();
                AsientosUC.VMAsientoSimple VM = new AsientosUC.VMAsientoSimple();
                VM.TabComCod = this._tab.TabComCod;
                VM.BaseTab = this._tab as VMTabMayor;
                VM.Type = ExpanderTabType.Simple;
                ASUC.DataContext = VM;
                AsientosUC.AsientosWindow w = new AsientosUC.AsientosWindow();

                w.RootAsGrid.Children.Add(ASUC);
                w.Show();
                w.Focus();
            }
        }
    }
}
