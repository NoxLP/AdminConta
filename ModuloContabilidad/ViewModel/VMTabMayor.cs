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
using ModuloContabilidad.TabbedExpanderTabs;
using ModuloContabilidad.ObjModels;

namespace ModuloContabilidad
{
    //++++++++++++++++++++++++++++++++++ OJO +++++++++++++++++++++++++++++++++++++++++++++++++++++
    //TODO: CUANDO SE CAMBIA DE CUENTA ESPERAR 2-3 SEGUNDOS ANTES DE PEDIR A LA BD LA NUEVA CUENTA
    //POR SI SOLO SE ESTA PASANDO DE UNA CUENTA A OTRA LEJANA
    //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
    
    public enum Mayor_SearchType : int { Fecha = 0, Concepto, Importe, ImporteDebe, ImporteHaber, Recibo, Factura }

    public class VMTabMayor : aTabsWithTabExpVM, IVMConAsientoRepo
    {
        public VMTabMayor()
        {
            base.TabType = TabType.Mayor;
            //base.InitializeComcod((Application.Current.MainWindow.DataContext as VMMain).LastComCod);
            InitializeUoW();

            //this._model = new TabMayorModel(base.TabCodigoComunidad); //BORRAME
            base.AddToTaskCargando(SetCuentaAsync(
                GlobalSettings.Properties.Settings.Default.ULTIMACUENTAMAYOR ??
                GlobalSettings.Properties.Settings.Default.CUENTADEFAULT));

            this.TopTabbedExpanderItemsSource = new ObservableCollection<TabExpTabItemBaseVM>();
            this.BottomTabbedExpanderItemsSource = new ObservableCollection<TabExpTabItemBaseVM>();

            this._NextRecord = new Command_NextRecord_Mayor(this);
            this._PrevRecord = new Command_PrevRecord_Mayor(this);
            this._NuevoAsientoSimple = new Command_NewAsientoSimple(this);
            this._Punteo = new Command_Punteo(this);

            this._StatusGridSource = new DataTable();
            RellenaStatusGrid();
        }

        #region fields
        //private TabMayorModel _model;
        private Mayor_SearchType _SearchType;
        private DataTable _StatusGridSource;

        private NotifyTask<CuentaMayor> _Cuenta;
        private NotifyTask<List<Asiento>> _Asientos;
        private ObservableCollection<ApunteParaVistaMayor> _Apuntes;

        private ApunteParaVistaMayor _MayorDGridSelectedItem;

        #region tabbed expander
        private int _TopTabbedExpanderSelectedIndex;
        private int _BottomTabbedExpanderSelectedIndex;
        #endregion
        #endregion

        #region properties
        public CuentaMayorRepository CuentaRepo { get; private set; }
        public AsientoRepository AsientoRepo { get; private set; }
        public UnitOfWork UOW { get; private set; }

        public NotifyTask<CuentaMayor> Cuenta
        {
            get { return this._Cuenta; }
            set
            {
                if (this._Cuenta != value)
                {
                    this._Cuenta = value;
                    //this.Asientos = NotifyTask.Create<List<Asiento>>(
                    //    this.AsientoRepo.GetTodosAsientosCuentaAsync(this._Cuenta.Result),
                    //    x => base.TaskCargando.Remove(x));
                    //base.TaskCargando.Add(this._Asientos);
                    base.AddToTaskCargando(this.AsientoRepo.GetTodosAsientosCuentaAsync(this._Cuenta.Result));
                    NotifyPropChanged("Cuenta");
                }
            }
        }
        public NotifyTask<List<Asiento>> Asientos
        {
            get { return this._Asientos; }
            private set
            {
                if (this._Asientos != value)
                {
                    this._Asientos = value;
                    base.AddToTaskCargando(CalculaSaldosAsync());
                    NotifyPropChanged("Asientos");
                }
            }
        }
        public ObservableCollection<ApunteParaVistaMayor> Apuntes
        {
            get { return this._Apuntes; }
        }
        //public DataView DView { get { return this._model.DView; } }
        public DataView StatusGridSource { get { return this._StatusGridSource.DefaultView; } }
        //TODO: ¿QUE PASA CON ESTE SALDO PUNTEADO CUANDO SE MODIFICA UN APUNTE, HAY QUE PERMITIR QUE SE MODIFIQUE SI ESTA PUNTEADO?
        public decimal StatusGridSaldoPunteado { get { return this.GetSaldoPunteado(); } }

        public ApunteParaVistaMayor MayorDGridSelectedItem
        {
            get { return this._MayorDGridSelectedItem; }
            set
            {
                if(this._MayorDGridSelectedItem != value)
                {
                    this._MayorDGridSelectedItem = value;
                    this.BotTE_AsientoSeleccionado = value.Asiento;
                    
                    NotifyPropChanged("MayorDGridSelectedItem");
                }
            }
        }

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
        public string TopTE_NombreCuenta
        {
            get { return this.Cuenta.Result.Nombre; }
        }
        public string TopTE_CodigoCuenta
        {
            get { return this.Cuenta.Result.Codigo; }
        }
        public Asiento BotTE_AsientoSeleccionado
        {
            get { return this.MayorDGridSelectedItem.Asiento; }
            private set
            {
                //Si al cambiar el asiento seleccionado (un click en la datagrid) estaba seleccionada una pestaña de asiento simple
                //en el TE inferior, manda a su VM el nuevo asiento
                var selectedBottomTabbedExpanderVM = this.BottomTabbedExpanderItemsSource[BottomTabbedExpanderSelectedIndex];
                if (selectedBottomTabbedExpanderVM is TabExpTabAsientoVM)
                {
                    ((TabExpTabAsientoVM)selectedBottomTabbedExpanderVM).Asiento = value;
                }
                
                NotifyPropChanged("BotTE_AsientoSeleccionado");
            }
        }
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
        private Command_NewAsientoSimple _NuevoAsientoSimple;
        private Command_Punteo _Punteo;
        #endregion

        #region commands props
        public ICommand NextRecord { get { return this._NextRecord; } }
        public ICommand PrevRecord { get { return this._PrevRecord; } }
        public ICommand NuevoAsientoSimple { get { return this._NuevoAsientoSimple; } }
        public ICommand Punteo { get { return this._Punteo; } }
        #endregion

        #region helpers        
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        private async Task SetCuentaAsync(string numCuenta)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            int numeroCuenta = int.Parse(numCuenta);
            var ntcCuenta = NotifyTask.Create<CuentaMayor>(
                this.CuentaRepo.GetByNumCuentaAsync(numeroCuenta, base.TabCodigoComunidad, base._TabCodigoEjercicio, this),
                x => base.RemoveFromTaskCargando(x));
                //x => base.TaskCargando.Remove(x));
            //base.TaskCargando.Add(ntcCuenta);
            base.AddToTaskCargando(ntcCuenta);
            this.Cuenta = ntcCuenta;
        }
        private async Task CalculaSaldosAsync()
        {
            //Espera a que se hayan recibido todos los asientos de la BD
            await this._Asientos.Task.ConfigureAwait(false);

            //Crea lista de apuntes para vista solo con los apuntes de esta cuenta y los saldos a 0 para rellenar
            List<ApunteParaVistaMayor> apuntesParaVista = 
                new List<ApunteParaVistaMayor>(this.Asientos.Result             //Partiendo de los asientos de la cuenta
                    .SelectMany(asiento => asiento.Apuntes                      //coge todos los apuntes
                        .Where(apunte => apunte.Cuenta == this._Cuenta.Result)  //de los apuntes coge solo los que sean de esta cuenta
                        .Select(apunte => new ApunteParaVistaMayor(apunte, 0))) //y transformalos en ApunteParaVistaMayor con saldo 0
                    .OrderBy(apuntepv => apuntepv.Fecha));                      //ordenados por fecha de asiento(ya en el objeto), por si se desordenan.
            //Calcula saldo de cada uno
            for (int i = 0;i<apuntesParaVista.Count();i++)
            {
                decimal saldoAnterior = i == 0 ? 0 : apuntesParaVista[i - 1].SaldoEnCuenta;
                decimal saldo = apuntesParaVista[i].ImporteAlDebe - apuntesParaVista[i].ImporteAlHaber + saldoAnterior; //simplemente acumula
                apuntesParaVista[i].SaldoEnCuenta = saldo;
            }

            this._Apuntes = new ObservableCollection<ApunteParaVistaMayor>(apuntesParaVista);

            NotifyPropChanged("Apuntes");
        }
        private void RellenaStatusGrid()
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

            this._StatusGridSource.Rows.Add(new object[] {
                this.Apuntes.Max(apunteparavista => apunteparavista.NAsiento),
                this.Apuntes.Max(apunteparavista => apunteparavista.Fecha),
                "Sumas y Saldos",
                this.Apuntes.Sum(apunteparavista => apunteparavista.ImporteAlDebe),
                this.Apuntes.Sum(apunteparavista => apunteparavista.ImporteAlHaber),
                this.Apuntes.Last().SaldoEnCuenta,
                "",
                ""
                });

            this.NotifyPropChanged("StatusGridSource");
        }
        //**********************************************************************
        //¿REALMENTE HACE FALTA ESTO?
        //private void GetMaxFromColumn(string column, out int maxResult)
        //{
        //    if (this._model.CurrentAccount.CuentaFalsa || this._model.DTable.Rows.Count == 0)
        //    {
        //        maxResult = 0;
        //        return;
        //    }

        //    int max = (int)this._model.DTable.Rows[0][column];
        //    int temp;

        //    for (int i = 0; i < this._model.DTable.Rows.Count; i++)
        //    {
        //        temp = GetValueFromTable<int>(column, i);
        //        if (temp > max) max = temp;
        //    }

        //    maxResult = max;
        //}
        //private void GetMaxFromColumn(string column, out DateTime maxResult)
        //{
        //    if (this._model.CurrentAccount.CuentaFalsa || this._model.DTable.Rows.Count == 0)
        //    {
        //        maxResult = new DateTime(DateTime.Today.Year, 1, 1);
        //        return;
        //    }

        //    DateTime max = (DateTime)this._model.DTable.Rows[0][column];
        //    DateTime temp;

        //    for (int i = 0; i < this._model.DTable.Rows.Count; i++)
        //    {
        //        temp = GetValueFromTable<DateTime>(column, i);
        //        if (temp > max) max = temp;
        //    }

        //    maxResult = max;
        //}
        //**********************************************************************
        //private decimal GetColumnSum(string column)
        //{
        //    if (this._model.CurrentAccount.CuentaFalsa || this._model.DTable.Rows.Count == 0)
        //        return 0;

        //    int sum = 0;

        //    for (int i = 0; i < this._model.DTable.Rows.Count; i++)
        //        sum += GetValueFromTable<int>(column, i);

        //    return sum;
        //}
        private decimal GetSaldoPunteado()
        {
            if (this.Cuenta.Result.CuentaFalsa || this.Apuntes.Count == 0)
                return 0;

            decimal sum = 0;
            foreach(ApunteParaVistaMayor apuntepv in this.Apuntes)
            {
                if(apuntepv.Punteo)
                {
                    sum += apuntepv.ImporteAlDebe;
                    sum -= apuntepv.ImporteAlHaber;
                }
            }

            return sum;
        }
        #endregion

        #region events from code-behind
        public void MayorDGridCell_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            DataGridCell celda = (DataGridCell)sender;

            int column = celda.Column.DisplayIndex;
        }
        #endregion

        #region datatablehelpers overridden methods
        ///// <summary>
        ///// Gets value of type T from datatable column using ConvertFromDBVal. Doesn't check if value is of type T. Only one row supposed(f.i. tab cdades).
        ///// </summary>
        ///// <typeparam name="T"></typeparam>
        ///// <param name="column"></param>
        ///// <param name="row"></param>
        ///// <returns></returns>
        //public override T GetValueFromTable<T>(string column, int row)
        //{
        //    return ConvertFromDBVal<T>(this._model.DTable.Rows[row][column]);
        //}
        ///// <summary>
        ///// Set value of datatable column. Only one row supposed(f.i. tab cdades).
        ///// </summary>
        ///// <param name="column"></param>
        ///// <param name="value"></param>
        //public override void SetValueToTable(string column, object value)
        //{
        //    this._model.DTable.Rows[0][column] = value;
        //}
        #endregion

        #region common commands overridden methods
        /// <summary>
        /// Order model to get data of new account cod.
        /// </summary>
        /// <param name="newCod"></param>
        public override void OnChangedComunidad(int newCod)
        {
            //if (!this._model.ChangeAcc(newCod, base.TabCodigoComunidad))
            //{
            //    //Fake account

            //}
            //else
            //{

            //}
            throw new NotImplementedException();
        }
        public override void OnChangedEjercicio(int newCodigoEjercicio)
        {
            throw new NotImplementedException();
        }

        public override bool IsFirstAccount()
        {
            return this.Cuenta.Result.IsFirstAccount();
        }
        public override bool IsLastAccount()
        {
            return this.Cuenta.Result.IsLastAccount();
        }
        #endregion

        #region UoW
        /// <summary>
        /// Llamado por AbleTabControl cuando se cierra la pestaña
        /// </summary>
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public override async Task CleanUnitOfWorkAsync()
        {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            this.UOW.RemoveVMTabReferencesFromReposAsync().Forget().ConfigureAwait(false);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        }
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        public override void InitializeUoW()
        {
            IAppRepositories appRepos = (IAppRepositories)Application.Current;
            HashSet<aRepositoryInternal> repos = new HashSet<aRepositoryInternal>();

            repos.Add(appRepos.CuentaMayorRepo);
            repos.Add(appRepos.AsientoRepo);
            repos.Add(appRepos.ApunteRepo);
            this.UOW = new UnitOfWork(repos, this);

            this.CuentaRepo = appRepos.CuentaMayorRepo;
            this.AsientoRepo = appRepos.AsientoRepo;
        }
        #endregion
    }
}
