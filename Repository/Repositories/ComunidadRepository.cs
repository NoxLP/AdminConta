using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AdConta.ViewModel;
using QBuilder;
using Mapper;
using AdConta.Models;
using ModuloContabilidad.ObjModels;

namespace Repository
{
    public sealed class ComunidadRepository : aRepositoryBase, IRepositoryInternal
    {
        public ComunidadRepository()
        {
            MapperStore store = new MapperStore();
            this._Mapper = (DapperMapper<Comunidad>)store.GetMapper(GetObjModelType());
        }

        #region fields
        /// <summary>
        /// Newly objects added here till ApplyChangesAsync get called.
        /// </summary>
        private ConcurrentDictionary<aVMTabBase, HashSet<Comunidad>> _NewObjects = new ConcurrentDictionary<aVMTabBase, HashSet<Comunidad>>();
        /// <summary>
        /// Objects currently retrieved from DB.
        /// </summary>
        private ConcurrentDictionary<int, Comunidad> _ObjModels = new ConcurrentDictionary<int, Comunidad>();
        /// <summary>
        /// Original objects currently retrieved from DB. Deep copy of thos stored at _ObjModels. Used in the case of a rollback.
        /// </summary>
        private ConcurrentDictionary<int, Comunidad> _OriginalObjModels = new ConcurrentDictionary<int, Comunidad>();
        /// <summary>
        /// Objects removed are added here till ApplyChangesAsync get called.
        /// </summary>
        private ConcurrentDictionary<aVMTabBase, HashSet<Comunidad>> _ObjectsRemoved = new ConcurrentDictionary<aVMTabBase, HashSet<Comunidad>>();
        /// <summary>
        /// Objects type mapper. Initialized at constructor.
        /// </summary>
        private DapperMapper<Comunidad> _Mapper;
        #endregion

        #region properties
        public ConcurrentDictionary<aVMTabBase, List<Tuple<QueryBuilder, IConditionToCommit>>> Transactions { get; private set; }
        #endregion

        #region helpers
        public override Type GetObjModelType()
        {
            return typeof(Comunidad);
        }
        void IRepositoryInternal.NewVM(aVMTabBase VM)
        {
            base._RepoSphr.Wait();
            if (!this.Transactions.ContainsKey(VM)) this.Transactions.TryAdd(VM, new List<Tuple<QueryBuilder, IConditionToCommit>>());
            if (!this._NewObjects.ContainsKey(VM)) this._NewObjects.TryAdd(VM, new HashSet<Comunidad>());
            if (!this._ObjectsRemoved.ContainsKey(VM)) this._ObjectsRemoved.TryAdd(VM, new HashSet<Comunidad>());
            if (!base._DirtyMembers.ContainsKey(VM)) base._DirtyMembers.TryAdd(VM, new Dictionary<int, string[]>());
            base._RepoSphr.Release();
        }
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        async Task IRepositoryInternal.RemoveVMTabReferences(aVMTabBase VM)
        {
            HashSet<Comunidad> setDump;
            List<Tuple<QueryBuilder, IConditionToCommit>> lDump;
            Dictionary<int, string[]> dDump;

            base._RepoSphr.Wait();
            this._NewObjects.TryRemove(VM, out setDump);
            this.Transactions.TryRemove(VM, out lDump);
            this._ObjectsRemoved.TryRemove(VM, out setDump);
            base._DirtyMembers.TryRemove(VM, out dDump);
            base._RepoSphr.Release();
        }
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        async Task IRepositoryInternal.ApplyChangesAsync(aVMTabBase VM)
        {
            await base._RepoSphr.WaitAsync();
            foreach (Comunidad obj in this._NewObjects[VM]) _ObjModels.TryAdd(obj.Id, obj); //All old new objects are now normal objects
            this._NewObjects[VM].Clear(); //Therefore, clear new objects
            this.Transactions[VM].Clear(); //Transactions made, clear transactions
            this._ObjectsRemoved[VM].Clear(); //Apply deletes
            base._DirtyMembers[VM].Clear(); //Clear objects members modified
            base._RepoSphr.Release();
        }
        async Task IRepositoryInternal.RollbackRepoAsync(aVMTabBase VM)
        {
            await base._RepoSphr.WaitAsync();

            foreach (KeyValuePair<int, string[]> kvp in base._DirtyMembers[VM])
                this._ObjModels[kvp.Key] = this._OriginalObjModels[kvp.Key]; //Change back al objects to their original state
            //����OJO!!!! This above only changes dictionary reference, if VM or others had a reference to the object, it will not GCollected AND
            //the owner/s will maintain the reference to the bad, modified, "not-rollbacked" object.

            this._NewObjects[VM].Clear(); //Clear new objects
            this.Transactions[VM].Clear(); //Transactions made, clear transactions
            this._ObjModels = new ConcurrentDictionary<int, Comunidad>(
                this._ObjModels.Union(this._ObjectsRemoved[VM].ToDictionary(x => x.Id, x => x))); //Change back objects removed
            this._OriginalObjModels = new ConcurrentDictionary<int, Comunidad>(
                this._OriginalObjModels.Union(this._ObjectsRemoved[VM].ToDictionary(x => x.Id, x => x))); //Change back objects removed
            this._ObjectsRemoved[VM].Clear();
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
        private QueryBuilder GetInsertSQL(Comunidad cuenta)
        {
            throw new NotImplementedException();
        }
        protected override QueryBuilder GetDeleteSQL(int id)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region public methods
        public async Task<Comunidad> GetById(int id)
        {
            throw new NotImplementedException();
        }
        public async Task<bool> AddNew(Comunidad ComunidadObj, aVMTabBase VM)
        {
            throw new NotImplementedException();
        }
        public async Task<bool> Update(Comunidad ComunidadObj, aVMTabBase VM)
        {
            throw new NotImplementedException();
        }
        public async Task<bool> Remove(Comunidad ComunidadObj, aVMTabBase VM)
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
