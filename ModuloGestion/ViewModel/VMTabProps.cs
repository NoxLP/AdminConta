using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using AdConta;
using AdConta.ViewModel;
using Repository;
using Extensions;

namespace ModuloGestion
{
    public class VMTabProps : aVMTabBase
    {
        public VMTabProps()
        {
            base.TabType = TabType.Props;
            //this.TabComCod = (Application.Current.MainWindow.DataContext as VMMain).LastComCod;
            InitializeUoW();
        }

        #region properties
        public UnitOfWork UOW { get; private set; }
        public PropietarioRepository PropietarioRepo { get; private set; }
        #endregion

        #region datatablehelpers overrided methods
        public override T GetValueFromTable<T>(string column)
        {
            throw new NotImplementedException();
        }
        public override void SetValueToTable(string column, object value)
        {
            throw new NotImplementedException();
        }
        public override T ConvertFromDBVal<T>(object obj)
        {
            throw new NotImplementedException();
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
            
            repos.Add(appRepos.PropietarioRepo);
            this.UOW = new UnitOfWork(repos, this);

            this.PropietarioRepo = appRepos.PropietarioRepo;
        }
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        #endregion
        
        public override void OnChangedEjercicio(int newCodigoEjercicio)
        {
            throw new NotImplementedException();
        }
        public override void OnChangedComunidad(int newCodigoComunidad)
        {
            throw new NotImplementedException();
        }
    }
}
