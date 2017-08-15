using AdConta.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using Repository;
using Extensions;
using AdConta.Models;
using System.Windows;

namespace AdConta.SideTool
{
    public class VMSideTool : aVMTabBase
    {
        public VMSideTool()
        {
            this._F2KeyBinding = new Command_F2KeyBinding(this);
        }

        #region fields
        private ObservableCollection<SideToolTabItem> _TabItemsSource;
        private ObservableCollection<SideToolEjercicioItem> _EjercicioItemsSource;
        private bool _ExIsExpanded;

        #endregion

        #region properties
        public UnitOfWork UOW { get; private set; }
        public EjercicioRepository EjercicioRepo { get; private set; }
        public ObservableCollection<SideToolTabItem> TabItemsSource
        {
            get { return this._TabItemsSource; }
            set
            {
                this._TabItemsSource = value;
                NotifyPropChanged("TabItemsSource");
            }
        }
        public ObservableCollection<SideToolEjercicioItem> EjercicioItemsSource
        {
            get { return this._EjercicioItemsSource; }
            set
            {
                this._EjercicioItemsSource = value;
                NotifyPropChanged("EjercicioItemsSource");
            }
        }
        public bool ExIsExpanded
        {
            get { return this._ExIsExpanded; }
            set
            {
                if (value != this._ExIsExpanded)
                {
                    VMMain vm = App.Current.MainWindow.DataContext as VMMain;
                    vm.ZIndex = (value == true ? -1 : 1);

                    this._ExIsExpanded = value;
                    NotifyPropChanged("ExIsExpanded");
                }
            }
        }
        #endregion

        #region commands
        private Command_F2KeyBinding _F2KeyBinding;
        public ICommand F2KeyBinding { get { return _F2KeyBinding; } }
        #endregion

        #region helpers
        public async Task OnTabItemButtonClickAsync(int comCod, TabType tabType)
        {
            Task updateDLOs = this.EjercicioRepo.UpdateDLOsDictionariesAsync(new int[] { comCod });
            
            this.ExIsExpanded = false;
            VMMain mainVM = App.Current.MainWindow.DataContext as VMMain;
            mainVM.SetNewTabCodigoComunidad(comCod, tabType);

            await updateDLOs;
            FillEjercicioItems();
        }
        public void FillEjercicioItems()
        {
            this.EjercicioItemsSource.Clear();
            SideToolEjercicioItem min = null;
            EjercicioDLOParaSideTool minDLO = null;

            foreach(EjercicioDLOParaSideTool DLO in this.EjercicioRepo.DLOs2)
            {
                SideToolEjercicioItem item = new SideToolEjercicioItem(this, DLO);
                if (EjercicioDLOParaSideTool.Min(min.EjercicioDLO, DLO) == DLO)
                {
                    minDLO = DLO;
                    min = item;
                }                
                this.EjercicioItemsSource.Add(item);
            }

            min.IsSelected = true;
            VMMain mainVM = App.Current.MainWindow.DataContext as VMMain;
            mainVM.SetNewTabEjercicio(minDLO);
        }

        public async Task OnEjercicioItemButtonClickAsync(EjercicioDLOParaSideTool DLO)
        {
            VMMain mainVM = App.Current.MainWindow.DataContext as VMMain;
            mainVM.SetNewTabEjercicio(DLO);
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

        public override void InitializeUoW()
        {
            IAppRepositories appRepos = (IAppRepositories)Application.Current;
            HashSet<aRepositoryInternal> repos = new HashSet<aRepositoryInternal>();

            repos.Add(appRepos.EjercicioRepo);
            this.UOW = new UnitOfWork(repos, this);

            this.EjercicioRepo = appRepos.EjercicioRepo;
        }
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        #endregion

        public override void OnChangedComunidad(int newCodigoComunidad)
        {
            throw new NotImplementedException();
        }
        public override void OnChangedEjercicio(int newCodigoEjercicio)
        {
            throw new NotImplementedException();
        }
    }

    public class Command_F2KeyBinding : ICommand
    {
        private VMSideTool _Side;

        public Command_F2KeyBinding(VMSideTool side)
        {
            _Side = side;
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
            this._Side.ExIsExpanded = !this._Side.ExIsExpanded;
        }
    }
}
