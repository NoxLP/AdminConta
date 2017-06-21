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
using AdConta;
using System.Data.SqlClient;
using Dapper;
using Extensions;

namespace Repository
{
    public sealed class CuentaMayorRepository : aRepositoryBaseWithTwoOwners, IRepositoryInternal
    {
        public CuentaMayorRepository()
        {
            MapperStore store = new MapperStore();
            this._Mapper = (DapperMapper<CuentaMayor>)store.GetMapper(GetObjModelType());
        }

        #region fields
        /// <summary>
        /// Newly objects added here till ApplyChangesAsync get called.
        /// </summary>
        private ConcurrentDictionary<aVMTabBase, HashSet<CuentaMayor>> _NewObjects = new ConcurrentDictionary<aVMTabBase, HashSet<CuentaMayor>>();
        /// <summary>
        /// Objects currently retrieved from DB.
        /// </summary>
        private ConcurrentDictionary<int, CuentaMayor> _ObjModels = new ConcurrentDictionary<int, CuentaMayor>();
        /// <summary>
        /// Relations between owners, numCuenta and Id. Used to manage objects by numCuenta instead of by Id.
        /// </summary>
        private ConcurrentDictionary<Tuple<int, long>, int> _CuentasIds = new ConcurrentDictionary<Tuple<int, long>, int>();
        /// <summary>
        /// Original objects currently retrieved from DB. Deep copy of thos stored at _ObjModels. Used in the case of a rollback.
        /// </summary>
        private ConcurrentDictionary<int, CuentaMayor> _OriginalObjModels = new ConcurrentDictionary<int, CuentaMayor>();
        /// <summary>
        /// Objects removed are added here till ApplyChangesAsync get called.
        /// </summary>
        private ConcurrentDictionary<aVMTabBase, HashSet<CuentaMayor>> _ObjectsRemoved = new ConcurrentDictionary<aVMTabBase, HashSet<CuentaMayor>>();
        /// <summary>
        /// Objects type mapper. Initialized at constructor.
        /// </summary>
        private DapperMapper<CuentaMayor> _Mapper;
        #endregion

        #region properties
        public ConcurrentDictionary<aVMTabBase, List<Tuple<QueryBuilder, IConditionToCommit>>> Transactions { get; private set; }
        #endregion

        #region helpers
        public override Type GetObjModelType()
        {
            return typeof(CuentaMayor);
        }
        void IRepositoryInternal.NewVM(aVMTabBase VM)
        {
            base._RepoSphr.Wait();
            if (!this.Transactions.ContainsKey(VM)) this.Transactions.TryAdd(VM, new List<Tuple<QueryBuilder, IConditionToCommit>>());
            if (!this._NewObjects.ContainsKey(VM)) this._NewObjects.TryAdd(VM, new HashSet<CuentaMayor>());
            if (!this._ObjectsRemoved.ContainsKey(VM)) this._ObjectsRemoved.TryAdd(VM, new HashSet<CuentaMayor>());
            if (!base._DirtyMembers.ContainsKey(VM)) base._DirtyMembers.TryAdd(VM, new Dictionary<int, string[]>());
            base._RepoSphr.Release();
        }
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        async Task IRepositoryInternal.RemoveVMTabReferences(aVMTabBase VM)
        {
            HashSet<CuentaMayor> setDump;
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
            foreach (CuentaMayor p in this._NewObjects[VM]) //All old new objects are now normal objects
            {
                _ObjModels.TryAdd(p.Id, p); 
                Tuple<int, long> key = new Tuple<int, long>(p.NumCuenta, p.IdOwnerComunidad.CantorPair(p.IdOwnerEjercicio));
                this._CuentasIds.TryAdd(key, p.Id);
            }
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
            //¡¡¡¡OJO!!!! This above only changes dictionary reference, if VM or others had a reference to the object, it will not GCollected AND
            //the owner/s will maintain the reference to the bad, modified, "not-rollbacked" object.

            this._NewObjects[VM].Clear(); //Clear new objects
            this.Transactions[VM].Clear(); //Transactions made, clear transactions
            base._DirtyMembers[VM].Clear(); //Clear objects members modified

            base._RepoSphr.Release();
        }
        #endregion

        #region SQL helpers
        private QueryBuilder GetSelectSQLByNumCuenta(int numCuenta, int idCdad, int idEjer)
        {
            Type t = GetObjModelType();
            QueryBuilder qBuilder = new QueryBuilder();
            var ownerConditions = GetCurrentOwnersCondition();
            qBuilder
                .AddSelect(t)
                .AddFrom(t)
                .AddWhere(new SQLCondition("Codigo", "@codigo"))
                .AddConditions("AND ", ownerConditions)
                .SemiColon();

            qBuilder.StoreParameter("idCdad", CurrentCdadOwner);
            qBuilder.StoreParameter("idEjer", CurrentEjerOwner);
            qBuilder.StoreParameter("codigo", numCuenta.ToString());

            //Type asiType = typeof(Asiento);
            //Type apuType = typeof(Apunte);
            //string cuentaAlias = "cT";
            //string asientoAlias = "asiT";
            //string apunteAlias = "apuT";
            //var conditions = GetCurrentOwnersCondition(tableAlias: cuentaAlias) //Todos los objetos tienen los mismos owners
            //    .Union(GetCurrentOwnersCondition(tableAlias: asientoAlias))
            //    .Union(GetCurrentOwnersCondition(tableAlias: apunteAlias))
            //    .Union(new List<SQLCondition>() { new SQLCondition("Codigo", cuentaAlias, "@cuenta", "") }); //Codigo de la cuenta
            //QueryBuilder qBuilder = new QueryBuilder();
            //qBuilder
            //    .AddSelect(t, cuentaAlias)
            //    .AddSelect(asiType, asientoAlias, new string[] { "asi" })
            //    .AddSelect(apuType, apunteAlias, new string[] { "apu" })
            //    .AddFrom(t, cuentaAlias)
            //    .AddJoin("LEFT", apuType, apunteAlias)
            //    .AddOn(new SQLCondition("IdCuenta", apunteAlias, "Codigo", cuentaAlias))
            //    .AddJoin("LEFT", asiType, asientoAlias)
            //    .AddOn(new SQLCondition("Id", asientoAlias, "Asiento", apunteAlias))
            //    .AddWhere(conditions)
            //    .SemiColon();

            return qBuilder;
        }
        protected override QueryBuilder GetUpdateSQL(int id, aVMTabBase VM)
        {
            if (this._NewObjects[VM].Select(x => x.Id).Contains(id) || //If the object have been newly created it needs an INSERT not an UPDATE
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
        private QueryBuilder GetInsertSQL(CuentaMayor cuenta)
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
            qBuilder.StoreParametersFrom(cuenta);
            return qBuilder;
        }
        #endregion

        #region public methods
        /// <summary>
        /// Can return null after an exception.
        /// </summary>
        /// <param name="numCuenta"></param>
        /// <param name="idCdad"></param>
        /// <param name="idEjer"></param>
        /// <returns></returns>
        public async Task<CuentaMayor> GetByNumCuenta(int numCuenta, int idCdad, int idEjer)
        {
            long cantor = idCdad.CantorPair(idEjer);
            Tuple<int, long> key = new Tuple<int, long>(numCuenta, cantor);
            CuentaMayor cuenta = null;

            if (this._CuentasIds.ContainsKey(key))
            {
                int id = this._CuentasIds[key];
                //Si la cuenta esta en _NewObjects no puede estar disponible hasta que se haga Apply
                //Si la cuenta esta en _ObjectsRemoved no puede estar disponible por que esta esperando para ser borrada
                if (!this._ObjModels.TryGetValue(id, out cuenta))
                    throw new CustomException_Repository(
                        "Se ha tratado de crear una cuenta que ya se está creando.");
            }
            else
            {
                QueryBuilder qBuilder = await Task.Run(() => GetSelectSQLByNumCuenta(numCuenta, idCdad, idEjer)).ConfigureAwait(false);
                dynamic result;

                using (SqlConnection con = new SqlConnection(this._strCon))
                {
                    await con.OpenAsync().ConfigureAwait(false);
                    result = await con.QueryAsync(qBuilder.Query, qBuilder.Parameters).ConfigureAwait(false);
                    con.Close();
                }

                cuenta = await Task.Run(() => this._Mapper.Map(result)).ConfigureAwait(false);

                await this._RepoSphr.WaitAsync();
                //Add object retrieved from DB
                this._OriginalObjModels.TryAdd(cuenta.Id, cuenta);
                //Deep copy
                this._ObjModels.TryAdd(cuenta.Id, await Task.Run(() => this._Mapper.Map(result)).ConfigureAwait(false));

                this._CuentasIds.TryAdd(
                    key,
                    cuenta.Id);

                this._RepoSphr.Release();
            }
            return cuenta;
        }
        public async Task<CuentaMayor> GetById(int id)
        {
            CuentaMayor cuenta;
            if (!this._ObjModels.TryGetValue(id, out cuenta))
            {
                QueryBuilder qBuilder = await Task.Run(() => GetSelectSQL(id)).ConfigureAwait(false);
                dynamic result;
                using (SqlConnection con = new SqlConnection(this._strCon))
                {
                    await con.OpenAsync().ConfigureAwait(false);
                    result = await con.QueryAsync(qBuilder.Query, qBuilder.Parameters).ConfigureAwait(false);
                    con.Close();
                }

                await this._RepoSphr.WaitAsync();
                //Add object retrieved from DB
                this._OriginalObjModels.TryAdd(id, this._Mapper.Map(result));
                //Deep copy
                this._ObjModels.TryAdd(id, this._Mapper.Map(result));

                this._CuentasIds.TryAdd(
                    new Tuple<int, long>(cuenta.NumCuenta, cuenta.IdOwnerComunidad.CantorPair(cuenta.IdOwnerEjercicio)),
                    cuenta.Id);

                this._RepoSphr.Release();
                return cuenta;
            }
            return cuenta;
        }
        public async Task<bool> AddNew(CuentaMayor cuenta, aVMTabBase VM)
        {
            //¡¡¡¡OJO!!!!! AQUI HAY QUE AÑADIR "SELECT LAST_INSERT_ID()" MANUALMENTE AL QUERYBUILDER
            //El problema es que LAST_INSERT_ID() trabaja por conexion, por tanto si se realizan varios inserts no serviría de nada
            if (!this._NewObjects[VM].Add(cuenta)) return false;

            QueryBuilder SQL = await Task.Run(() => GetInsertSQL(cuenta)).ConfigureAwait(false);
            ConditionToCommitScalar<int> condition = new ConditionToCommitScalar<int>(ConditionTCType.greater, -1);
            SQL.Append("SELECT LAST_INSERT_ID();");
            var tuple = new Tuple<QueryBuilder, IConditionToCommit>(SQL, condition);

            await base._RepoSphr.WaitAsync();

            this._CuentasIds.TryAdd(
                    new Tuple<int, long>(cuenta.NumCuenta, cuenta.IdOwnerComunidad.CantorPair(cuenta.IdOwnerEjercicio)),
                    cuenta.Id);

            if (this.Transactions[VM].Contains(tuple)) return false;
            else this.Transactions[VM].Add(tuple);

            base._RepoSphr.Release();
            return true;
        }
        public async Task<bool> Update(CuentaMayor cuenta, aVMTabBase VM)
        {
            QueryBuilder SQL = await Task.Run(() => GetUpdateSQL(cuenta.Id, VM)).ConfigureAwait(false);
            if (SQL == null) return false;

            ConditionToCommitScalar<int> condition = new ConditionToCommitScalar<int>(ConditionTCType.equal, 1);
            var tuple = new Tuple<QueryBuilder, IConditionToCommit>(SQL, condition);

            await base._RepoSphr.WaitAsync();

            if (this.Transactions[VM].Contains(tuple)) return false;
            else this.Transactions[VM].Add(tuple);

            base._RepoSphr.Release();
            return true;
        }
        public async Task<bool> Remove(CuentaMayor cuenta, aVMTabBase VM)
        {
            if (!this._ObjModels.ContainsKey(cuenta.Id)) return false;

            QueryBuilder SQL = await Task.Run(() => GetDeleteSQL(cuenta.Id)).ConfigureAwait(false);
            ConditionToCommitScalar<int> condition = new ConditionToCommitScalar<int>(ConditionTCType.equal, 1);
            var tuple = new Tuple<QueryBuilder, IConditionToCommit>(SQL, condition);

            await base._RepoSphr.WaitAsync();

            if (this.Transactions[VM].Contains(tuple)) return false;
            else this.Transactions[VM].Add(tuple);

            this._ObjectsRemoved[VM].Add(cuenta); //Add object to ObjectsRemoved for later commit or rollback

            CuentaMayor dump;
            this._ObjModels.TryRemove(cuenta.Id, out dump);
            this._OriginalObjModels.TryRemove(cuenta.Id, out dump); //Doesn't matter if this one really had or not the object before, because the object
            //could be added before by the VM, and in that case it would be added only to this._ObjModels
            
            int idump;
            Tuple<int, long> key = new Tuple<int, long>(cuenta.NumCuenta, cuenta.IdOwnerComunidad.CantorPair(cuenta.IdOwnerEjercicio));
            this._CuentasIds.TryRemove(key, out idump);

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