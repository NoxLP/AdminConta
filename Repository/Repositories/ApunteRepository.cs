using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AdConta.ViewModel;
using QBuilder;
using Mapper;
using ModuloContabilidad.ObjModels;
using System.Data.SqlClient;
using Dapper;
using Extensions;

namespace Repository
{
    public sealed class ApunteRepository : 
        aRepositoryBaseWithTwoOwners<Apunte>,
        IRepositoryDependent<CuentaMayor>,
        IRepositoryCRUD<Apunte>
    {
        public ApunteRepository()
        {
            MapperStore store = new MapperStore();
            this._Mapper = (DapperMapper<Apunte>)store.GetMapper(GetObjModelType());
            this.Transactions = new ConcurrentDictionary<aVMTabBase, List<Tuple<QueryBuilder, IConditionToCommit>>>();
        }
        
        #region helpers
        internal override async Task ApplyChangesAsync(aVMTabBase VM, Func<Task> doFirstInsideSemaphoreWaiting = null, Func<Task> doLastInsideSemaphoreWaiting = null)
        {
            int[] VMIdsToAdd = base._DirtyMembers[VM].Keys.ToArray(); //To delete dependent later
            int[] VMIdsToRemove = this._ObjectsRemoved[VM].Select(x => x.Id).ToArray();

            await base.ApplyChangesAsync(VM);

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            ((IRepositoryDependent<CuentaMayor>)this).ApplyDependentAsync(VMIdsToAdd, VMIdsToRemove).Forget().ConfigureAwait(false);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        }
        internal override async Task RollbackRepoAsync(aVMTabBase VM, Func<Task> doFirstInsideSemaphoreWaiting = null, Func<Task> doLastInsideSemaphoreWaiting = null)
        {
            IEnumerable<int> VMIds = base._DirtyMembers[VM].Keys.Union(this._NewObjects[VM].Select(x => x.Id)).AsEnumerable(); //To delete dependent later

            await base.RollbackRepoAsync(VM);            

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            ((IRepositoryDependent<CuentaMayor>)this).RollbackDependentAsync(VMIds).Forget().ConfigureAwait(false);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
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
        private QueryBuilder GetInsertSQL(Apunte cuenta)
        {
            throw new NotImplementedException();
        }
        protected override QueryBuilder GetDeleteSQL(int id)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region public methods
        public async Task<Apunte> GetByIdAsync(int id, aVMTabBase VM)
        {
            throw new NotImplementedException();
        }
        public async Task<bool> AddNewAsync(Apunte ApunteObj, aVMTabBase VM)
        {
            throw new NotImplementedException();
        }
        public async Task<bool> UpdateAsync(Apunte ApunteObj, aVMTabBase VM)
        {
            throw new NotImplementedException();
        }
        public async Task<bool> RemoveAsync(Apunte ApunteObj, aVMTabBase VM)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region dependent CuentaMayor
        private ConcurrentDictionary<CuentaMayor, List<int>> _CuentaDependenciesDict;
        ConcurrentDictionary<CuentaMayor, List<int>> IRepositoryDependent<CuentaMayor>.DependenciesDict => _CuentaDependenciesDict;

        QueryBuilder IRepositoryDependent<CuentaMayor>.GetAllDependentByMasterSelectSQL(int dependentIdCodigo)
        {
            Type t = GetObjModelType();
            QueryBuilder qBuilder = new QueryBuilder();
            var ownerConditions = GetCurrentOwnersCondition(tableAlias: "cuenta");
            var joinCuentaCondition = new SQLCondition("Id", "cuenta", "IdCuenta", "apu");
            var joinAsientoCondition = new SQLCondition("Id", "asi", "Asiento", "apu");
            var owners = new string[2] { "IdOwnerComunidad", "IdOwnerEjercicio" };
            qBuilder
                //SELECT apu.Apunte, cuenta.owners FROM apunte apu INNER JOIN cuentamayor cuenta ON cuenta.Id = apu.IdCuenta 
                //WHERE cuenta.IdOwnerComunidad = @idCdad AND cuenta.IdOwnerEjercicio = @idEjer AND apu.IdCuenta = @codCuenta;
                .Select(t, "apu")
                .SelectColumns(owners, "cuenta")
                .From(t, "apu")
                .Join("INNER", typeof(CuentaMayor), "cuenta")
                .On(joinCuentaCondition)
                .Where(ownerConditions)
                .Condition("AND ", new SQLCondition("IdCuenta", "apu", "@codCuenta", ""))
                .SemiColon();

            qBuilder.StoreParameter("codCuenta", dependentIdCodigo);
            qBuilder.StoreParameter("idCdad", base.CurrentCdadOwner);
            qBuilder.StoreParameter("idEjer", base.CurrentEjerOwner);

            return qBuilder;
        }
        QueryBuilder IRepositoryDependent<CuentaMayor>.GetIdsDependentByMasterSelectSQL(int dependentIdCodigo)
        {
            Type t = GetObjModelType();
            QueryBuilder qBuilder = new QueryBuilder();
            var ownerConditions = GetCurrentOwnersCondition(tableAlias: "cuenta");
            var joinCondition = new SQLCondition("Id", "cuenta", "IdCuenta", "apu");
            qBuilder
                //SELECT apu.Apunte FROM apunte apu LEFT JOIN cuentamayor cuenta ON cuenta.Id = apu.IdCuenta 
                //WHERE cuenta.IdOwnerComunidad = @idCdad AND cuenta.IdOwnerEjercicio = @idEjer AND apu.IdCuenta = @codCuenta;
                .Select(new string[] { "Id" }, "apu")
                .From(t, "apu")
                .Join("INNER", typeof(CuentaMayor), "cuenta")
                .On(joinCondition)
                .Where(ownerConditions)
                .Condition("AND ", new SQLCondition("IdCuenta", "apu", "@codCuenta", ""))
                .SemiColon();

            qBuilder.StoreParameter("codCuenta", dependentIdCodigo);
            qBuilder.StoreParameter("idCdad", base.CurrentCdadOwner);
            qBuilder.StoreParameter("idEjer", base.CurrentEjerOwner);

            return qBuilder;
        }
        QueryBuilder IRepositoryDependent<CuentaMayor>.GetDependentByMasterSelectSQL(int dependentIdCodigo, IEnumerable<int> ids)
        {
            Type t = GetObjModelType();
            QueryBuilder qBuilder = new QueryBuilder();
            //var ownerConditions = GetCurrentOwnersCondition(tableAlias: "cuenta");
            var joinCondition = new SQLCondition("Id", "cuenta", "IdCuenta", "apu");
            var owners = new string[2] { "IdOwnerComunidad", "IdOwnerEjercicio" };
            IEnumerable<string> inParams = ids
                .Select(x => 
                    x.ToString().PutAhead("inP"));
            qBuilder
                //SELECT apu.Apunte FROM apunte apu WHERE apu.Id IN(ids);
                .Select(t, "apu")
                .SelectColumns(owners, "cuenta")
                .From(t, "apu")
                //.Join("INNER", typeof(CuentaMayor), "cuenta")
                //.On(joinCondition)
                //.AddWhere(ownerConditions)
                .Where(new SQLCondition("Id", "apu", inParams))// "@codCuenta", ""))
                .SemiColon();

            //qBuilder.StoreParameter("codCuenta", dependentIdCodigo);
            //qBuilder.StoreParameter("idCdad", base.CurrentCdadOwner);
            //qBuilder.StoreParameter("idEjer", base.CurrentEjerOwner);
            qBuilder.StoreParameters((IEnumerable<object>)ids, "inP");

            return qBuilder;
        }
        async Task IRepositoryDependent<CuentaMayor>.RollbackDependentAsync(IEnumerable<int> ids)
        {
            await base._RepoSphr.WaitAsync();
            Parallel.ForEach(this._CuentaDependenciesDict, kvp =>
            //foreach(KeyValuePair<CuentaMayor,List<int>> kvp in this._CuentaDependenciesDict)
            {
                var coincidences = kvp.Value.Intersect(ids);

                if (coincidences.Count() > 0)
                {
                    if (coincidences.Count() == kvp.Value.Count)
                    {
                        List<int> dump;
                        this._CuentaDependenciesDict.TryRemove(kvp.Key, out dump);
                    }
                    else this._CuentaDependenciesDict[kvp.Key] = kvp.Value.Except(coincidences).ToList();
                }
            });
            base._RepoSphr.Release();
        }
        async Task IRepositoryDependent<CuentaMayor>.ApplyDependentAsync(IEnumerable<int> idsToAdd, IEnumerable<int> idsToRemove)
        {
            await base._RepoSphr.WaitAsync();
            Parallel.ForEach(this._CuentaDependenciesDict, kvp =>
            {
                IEnumerable<int> toAddToThisCuenta = idsToAdd
                    .Where(id => this._ObjModels[id].Cuenta.Equals(kvp.Key)); //Those who share the same CuentaMayor
                    //.Except(kvp.Value); //Remove those that are already in the dictionary => not necessary, Union is Distinct
                IEnumerable<int> toRemoveFromThisCuenta = idsToRemove
                    .Where(id => this._ObjModels[id].Cuenta.Equals(kvp.Key)) //Those who share the same CuentaMayor
                    .Intersect(kvp.Value); //Those who exists currently in the dictionary

                if (toAddToThisCuenta.Count() > 0)
                    this._CuentaDependenciesDict[kvp.Key] = this._CuentaDependenciesDict[kvp.Key].Union(toAddToThisCuenta).ToList();

                if (toRemoveFromThisCuenta.Count() > 0)
                {
                    if (toRemoveFromThisCuenta.Count() == kvp.Value.Count)
                    {
                        List<int> dump;
                        this._CuentaDependenciesDict.TryRemove(kvp.Key, out dump);
                    }
                    else this._CuentaDependenciesDict[kvp.Key] = kvp.Value.Except(toRemoveFromThisCuenta).ToList();
                }
            });
            base._RepoSphr.Release();
        }
        public async Task<List<Apunte>> GetTodosApuntesCuentaAsync(CuentaMayor cuenta)
        {
            await base._RepoSphr.WaitAsync();
            base.CurrentCdadOwner = cuenta.IdOwnerComunidad;
            base.CurrentEjerOwner = cuenta.IdOwnerEjercicio;

            IEnumerable<int> ids;
            using (SqlConnection con = new SqlConnection(this._strCon))
            {
                await con.OpenAsync().ConfigureAwait(false);
                QueryBuilder qBuilder = ((IRepositoryDependent<CuentaMayor>)this).GetIdsDependentByMasterSelectSQL(cuenta.Id);

                ids = await con.QueryAsync<int>(qBuilder.Query, qBuilder.Parameters).ConfigureAwait(false);
                
                con.Close();
            }
            
            //Remove currently existent objects
            if (this._CuentaDependenciesDict.ContainsKey(cuenta))
            {
                //Más asientos podrían haberse añadido a la misma cuenta
                bool idsAreTheSameThanThoseInDictionary = ids
                    .OrderBy(x => x)
                    .SequenceEqual(
                        this._CuentaDependenciesDict[cuenta]
                        .OrderBy(x => x));
                //Si no se han añadido más asientos, devuelve los que están ya en el diccionario
                if (idsAreTheSameThanThoseInDictionary)
                {
                    base._RepoSphr.Release();
                    return this._ObjModels
                        .Where(kvp => ids.Contains(kvp.Key))
                        .Select(kvp => kvp.Value)
                        .ToList();
                }
                //De otra manera coge solo los asientos nuevos que NO están en el diccionario
                ids = ids.Except(this._CuentaDependenciesDict[cuenta]);
            }

            IEnumerable<dynamic> result;
            using (SqlConnection con = new SqlConnection(this._strCon))
            {
                await con.OpenAsync().ConfigureAwait(false);
                QueryBuilder qBuilder = ((IRepositoryDependent<CuentaMayor>)this).GetDependentByMasterSelectSQL(cuenta.Id, ids);

                result = await con.QueryAsync(qBuilder.Query, qBuilder.Parameters).ConfigureAwait(false);

                con.Close();
            }

            List<Apunte> final = await Task.Run(() => this._Mapper.Map<List<Apunte>>(result)).ConfigureAwait(false);

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
            Task.Run(async () =>
            {
                if (!this._CuentaDependenciesDict.ContainsKey(cuenta)) this._CuentaDependenciesDict.TryAdd(cuenta, ids.ToList());
                else this._CuentaDependenciesDict[cuenta] = this._CuentaDependenciesDict[cuenta].Union(ids).ToList();

                foreach (Apunte ap in final) this._OriginalObjModels.TryAdd(ap.Id, ap);

                List<Apunte> copy = this._Mapper.Map<List<Apunte>>(result);
                foreach (Apunte ap in copy) this._ObjModels.TryAdd(ap.Id, ap);
            })
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
            .Forget().ConfigureAwait(false);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

            base._RepoSphr.Release();

            return final;
        }
        public async Task<IEnumerable<int>> GetIdsApuntesCuentaAsync(CuentaMayor cuenta)
        {
            bool sphrWaiting = true; //The semaphore will be relased in the using block, if it doesn't get released, the method won't wait for it again after the using
            await base._RepoSphr.WaitAsync();
            base.CurrentCdadOwner = cuenta.IdOwnerComunidad;
            base.CurrentEjerOwner = cuenta.IdOwnerEjercicio;

            IEnumerable<int> ids;
            using (SqlConnection con = new SqlConnection(this._strCon))
            {
                await con.OpenAsync().ConfigureAwait(false);
                QueryBuilder qBuilder = ((IRepositoryDependent<CuentaMayor>)this).GetIdsDependentByMasterSelectSQL(cuenta.Id);
                base._RepoSphr.Release();

                ids = await con.QueryAsync<int>(qBuilder.Query, qBuilder.Parameters).ConfigureAwait(false);

                con.Close();
            }

            if (!sphrWaiting)
                base._RepoSphr.Release();

            return ids;
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
