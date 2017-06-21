using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using AdConta;
using AdConta.ViewModel;
using AdConta.Models;
using Mapper;
using QBuilder;
using System.Threading;

namespace Repository
{
    public sealed class PersonaRepository : aRepositoryBase, IRepositoryInternal
    {
        public PersonaRepository()
        {
            MapperStore store = new MapperStore();
            this._Mapper = (DapperMapper<Persona>)store.GetMapper(typeof(Persona));
        }

        #region fields
        private ConcurrentDictionary<aVMTabBase, HashSet<Persona>> _NewObjects = new ConcurrentDictionary<aVMTabBase, HashSet<Persona>>();
        private ConcurrentDictionary<int, Persona> _ObjModels = new ConcurrentDictionary<int, Persona>();
        private ConcurrentDictionary<int, Persona> _OriginalObjModels = new ConcurrentDictionary<int, Persona>();
        private ConcurrentDictionary<aVMTabBase, HashSet<Persona>> _ObjectsRemoved = new ConcurrentDictionary<aVMTabBase, HashSet<Persona>>();
        private DapperMapper<Persona> _Mapper;
        #endregion

        #region properties
        public ConcurrentDictionary<aVMTabBase, List<Tuple<QueryBuilder, IConditionToCommit>>> Transactions { get; private set; }
        #endregion

        #region helpers
        public override Type GetObjModelType()
        {
            return typeof(Persona);
        }
        void IRepositoryInternal.NewVM(aVMTabBase VM)
        {
            base._RepoSphr.Wait();
            if (!this.Transactions.ContainsKey(VM)) this.Transactions.TryAdd(VM, new List<Tuple<QueryBuilder, IConditionToCommit>>());
            if (!this._NewObjects.ContainsKey(VM)) this._NewObjects.TryAdd(VM, new HashSet<Persona>());
            if (!this._ObjectsRemoved.ContainsKey(VM)) this._ObjectsRemoved.TryAdd(VM, new HashSet<Persona>());
            if (!base._DirtyMembers.ContainsKey(VM)) base._DirtyMembers.TryAdd(VM, new Dictionary<int, string[]>());
            base._RepoSphr.Release();
        }
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        async Task IRepositoryInternal.RemoveVMTabReferences(aVMTabBase VM)
        {
            HashSet<Persona> setDump;
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
        async Task IRepositoryInternal.RollbackRepoAsync(aVMTabBase VM)
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
        async Task IRepositoryInternal.ApplyChangesAsync(aVMTabBase VM)
        {
            await base._RepoSphr.WaitAsync();
            foreach(Persona p in this._NewObjects[VM]) _ObjModels.TryAdd(p.Id, p); //All old new objects are now normal objects
            this._NewObjects[VM].Clear(); //Therefore, clear new objects
            this.Transactions[VM].Clear(); //Transactions made, clear transactions
            this._ObjectsRemoved[VM].Clear(); //Apply deletes
            base._DirtyMembers[VM].Clear(); //Clear objects members modified
            base._RepoSphr.Release();
        }
        #endregion

        #region SQL helpers
        protected override QueryBuilder GetUpdateSQL(int id, aVMTabBase VM)
        {
            if (this._NewObjects[VM].Select(p => p.Id).Contains(id) || //If the object have been newly created it needs an INSERT not an UPDATE
                !this._DirtyMembers[VM].ContainsKey(id)) //If there are no dirty members the object haven't been modified
                return null;

            Type t = GetObjModelType();
            QueryBuilder qBuilder = new QueryBuilder();
            qBuilder
                .AddUpdate(t)
                .AddUpdateSet(this._DirtyMembers[VM][id])
                .AddWhere(new SQLCondition("Id", "@id"));
            qBuilder.StoreParametersFrom(this._ObjModels[id]);
            qBuilder.StoreParameter("id", id);
            return qBuilder;
        }
        private QueryBuilder GetInsertSQL(Persona p)
        {
            Type t = GetObjModelType();
            QueryBuilder qBuilder = new QueryBuilder();
            qBuilder
                .AddInsertInto()
                .AddInsertFirstColumns(t)
                .CloseBrackets()
                .AddInsertValues()
                .AddInsertValues(t)
                .CloseBrackets()
                .SemiColon();
            qBuilder.StoreParametersFrom(p);
            return qBuilder;
        }
        #endregion

        #region public methods
        public async Task<Persona> GetById(int id)
        {
            Persona p;
            if(!this._ObjModels.TryGetValue(id, out p))
            {
                QueryBuilder qBuilder = await Task.Run(() => GetSelectSQL(id)).ConfigureAwait(false);
                dynamic result;
                using (SqlConnection con = new SqlConnection(this._strCon))
                {
                    await con.OpenAsync().ConfigureAwait(false);
                    result = await con.QueryAsync(qBuilder.Query, qBuilder.Parameters).ConfigureAwait(false);
                    con.Close();
                }
                p = await Task.Run(() => this._Mapper.Map(result)).ConfigureAwait(false);
                //Add object retrieved from DB
                this._OriginalObjModels.TryAdd(id, p);
                //Deep copy
                this._ObjModels.TryAdd(id, await Task.Run(() => this._Mapper.Map(result)).ConfigureAwait(false));
            }
            return p;
        }
        public async Task<bool> AddNew(Persona p, aVMTabBase VM)
        {
            if (!this._NewObjects[VM].Add(p)) return false;

            QueryBuilder SQL = await Task.Run(() => GetInsertSQL(p)).ConfigureAwait(false);
            ConditionToCommitScalar<int> condition = new ConditionToCommitScalar<int>(ConditionTCType.equal, 1);
            var tuple = new Tuple<QueryBuilder, IConditionToCommit>(SQL, condition);

            await base._RepoSphr.WaitAsync();

            if (this.Transactions[VM].Contains(tuple)) return false;
            else this.Transactions[VM].Add(tuple);

            base._RepoSphr.Release();
            return true;
        }
        public async Task<bool> Update(Persona p, aVMTabBase VM)
        {
            QueryBuilder SQL = await Task.Run(() => GetUpdateSQL(p.Id, VM)).ConfigureAwait(false);
            if (SQL == null) return false;

            ConditionToCommitScalar<int> condition = new ConditionToCommitScalar<int>(ConditionTCType.equal, 1);
            var tuple = new Tuple<QueryBuilder, IConditionToCommit>(SQL, condition);

            await base._RepoSphr.WaitAsync();

            if (this.Transactions[VM].Contains(tuple)) return false;
            else this.Transactions[VM].Add(tuple);

            base._RepoSphr.Release();
            return true;
        }
        public async Task<bool> Remove(Persona p, aVMTabBase VM)
        {
            if (!this._ObjModels.ContainsKey(p.Id)) return false;

            QueryBuilder SQL = await Task.Run(() => GetDeleteSQL(p.Id)).ConfigureAwait(false);
            ConditionToCommitScalar<int> condition = new ConditionToCommitScalar<int>(ConditionTCType.equal, 1);
            var tuple = new Tuple<QueryBuilder, IConditionToCommit>(SQL, condition);

            await base._RepoSphr.WaitAsync();

            if (this.Transactions[VM].Contains(tuple)) return false;
            else this.Transactions[VM].Add(tuple);

            this._ObjectsRemoved[VM].Add(p); //Add object to ObjectsRemoved for later commit or rollback

            Persona dump;
            this._ObjModels.TryRemove(p.Id, out dump);
            this._OriginalObjModels.TryRemove(p.Id, out dump); //Doesn't matter if this one really had or not the object before, because the object
            //could be added before by the VM, and in that case it would be added only to this._ObjModels

            base._RepoSphr.Release();
            return true;
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
