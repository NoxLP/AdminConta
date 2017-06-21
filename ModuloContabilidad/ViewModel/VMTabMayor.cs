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
using AdConta;
using AdConta.ViewModel;
using ModuloContabilidad.Models;
using TabbedExpanderCustomControl;
using Extensions;
using Repository;

namespace ModuloContabilidad
{
    //++++++++++++++++++++++++++++++++++ OJO +++++++++++++++++++++++++++++++++++++++++++++++++++++
    //TODO: CUANDO SE CAMBIA DE CUENTA ESPERAR 2-3 SEGUNDOS ANTES DE PEDIR A LA BD LA NUEVA CUENTA
    //POR SI SOLO SE ESTA PASANDO DE UNA CUENTA A OTRA LEJANA
    //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

    public enum Mayor_SearchType : int { Fecha = 0, Concepto, Importe, ImporteDebe, ImporteHaber, Recibo, Factura }

    public class VMTabMayor : aTabsWithTabExpVM
    {
        public VMTabMayor()
        {
            base.Type = TabType.Mayor;
            //base.InitializeComcod((Application.Current.MainWindow.DataContext as VMMain).LastComCod);
            try { base.InitializeComcod((int)Messenger.Messenger.SearchMsg("LastComCod")); }
            catch (Exception)
            {
                MessageBox.Show("No se pudo abrir la pestaña de libro mayor por falta del código de Comunidad");
                return;
            }

            Task.Run(()=> InitUoWAsync()).Forget().ConfigureAwait(false);

            this._model = new TabMayorModel(base.TabComCod);
            this._StatusGridSource = new DataTable();
            this.PopulateStatusGrid();
            
            this.TopTabbedExpanderItemsSource = new ObservableCollection<TabExpTabItemBaseVM>();
            this.BottomTabbedExpanderItemsSource = new ObservableCollection<TabExpTabItemBaseVM>();

            this._NextRecord = new Command_NextRecord_Mayor(this);
            this._PrevRecord = new Command_PrevRecord_Mayor(this);
            this._NewAS = new Command_NewAsientoSimple(this);
            this._Punteo = new Command_Punteo(this);
        }

        #region fields
        private TabMayorModel _model;
        private Mayor_SearchType _SearchType;
        private DataTable _StatusGridSource;

        #region tabbed expander
        private int _TopTabbedExpanderSelectedIndex;
        private int _BottomTabbedExpanderSelectedIndex;
        #endregion
        #endregion

        #region properties
        public CuentaMayorRepository CuentaRepo { get; private set; }
        public AsientoRepository AsientoRepo { get; private set; }
        public ApunteRepository ApunteRepo { get; private set; }
        public UnitOfWork UOW { get; private set; }
        public string Nombre { get; set; }
        public string AccountCod
        {
            get { return this._model.CurrentAccount.Codigo; }
        }
        public DataView DView { get { return this._model.DView; } }
        public DataView StatusGridSource { get { return this._StatusGridSource.DefaultView; } }
        //TODO cuando se cambie la cuenta Y el punteo esté activado, hay que hacer propchanged aquí en StatusGridSaldoPunteado
        public decimal StatusGridSaldoPunteado { get { return this.GetSaldoPunteado(); } }
        /*public ObservableCollection<iTabbedExpanderItemVM> TabbedExpanderItemsSource
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
        public int TabbedExpanderSelectedIndex
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
        }*/

        #region tabbed expander
        public override ObservableCollection<TabExpTabItemBaseVM> TopTabbedExpanderItemsSource { get; set; }
        public override ObservableCollection<TabExpTabItemBaseVM> BottomTabbedExpanderItemsSource { get; set; }
        public override int TopTabbedExpanderSelectedIndex
        {
            get { return this._TopTabbedExpanderSelectedIndex; }
            set
            {
                if (this._TopTabbedExpanderSelectedIndex != value)
                {
                    this._TopTabbedExpanderSelectedIndex = value;
                    this.NotifyPropChanged("TopTabbedExpanderSelectedIndex");
                }
            }
        }
        public override int BottomTabbedExpanderSelectedIndex
        {
            get { return this._BottomTabbedExpanderSelectedIndex; }
            set
            {
                if (this._BottomTabbedExpanderSelectedIndex != value)
                {
                    this._BottomTabbedExpanderSelectedIndex = value;
                    this.NotifyPropChanged("BottomTabbedExpanderSelectedIndex");
                }
            }
        }
        #endregion
        #endregion

        #region commands
        private Command_NextRecord_Mayor _NextRecord;
        private Command_PrevRecord_Mayor _PrevRecord;
        private Command_NewAsientoSimple _NewAS;
        private Command_Punteo _Punteo;
        #endregion

        #region commands props
        public ICommand NextRecord { get { return this._NextRecord; } }
        public ICommand PrevRecord { get { return this._PrevRecord; } }
        public ICommand NewAs { get { return this._NewAS; } }
        public ICommand Punteo { get { return this._Punteo; } }
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
            if (this._model.CurrentAccount.CuentaFalsa || this._model.DTable.Rows.Count == 0)
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
            if (this._model.CurrentAccount.CuentaFalsa || this._model.DTable.Rows.Count == 0)
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
            if (this._model.CurrentAccount.CuentaFalsa || this._model.DTable.Rows.Count == 0)
                return 0;

            int sum = 0;

            for (int i = 0; i < this._model.DTable.Rows.Count; i++)
                sum += GetValueFromTable<int>(column, i);

            return sum;
        }
        private decimal GetSaldoPunteado()
        {
            if (this._model.CurrentAccount.CuentaFalsa || this._model.DTable.Rows.Count == 0)
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
        public override void OnChangedCod(int newCod)
        {
            if (!this._model.ChangeAcc(newCod, base.TabComCod))
            {
                //Fake account

            }
            else
            {

            }
        }

        public override bool IsFirstAccount()
        {
            return this._model.CurrentAccount.IsFirstAccount();
        }
        public override bool IsLastAccount()
        {
            return this._model.CurrentAccount.IsLastAccount();
        }
        #endregion

        #region UoW
        /// <summary>
        /// Llamado por AbleTabControl cuando se cierra la pestaña
        /// </summary>
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public override async Task CleanUnitOfWork()
        {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            this.UOW.RemoveVMTabReferencesFromRepos().Forget().ConfigureAwait(false);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        }

        public override async Task InitUoWAsync()
        {
            iAppRepositories appRepos = (iAppRepositories)Application.Current;
            HashSet<IRepository> repos = new HashSet<IRepository>();

            repos.Add(appRepos.CuentaMayorRepo);
            repos.Add(appRepos.AsientoRepo);
            repos.Add(appRepos.ApunteRepo);
            this.UOW = new UnitOfWork(repos, this);

            this.CuentaRepo = appRepos.CuentaMayorRepo;
            this.AsientoRepo = appRepos.AsientoRepo;
            this.ApunteRepo = appRepos.ApunteRepo;
        }
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        #endregion
    }

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
                VMAsientoSimple VM = new VMAsientoSimple();
                VM.TabComCod = tab.TabComCod;
                VM.ParentVM = tab;
                VM.TabExpType = TabExpTabType.Simple;


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
                AsientoSimple ASUC = new AsientoSimple();
                VMAsientoSimple VM = new VMAsientoSimple();
                VM.TabComCod = this._tab.TabComCod;
                VM.ParentVM = this._tab as VMTabMayor;
                VM.TabExpType = TabExpTabType.Simple;
                ASUC.DataContext = VM;
                AsientosWindow w = new AsientosWindow();

                w.RootAsGrid.Children.Add(ASUC);
                w.Show();
                w.Focus();
            }
        }
    }

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
