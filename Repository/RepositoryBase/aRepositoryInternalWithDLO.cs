using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AdConta.ViewModel;
using AdConta.Models;
using QBuilder;
using System.Collections.Concurrent;
using Extensions;
using Mapper;
using System.Collections.ObjectModel;
using System.Collections;

namespace Repository
{
    public abstract class aRepositoryInternalWithDLO<T, DLO> : aRepositoryInternal<T>, IRepositoryDLO<DLO>
        where T : IObjModelBase
        where DLO : IDataListObject
    {
        public aRepositoryInternalWithDLO()
        {
            MapperStore store = new MapperStore();
            this._DLOMapper = (DapperMapper<DLO>)store.GetMapper(typeof(DLO));
        }

        #region fields
        private HashSet<aVMTabBase> _VMs = new HashSet<aVMTabBase>();
        protected DapperMapper<DLO> _DLOMapper;
        #endregion

        #region properties
        public ReadOnlyCollection<DLO> DLOs { get; private set; }
        #endregion

        #region helpers
        internal override async Task ApplyChangesAsync(aVMTabBase VM, Func<Task> doFirstInsideSemaphoreWaiting = null, Func<Task> doLastInsideSemaphoreWaiting = null)
        {
            Func<Task> thisFunc = async () => await UpdateDLOsDictionariesAsync(VM.GetOwners());
            await base.ApplyChangesAsync(VM, doFirstInsideSemaphoreWaiting, doLastInsideSemaphoreWaiting += thisFunc);
        }
        internal override async Task RollbackRepoAsync(aVMTabBase VM, Func<Task> doFirstInsideSemaphoreWaiting = null, Func<Task> doLastInsideSemaphoreWaiting = null)
        {
            Func<Task> thisFunc = async () => await UpdateDLOsDictionariesAsync(VM.GetOwners());
            await base.RollbackRepoAsync(VM, doFirstInsideSemaphoreWaiting, doLastInsideSemaphoreWaiting += thisFunc);
        }
        internal override async Task RemoveVMTabReferencesAsync(aVMTabBase VM, Func<Task> doFirstInsideSemaphoreWaiting = null, Func<Task> doLastInsideSemaphoreWaiting = null)
        {
            Func<Task> thisFunc = async () =>
               {
                   this._VMs.Remove(VM);
                   if (this._VMs.Count == 0)
                       await CleanDLOsDictionariesAsync();
               };
            await base.RemoveVMTabReferencesAsync(VM, doFirstInsideSemaphoreWaiting, doLastInsideSemaphoreWaiting += thisFunc);
        }
        #endregion

        #region public methods
        protected abstract Task<ReadOnlyCollection<DLO>> GetDLOsAsync(IEnumerable<int> owners);
        public virtual async Task UpdateDLOsDictionariesAsync(IEnumerable<int> owners)
        {
            await base._RepoSphr.WaitAsync();
            if(owners == null)
                throw new ArgumentException("EjercicioRepository.GetDLOs2Async: El argumento de este método no puede ser nulo");
            this.DLOs = await GetDLOsAsync(owners);
            base._RepoSphr.Release();
        }
        public virtual async Task CleanDLOsDictionariesAsync()
        {
            await base._RepoSphr.WaitAsync();
            ((IList)this.DLOs).Clear();
            base._RepoSphr.Release();
        }
        #endregion

        #region Dispose
        public override void Dispose()
        {
            base.Dispose();
        }
        #endregion
    }

    public abstract class aRepositoryInternalWithTwoDLOs<T, DLO, DLO2> : aRepositoryInternal<T>, IRepository2DLOs<DLO, DLO2>
        where T : IObjModelBase
        where DLO : IDataListObject
        where DLO2 : IDataListObject
    {
        public aRepositoryInternalWithTwoDLOs()
        {
            MapperStore store = new MapperStore();
            this._DLOMapper1 = (DapperMapper<DLO>)store.GetMapper(typeof(DLO));
            this._DLOMapper2 = (DapperMapper<DLO2>)store.GetMapper(typeof(DLO2));
        }

        #region fields
        private HashSet<aVMTabBase> _VMs1 = new HashSet<aVMTabBase>();
        protected DapperMapper<DLO> _DLOMapper1;
        private HashSet<aVMTabBase> _VMs2 = new HashSet<aVMTabBase>();
        protected DapperMapper<DLO2> _DLOMapper2;
        #endregion

        #region properties
        public ReadOnlyCollection<DLO> DLOs1 { get; private set; }
        public ReadOnlyCollection<DLO2> DLOs2 { get; private set; }
        #endregion

        #region helpers
        internal override async Task ApplyChangesAsync(aVMTabBase VM, Func<Task> doFirstInsideSemaphoreWaiting, Func<Task> doLastInsideSemaphoreWaiting)
        {
            Func<Task> thisFunc = async () => await UpdateDLOsDictionariesAsync(VM.GetOwners());
            await base.ApplyChangesAsync(VM, doFirstInsideSemaphoreWaiting, doLastInsideSemaphoreWaiting += thisFunc);
        }
        internal override async Task RollbackRepoAsync(aVMTabBase VM, Func<Task> doFirstInsideSemaphoreWaiting, Func<Task> doLastInsideSemaphoreWaiting)
        {
            Func<Task> thisFunc = async () => await UpdateDLOsDictionariesAsync(VM.GetOwners());
            await base.RollbackRepoAsync(VM, doFirstInsideSemaphoreWaiting, doLastInsideSemaphoreWaiting += thisFunc);
        }
        internal override async Task RemoveVMTabReferencesAsync(aVMTabBase VM, Func<Task> doFirstInsideSemaphoreWaiting, Func<Task> doLastInsideSemaphoreWaiting)
        {
            Func<Task> thisFunc = async () =>
            {
                this._VMs1.Remove(VM);
                if (this._VMs1.Count == 0)
                    await CleanDLOs1DictionariesAsync();

                this._VMs2.Remove(VM);
                if (this._VMs2.Count == 0)
                    await CleanDLOs2DictionariesAsync();
            };
            await base.RemoveVMTabReferencesAsync(VM, doFirstInsideSemaphoreWaiting, doLastInsideSemaphoreWaiting += thisFunc);
        }
        #endregion

        #region public methods
        protected abstract Task<ReadOnlyCollection<DLO>> GetDLOs1Async(IEnumerable<int> owners);
        protected abstract Task<ReadOnlyCollection<DLO2>> GetDLOs2Async(IEnumerable<int> owners);
        public virtual async Task UpdateDLOsDictionariesAsync(IEnumerable<int> owners)
        {
            await base._RepoSphr.WaitAsync();
            if (owners == null)
                throw new ArgumentException("EjercicioRepository.GetDLOs2Async: El argumento de este método no puede ser nulo");
            this.DLOs1 = await GetDLOs1Async(owners);
            this.DLOs2 = await GetDLOs2Async(owners);
            base._RepoSphr.Release();
        }
        public virtual async Task CleanDLOs1DictionariesAsync()
        {
            await base._RepoSphr.WaitAsync();
            ((IList)this.DLOs1).Clear();
            base._RepoSphr.Release();
        }
        public virtual async Task CleanDLOs2DictionariesAsync()
        {
            await base._RepoSphr.WaitAsync();
            ((IList)this.DLOs2).Clear();
            base._RepoSphr.Release();
        }
        #endregion

        #region Dispose
        public override void Dispose()
        {
            base.Dispose();
        }
        #endregion
    }
}
