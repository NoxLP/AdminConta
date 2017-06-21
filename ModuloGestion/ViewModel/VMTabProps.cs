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
            base.Type = TabType.Props;
            //this.TabComCod = (Application.Current.MainWindow.DataContext as VMMain).LastComCod;
            try { base.InitializeComcod((int)Messenger.Messenger.SearchMsg("LastComCod")); }
            catch (Exception)
            {
                MessageBox.Show("No se pudo abrir la pestaña de libro mayor por falta del código de Comunidad");
                return;
            }
            InitUoWAsync().Forget().ConfigureAwait(false);
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
            
            repos.Add(appRepos.PropietarioRepo);
            this.UOW = new UnitOfWork(repos, this);

            this.PropietarioRepo = appRepos.PropietarioRepo;
        }
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        #endregion
    }
}
