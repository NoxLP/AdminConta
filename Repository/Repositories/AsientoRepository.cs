using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AdConta.ViewModel;
using QBuilder;
using ModuloContabilidad.ObjModels;
using Mapper;

namespace Repository
{
    public sealed class AsientoRepository : aRepositoryBase, IRepository
    {
        public AsientoRepository()
        {
            MapperStore store = new MapperStore();
            this._Mapper = (DapperMapper<Asiento>)store.GetMapper(GetObjModelType());
        }

        #region fields
        private ConcurrentDictionary<aVMTabBase, HashSet<Asiento>> _NewObjects = new ConcurrentDictionary<aVMTabBase, HashSet<Asiento>>();
        private ConcurrentDictionary<int, Asiento> _ObjModels = new ConcurrentDictionary<int, Asiento>();
        private ConcurrentDictionary<int, Asiento> _OriginalObjModels = new ConcurrentDictionary<int, Asiento>();
        private ConcurrentDictionary<aVMTabBase, HashSet<Asiento>> _ObjectsRemoved = new ConcurrentDictionary<aVMTabBase, HashSet<Asiento>>();
        private DapperMapper<Asiento> _Mapper;
        #endregion

        #region properties
        public ConcurrentDictionary<aVMTabBase, List<Tuple<QueryBuilder, IConditionToCommit>>> Transactions { get; private set; }
        #endregion

        #region helpers
        public override Type GetObjModelType()
        {
            return typeof(Asiento);
        }
        public void NewVM(aVMTabBase VM)
        {
            base._RepoSphr.Wait();
            if (!this.Transactions.ContainsKey(VM)) this.Transactions.TryAdd(VM, new List<Tuple<QueryBuilder, IConditionToCommit>>());
            if (!this._NewObjects.ContainsKey(VM)) this._NewObjects.TryAdd(VM, new HashSet<Asiento>());
            if (!this._ObjectsRemoved.ContainsKey(VM)) this._ObjectsRemoved.TryAdd(VM, new HashSet<Asiento>());
            if (!base._DirtyMembers.ContainsKey(VM)) base._DirtyMembers.TryAdd(VM, new Dictionary<int, string[]>());
            base._RepoSphr.Release();
        }
        public void RemoveVMTabReferences(aVMTabBase VM)
        {
            HashSet<Asiento> setDump;
            List<Tuple<QueryBuilder, IConditionToCommit>> lDump;
            Dictionary<int, string[]> dDump;

            base._RepoSphr.Wait();
            this._NewObjects.TryRemove(VM, out setDump);
            this.Transactions.TryRemove(VM, out lDump);
            this._ObjectsRemoved.TryRemove(VM, out setDump);
            base._DirtyMembers.TryRemove(VM, out dDump);
            base._RepoSphr.Release();
        }
        public async Task ApplyChangesAsync(aVMTabBase VM)
        {
            await base._RepoSphr.WaitAsync();
            foreach (Asiento obj in this._NewObjects[VM]) _ObjModels.TryAdd(obj.Id, obj); //All old new objects are now normal objects
            this._NewObjects[VM].Clear(); //Therefore, clear new objects
            this.Transactions[VM].Clear(); //Transactions made, clear transactions
            this._ObjectsRemoved[VM].Clear(); //Apply deletes
            base._DirtyMembers[VM].Clear(); //Clear objects members modified
            base._RepoSphr.Release();
        }
        public async Task RollbackRepoAsync(aVMTabBase VM)
        {
            await base._RepoSphr.WaitAsync();

            foreach (KeyValuePair<int, string[]> kvp in base._DirtyMembers[VM])
                this._ObjModels[kvp.Key] = this._OriginalObjModels[kvp.Key]; //Change back al objects to their original state
            //¡¡¡¡OJO!!!! This above only changes dictionary reference, if VM or others had a reference to the object, it will not GCollected AND
            //the owner/s will maintain the reference to the bad, modified, "not-rollbacked" object.

            this._NewObjects[VM].Clear(); //Clear new objects
            this.Transactions[VM].Clear(); //Transactions made, clear transactions
            base._DirtyMembers[VM].Clear(); //Clear objects members modified

            base._RepoSphr.Release();
        }
        #endregion

        #region SQL helpers
        protected override QueryBuilder GetSelectSQL(int id)
        {
            throw new NotImplementedException();
        }
        protected override QueryBuilder GetUpdateSQL(int id, aVMTabBase VM)
        {
            throw new NotImplementedException();
        }
        private QueryBuilder GetInsertSQL(Asiento cuenta)
        {
            throw new NotImplementedException();
        }
        protected override QueryBuilder GetDeleteSQL(int id)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region public methods
        public async Task<Asiento> GetById(int id)
        {
            throw new NotImplementedException();
        }
        public async Task<bool> AddNew(Asiento AsientoObj, aVMTabBase VM)
        {
            throw new NotImplementedException();
        }
        public async Task<bool> Update(Asiento AsientoObj, aVMTabBase VM)
        {
            throw new NotImplementedException();
        }
        public async Task<bool> Remove(Asiento AsientoObj, aVMTabBase VM)
        {
            throw new NotImplementedException();
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
