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
    public sealed class CuentaMayorRepository : aRepositoryBaseWithTwoOwners<CuentaMayor>, IRepositoryCRUD<CuentaMayor>
    {
        public CuentaMayorRepository()
        {
            MapperStore store = new MapperStore();
            this._Mapper = (DapperMapper<CuentaMayor>)store.GetMapper(GetObjModelType());
            base.Transactions = new ConcurrentDictionary<aVMTabBase, List<Tuple<QueryBuilder, IConditionToCommit>>>();
        }

        #region fields
        /// <summary>
        /// Relations between owners, numCuenta and Id. Used to manage objects by numCuenta instead of by Id.
        /// </summary>
        private ConcurrentDictionary<Tuple<int, long>, int> _CuentasIds = new ConcurrentDictionary<Tuple<int, long>, int>();
        #endregion

        #region internal override
        internal override async Task ApplyChangesAsync(aVMTabBase VM, Func<Task> doFirstInsideSemaphoreWaiting = null, Func<Task> doLastInsideSemaphoreWaiting = null)
        {
            await base._RepoSphr.WaitAsync();

            //All old new objects are now normal objects
            foreach (CuentaMayor objModel in base._NewObjects[VM])
            {
                base._ObjModels.TryAdd(objModel.Id, objModel);
                base._VMsThatOwnsObjModels.AddOrUpdate(
                    objModel.Id,
                    new HashSet<aVMTabBase>() { VM },
                    (id, hashset) => new HashSet<aVMTabBase>(base._VMsThatOwnsObjModels[id].AddAndGetUpdatedHashSet(VM)));
                Tuple<int, long> key = new Tuple<int, long>(objModel.NumCuenta, objModel.IdOwnerComunidad.CantorPair(objModel.IdOwnerEjercicio));
                this._CuentasIds.TryAdd(key, objModel.Id);
            }
            base._NewObjects[VM].Clear(); //Therefore, clear new objects

            base.Transactions[VM].Clear(); //Transactions made, clear transactions

            //Apply deletes
            CuentaMayor dump;
            HashSet<aVMTabBase> hDump;
            int iDump;
            foreach (CuentaMayor objModel in base._ObjectsRemoved[VM])
            {
                base._OriginalObjModels.TryRemove(objModel.Id, out dump);
                this._VMsThatOwnsObjModels.TryRemove(objModel.Id, out hDump);
                Tuple<int, long> key = new Tuple<int, long>(objModel.NumCuenta, objModel.IdOwnerComunidad.CantorPair(objModel.IdOwnerEjercicio));
                this._CuentasIds.TryRemove(key, out iDump);
            }
            base._ObjectsRemoved[VM].Clear();

            //Apply members modified to original objects
            foreach (KeyValuePair<int, string[]> kvp in base._DirtyMembers[VM])
                base._OriginalObjModels[kvp.Key] = base._ObjModels[kvp.Key];
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
                .Select(t)
                .From(t)
                .Where(new SQLCondition("Codigo", "@codigo"))
                .Conditions("AND ", ownerConditions)
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
                .Update(t)
                .UpdateSet(this._DirtyMembers[VM][id])
                .Where(new SQLCondition("Id", "@id"));
            qBuilder.StoreParametersFrom(this._ObjModels[id]);
            qBuilder.StoreParameter("@id", id);
            return qBuilder;
        }
        private QueryBuilder GetInsertSQL(CuentaMayor cuenta)
        {
            Type t = GetObjModelType();
            QueryBuilder qBuilder = new QueryBuilder();
            qBuilder
                .InsertInto()
                .InsertFirstColumns(t)
                .CloseBrackets()
                .InsertValues()
                .InsertValues(t)
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
        public async Task<CuentaMayor> GetByNumCuentaAsync(int numCuenta, int idCdad, int idEjer, aVMTabBase VM)
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

                cuenta = await Task.Run(() => base._Mapper.Map(result)).ConfigureAwait(false);
                AddToDictionariesObjectRetrievedFromDBAsync(
                    cuenta,
                    await Task.Run(() => base._Mapper.Map(result)).ConfigureAwait(false),
                    VM)
                    .Forget()
                    .ConfigureAwait(false);

                await this._RepoSphr.WaitAsync();
                this._CuentasIds.TryAdd(key, cuenta.Id);
                this._RepoSphr.Release();
            }
            return cuenta;
        }
        public async Task<CuentaMayor> GetByIdAsync(int id, aVMTabBase VM)
        {
            CuentaMayor cuenta;
            if (!base._ObjModels.TryGetValue(id, out cuenta))
            {
                QueryBuilder qBuilder = await Task.Run(() => GetSelectSQL(id)).ConfigureAwait(false);
                dynamic result;
                using (SqlConnection con = new SqlConnection(this._strCon))
                {
                    await con.OpenAsync().ConfigureAwait(false);
                    result = await con.QueryAsync(qBuilder.Query, qBuilder.Parameters).ConfigureAwait(false);
                    con.Close();
                }

                cuenta = await Task.Run(() => base._Mapper.Map(result)).ConfigureAwait(false);
                AddToDictionariesObjectRetrievedFromDBAsync(
                    cuenta,
                    await Task.Run(() => base._Mapper.Map(result)).ConfigureAwait(false),
                    VM)
                    .Forget()
                    .ConfigureAwait(false);

                await base._RepoSphr.WaitAsync();
                this._CuentasIds.TryAdd(
                    new Tuple<int, long>(cuenta.NumCuenta, cuenta.IdOwnerComunidad.CantorPair(cuenta.IdOwnerEjercicio)),
                    cuenta.Id);
                base._RepoSphr.Release();
            }
            return cuenta;
        }
        public async Task<bool> AddNewAsync(CuentaMayor cuenta, aVMTabBase VM)
        {
            //¡¡¡¡OJO!!!!! AQUI HAY QUE AÑADIR "SELECT LAST_INSERT_ID()" MANUALMENTE AL QUERYBUILDER
            //El problema es que LAST_INSERT_ID() trabaja por conexion, por tanto si se realizan varios inserts no serviría de nada
            if (!base._NewObjects[VM].Add(cuenta)) return false;

            QueryBuilder SQL = await Task.Run(() => GetInsertSQL(cuenta)).ConfigureAwait(false);
            ConditionToCommitScalar<int> condition = new ConditionToCommitScalar<int>(ConditionTCType.greater, -1);
            SQL.Append("SELECT LAST_INSERT_ID();");
            var tuple = new Tuple<QueryBuilder, IConditionToCommit>(SQL, condition);

            await base._RepoSphr.WaitAsync();
            this._CuentasIds.TryAdd(
                    new Tuple<int, long>(cuenta.NumCuenta, cuenta.IdOwnerComunidad.CantorPair(cuenta.IdOwnerEjercicio)),
                    cuenta.Id);
            base._RepoSphr.Release();

            if (!await AddTransactionWaitingForCommitAsync(tuple, VM))
            {
                base._NewObjects[VM].Remove(cuenta);
                return false;
            }
            return true;
        }
        public async Task<bool> UpdateAsync(CuentaMayor cuenta, aVMTabBase VM)
        {
            QueryBuilder SQL = await Task.Run(() => GetUpdateSQL(cuenta.Id, VM)).ConfigureAwait(false);
            if (SQL == null) return false;

            ConditionToCommitScalar<int> condition = new ConditionToCommitScalar<int>(ConditionTCType.equal, 1);
            var tuple = new Tuple<QueryBuilder, IConditionToCommit>(SQL, condition);
            
            return await AddTransactionWaitingForCommitAsync(tuple, VM);
        }
        public async Task<bool> RemoveAsync(CuentaMayor cuenta, aVMTabBase VM)
        {
            if (!this._ObjModels.ContainsKey(cuenta.Id)) return false;

            QueryBuilder SQL = await Task.Run(() => GetDeleteSQL(cuenta.Id)).ConfigureAwait(false);
            ConditionToCommitScalar<int> condition = new ConditionToCommitScalar<int>(ConditionTCType.equal, 1);
            var tuple = new Tuple<QueryBuilder, IConditionToCommit>(SQL, condition);

            if (!await AddTransactionWaitingForCommitAsync(tuple, VM)) return false;
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            RemoveObjectWaitingForCommitAsync(cuenta, VM).Forget().ConfigureAwait(false);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

            await base._RepoSphr.WaitAsync();

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