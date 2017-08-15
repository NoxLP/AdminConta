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
using Extensions;
using System.Data.SqlClient;
using Dapper;
using AdConta;
using System.Threading;

namespace Repository
{
    public sealed class AsientoRepository : aRepositoryBaseWithTwoOwners<Asiento>, IRepositoryCRUD<Asiento>, IRepositoryDependent<CuentaMayor>, IDisposable
    {
        public AsientoRepository()
        {
            MapperStore store = new MapperStore();
            this._Mapper = (DapperMapper<Asiento>)store.GetMapper(GetObjModelType());
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
            if (base._NewObjects[VM].Select(asiento => asiento.Id).Contains(id) || //If the object have been newly created it needs an INSERT not an UPDATE
                !base._DirtyMembers[VM].ContainsKey(id)) //If there are no dirty members the object haven't been modified
                return null;

            Type t = GetObjModelType();
            QueryBuilder qBuilder = new QueryBuilder();
            if (!base._DirtyMembers[VM][id].Contains("Apuntes"))
            {
                qBuilder
                    .Update(t)
                    .UpdateSet(this._DirtyMembers[VM][id])
                    .Where(new SQLCondition("Id", "@id"));
                qBuilder.StoreParametersFrom(this._ObjModels[id]);
                qBuilder.StoreParameter("id", id);
            }
            else
            {
                Type tApunte = typeof(Apunte);
                qBuilder
                    .Update(t)
                    .UpdateSet(this._DirtyMembers[VM][id])
                    .Where(new SQLCondition("Id", "@id"))
                    .SemiColon()
                    .Append(Environment.NewLine);
                qBuilder.StoreParametersFrom(this._ObjModels[id]);
                qBuilder.StoreParameter("id", id);

                if (base._ObjModels[id].Apuntes.Count > 20)
                {
                    SemaphoreSlim sphr = new SemaphoreSlim(1);
                    int n = 0;
                    Parallel.ForEach(base._ObjModels[id].Apuntes.Where(apunte => apunte.DirtyMembers.Count > 0), apunte =>
                    {
                        sphr.Wait();
                        int i = n++;
                        sphr.Release();
                        qBuilder
                            .Update(tApunte)
                            .UpdateSet(apunte.DirtyMembers)
                            .Where(new SQLCondition("Id", $"@apuid{i}"))
                            .SemiColon()
                            .Append(Environment.NewLine);
                        qBuilder.StoreParametersFrom(apunte, apunte.DirtyMembers, i.ToString());
                        qBuilder.StoreParameter($"apuid{i}", apunte.Id);
                    });
                }
                else
                {
                    int i = 0;
                    foreach(Apunte apunte in this._ObjModels[id].Apuntes.Where(apunte => apunte.DirtyMembers.Count > 0))
                    {
                        qBuilder
                            .Update(tApunte)
                            .UpdateSet(apunte.DirtyMembers)
                            .Where(new SQLCondition("Id", $"@apuid{i}"))
                            .SemiColon()
                            .Append(Environment.NewLine);
                        qBuilder.StoreParametersFrom(apunte, apunte.DirtyMembers, i.ToString());
                        qBuilder.StoreParameter($"apuid{i}", apunte.Id);
                        i++;
                    }
                }
            }
            return qBuilder;
        }
        private QueryBuilder GetInsertSQL(Asiento asiento)
        {
            throw new NotImplementedException();
        }
        protected override QueryBuilder GetDeleteSQL(int id)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region public methods
        public Asiento AsientoMapperGetFromDictionary(int id)
        {
            Asiento asiento;
            if (!base._ObjModels.TryGetValue(id, out asiento)) return null;

            return asiento;
        }
        public async Task<Asiento> GetByIdAsync(int id, aVMTabBase VM)
        {
            throw new NotImplementedException();
        }
        public async Task<bool> AddNewAsync(Asiento AsientoObj, aVMTabBase VM)
        {
            throw new NotImplementedException();
        }
        public async Task<bool> UpdateAsync(Asiento AsientoObj, aVMTabBase VM)
        {
            QueryBuilder SQL = await Task.Run(() => GetUpdateSQL(AsientoObj.Id, VM)).ConfigureAwait(false);
            if (SQL == null) return false;

            ConditionToCommitScalar<int> condition = new ConditionToCommitScalar<int>(ConditionTCType.equal, 
                SQL.CountNumberOfOcurrencesInQuery("UPDATE"));
            var tuple = new Tuple<QueryBuilder, IConditionToCommit>(SQL, condition);

            return await AddTransactionWaitingForCommitAsync(tuple, VM);
        }
        public async Task<bool> RemoveAsync(Asiento AsientoObj, aVMTabBase VM)
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
            Type apunte = typeof(Apunte);
            Type cuenta = typeof(CuentaMayor);
            QueryBuilder qBuilder = new QueryBuilder();
            var ownerConditions = GetCurrentOwnersCondition(tableAlias: "cuenta");
            var joinApunteCuentaCondition = new SQLCondition("Id", "cuenta", "IdCuenta", "apu");
            var joinApunteCondition = new SQLCondition("Asiento", "apu", "Id", "asi");
            var owners = new string[2] { "IdOwnerComunidad", "IdOwnerEjercicio" };
            //SELECT asi.asiasiento, apu.apuapunte, cuenta.owners FROM asiento asi 
            //INNER JOIN apunte apu ON apu.Asiento = asi.Id -> joinApuntecondition
            //INNER JOIN cuentamayor cuenta ON cuenta.Id = apu.IdCuenta -> joinApunteCuentacondition
            //WHERE cuenta.IdOwnerComunidad = @idCdad AND cuenta.IdOwnerEjercicio = @idEjer AND apu.IdCuenta = @codCuenta;
            qBuilder
                .Select(t, "asi", "asi")
                .SelectColumns(apunte, "apu", "apu")
                .SelectColumns(owners, "cuenta")
                .From(t, "asi")
                .Join("INNER", apunte, "apu")
                .On(joinApunteCondition)
                .Join("INNER", cuenta, "cuenta")
                .On(joinApunteCuentaCondition)
                .Where(ownerConditions)
                .Condition("AND", new SQLCondition("IdCuenta", "apu", "@codCuenta", ""))
                .OrderBy(new string[1] { "Fecha" }, "asi")
                .OrderBy(new string[1] { "OrdenEnAsiento" }, "apu")
                .SemiColon();
            base.StoreCurrentOwnerConditionsParameters(qBuilder);
            qBuilder.StoreParameter("codCuenta", dependentIdCodigo);

            return qBuilder;
        }
        QueryBuilder IRepositoryDependent<CuentaMayor>.GetIdsDependentByMasterSelectSQL(int dependentIdCodigo)
        {
            Type t = GetObjModelType();
            Type apunte = typeof(Apunte);
            Type cuenta = typeof(CuentaMayor);
            QueryBuilder qBuilder = new QueryBuilder();
            var ownerConditions = GetCurrentOwnersCondition(tableAlias: "cuenta");
            var joinApunteCuentaCondition = new SQLCondition("Id", "cuenta", "IdCuenta", "apu");
            var joinApunteCondition = new SQLCondition("Asiento", "apu", "Id", "asi");
            var owners = new string[2] { "IdOwnerComunidad", "IdOwnerEjercicio" };
            //SELECT asi.Id FROM asiento asi 
            //INNER JOIN apunte apu ON apu.Asiento = asi.Id -> joinApuntecondition
            //INNER JOIN cuentamayor cuenta ON cuenta.Id = apu.IdCuenta -> joinApunteCuentacondition
            //WHERE cuenta.IdOwnerComunidad = @idCdad AND cuenta.IdOwnerEjercicio = @idEjer AND apu.IdCuenta = @codCuenta;
            qBuilder
                .Select(new string[] { "Id" }, "asi")
                .From(t, "asi")
                .Join("INNER", apunte, "apu")
                .On(joinApunteCondition)
                .Join("INNER", cuenta, "cuenta")
                .On(joinApunteCuentaCondition)
                .Where(ownerConditions)
                .Condition("AND", new SQLCondition("IdCuenta", "apu", "@codCuenta", ""))
                .SemiColon();
            base.StoreCurrentOwnerConditionsParameters(qBuilder);
            qBuilder.StoreParameter("codCuenta", dependentIdCodigo);

            return qBuilder;
        }
        QueryBuilder IRepositoryDependent<CuentaMayor>.GetDependentByMasterSelectSQL(int dependentIdCodigo, IEnumerable<int> ids)
        {
            Type t = GetObjModelType();
            Type apunte = typeof(Apunte);
            QueryBuilder qBuilder = new QueryBuilder();
            var joinApunteCondition = new SQLCondition("Asiento", "apu", "Id", "asi");
            IEnumerable<string> inParams = ids
                .Select(x =>
                    x.ToString().PutAhead("inP"));
            //SELECT asi.asiento, apu.apunte FROM asiento asi 
            //INNER JOIN apunte apu ON apu.Asiento = asi.Id -> joinApuntecondition
            //WHERE asi.Id IN(ids)
            qBuilder
                .Select(t, "asi")
                .SelectColumns(apunte, "apu")
                .Join("INNER", apunte, "apu")
                .On(joinApunteCondition)
                .Where(new SQLCondition("Id", "asi", inParams))
                .OrderBy(new string[1] { "Fecha" }, "asi")
                .OrderBy(new string[1] { "OrdenEnAsiento" }, "apu")
                .SemiColon();
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
                    .Where(id => this._ObjModels[id].Apuntes
                        .Any(apunte => apunte.Cuenta.Equals(kvp.Key))); //Those who share the same CuentaMayor
                    //.Except(kvp.Value); //Remove those that are already in the dictionary => not necessary, Union is Distinct
                IEnumerable<int> toRemoveFromThisCuenta = idsToRemove
                    .Where(id => this._ObjModels[id].Apuntes
                        .Any(apunte => apunte.Cuenta.Equals(kvp.Key))) //Those who share the same CuentaMayor
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
        
        public async Task<List<Asiento>> GetTodosAsientosCuentaAsync(CuentaMayor cuenta)
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

            List<Asiento> final = await Task.Run(() => this._Mapper.Map<List<Asiento>>(result)).ConfigureAwait(false);

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
            Task.Run(async () =>
            {
                if (!this._CuentaDependenciesDict.ContainsKey(cuenta)) this._CuentaDependenciesDict.TryAdd(cuenta, ids.ToList());
                else this._CuentaDependenciesDict[cuenta] = this._CuentaDependenciesDict[cuenta].Union(ids).ToList();

                foreach (Asiento asi in final) this._OriginalObjModels.TryAdd(asi.Id, asi);

                List<Asiento> copy = this._Mapper.Map<List<Asiento>>(result);
                foreach (Asiento asi in copy) this._ObjModels.TryAdd(asi.Id, asi);
            })
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
            .Forget().ConfigureAwait(false);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

            base._RepoSphr.Release();

            return final;
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
