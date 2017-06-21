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
    public sealed class ApunteRepository : aRepositoryBaseWithTwoOwners, IRepositoryInternal, IRepositoryDependent<CuentaMayor>, IRepositoryDependent<Asiento>
    {
        public ApunteRepository()
        {
            MapperStore store = new MapperStore();
            this._Mapper = (DapperMapper<Apunte>)store.GetMapper(GetObjModelType());
        }

        #region fields
        private ConcurrentDictionary<aVMTabBase, HashSet<Apunte>> _NewObjects = new ConcurrentDictionary<aVMTabBase, HashSet<Apunte>>();
        private ConcurrentDictionary<int, Apunte> _ObjModels = new ConcurrentDictionary<int, Apunte>();
        private ConcurrentDictionary<int, Apunte> _OriginalObjModels = new ConcurrentDictionary<int, Apunte>();
        private ConcurrentDictionary<aVMTabBase, HashSet<Apunte>> _ObjectsRemoved = new ConcurrentDictionary<aVMTabBase, HashSet<Apunte>>();
        private DapperMapper<Apunte> _Mapper;
        #endregion

        #region properties
        public ConcurrentDictionary<aVMTabBase, List<Tuple<QueryBuilder, IConditionToCommit>>> Transactions { get; private set; }
        #endregion

        #region helpers
        public override Type GetObjModelType()
        {
            return typeof(Apunte);
        }
        void IRepositoryInternal.NewVM(aVMTabBase VM)
        {
            base._RepoSphr.Wait();
            if (!this.Transactions.ContainsKey(VM)) this.Transactions.TryAdd(VM, new List<Tuple<QueryBuilder, IConditionToCommit>>());
            if (!this._NewObjects.ContainsKey(VM)) this._NewObjects.TryAdd(VM, new HashSet<Apunte>());
            if (!this._ObjectsRemoved.ContainsKey(VM)) this._ObjectsRemoved.TryAdd(VM, new HashSet<Apunte>());
            if (!base._DirtyMembers.ContainsKey(VM)) base._DirtyMembers.TryAdd(VM, new Dictionary<int, string[]>());
            base._RepoSphr.Release();
        }
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        async Task IRepositoryInternal.RemoveVMTabReferences(aVMTabBase VM)
        {
            HashSet<Apunte> setDump;
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
            foreach (Apunte obj in this._NewObjects[VM]) _ObjModels.TryAdd(obj.Id, obj); //All old new objects are now normal objects

            IEnumerable<int> VMIdsToAdd = base._DirtyMembers[VM].Keys.AsEnumerable(); //To delete dependent later
            this._NewObjects[VM].Clear(); //Therefore, clear new objects

            this.Transactions[VM].Clear(); //Transactions made, clear transactions

            IEnumerable<int> VMIdsToRemove = this._ObjectsRemoved[VM].Select(x => x.Id);
            this._ObjectsRemoved[VM].Clear(); //Apply deletes

            base._DirtyMembers[VM].Clear(); //Clear objects members modified
            base._RepoSphr.Release();

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            ((IRepositoryDependent<CuentaMayor>)this).ApplyDependentAsync(VMIdsToAdd, VMIdsToRemove).Forget().ConfigureAwait(false);
            ((IRepositoryDependent<Asiento>)this).ApplyDependentAsync(VMIdsToAdd, VMIdsToRemove).Forget().ConfigureAwait(false);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        }
        async Task IRepositoryInternal.RollbackRepoAsync(aVMTabBase VM)
        {
            await base._RepoSphr.WaitAsync();

            IEnumerable<int> VMIds = base._DirtyMembers[VM].Keys.AsEnumerable(); //To delete dependent later
            foreach (KeyValuePair<int, string[]> kvp in base._DirtyMembers[VM])
                this._ObjModels[kvp.Key] = this._OriginalObjModels[kvp.Key]; //Change back al objects to their original state
            //����OJO!!!! This above only changes dictionary reference, if VM or others had a reference to the object, it will not GCollected AND
            //the owner/s will maintain the reference to the bad, modified, "not-rollbacked" object.

            VMIds = VMIds.Union(this._NewObjects[VM].Select(x => x.Id)); //To delete dependent later
            this._NewObjects[VM].Clear(); //Clear new objects

            this.Transactions[VM].Clear(); //Transactions made, clear transactions

            VMIds = VMIds.Union(this._ObjectsRemoved[VM].Select(x => x.Id));
            this._ObjModels = new ConcurrentDictionary<int, Apunte>(
                this._ObjModels.Union(this._ObjectsRemoved[VM].ToDictionary(x => x.Id, x => x))); //Change back objects removed
            this._OriginalObjModels = new ConcurrentDictionary<int, Apunte>(
                this._OriginalObjModels.Union(this._ObjectsRemoved[VM].ToDictionary(x => x.Id, x => x))); //Change back objects removed
            this._ObjectsRemoved[VM].Clear();

            base._DirtyMembers[VM].Clear(); //Clear objects members modified
            
            base._RepoSphr.Release();

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            ((IRepositoryDependent<CuentaMayor>)this).RollbackDependentAsync(VMIds).Forget().ConfigureAwait(false);
            ((IRepositoryDependent<Asiento>)this).RollbackDependentAsync(VMIds).Forget().ConfigureAwait(false);
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
        public async Task<Apunte> GetById(int id)
        {
            throw new NotImplementedException();
        }
        public async Task<bool> AddNew(Apunte ApunteObj, aVMTabBase VM)
        {
            throw new NotImplementedException();
        }
        public async Task<bool> Update(Apunte ApunteObj, aVMTabBase VM)
        {
            throw new NotImplementedException();
        }
        public async Task<bool> Remove(Apunte ApunteObj, aVMTabBase VM)
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
            var joinCondition = new SQLCondition("Id", "cuenta", "IdCuenta", "apu");
            qBuilder
                //SELECT apu.Apunte FROM apunte apu LEFT JOIN cuentamayor cuenta ON cuenta.Id = apu.IdCuenta 
                //WHERE cuenta.IdOwnerComunidad = @idCdad AND cuenta.IdOwnerEjercicio = @idEjer AND apu.IdCuenta = @codCuenta;
                .AddSelect(t, "apu")
                .AddFrom(t, "apu")
                .AddJoin("INNER", typeof(CuentaMayor), "cuenta")
                .AddOn(joinCondition)
                .AddWhere(ownerConditions)
                .AddCondition("AND ", new SQLCondition("IdCuenta", "apu", "@codCuenta", ""))
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
                .AddSelect(new string[] { "Id" }, "apu")
                .AddFrom(t, "apu")
                .AddJoin("INNER", typeof(CuentaMayor), "cuenta")
                .AddOn(joinCondition)
                .AddWhere(ownerConditions)
                .AddCondition("AND ", new SQLCondition("IdCuenta", "apu", "@codCuenta", ""))
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
            //var joinCondition = new SQLCondition("Id", "cuenta", "IdCuenta", "apu");
            IEnumerable<string> inParams = ids
                .Select(x => 
                    x.ToString().PutAhead("inP"));
            qBuilder
                //SELECT apu.Apunte FROM apunte apu LEFT JOIN cuentamayor cuenta ON cuenta.Id = apu.IdCuenta 
                //WHERE cuenta.IdOwnerComunidad = @idCdad AND cuenta.IdOwnerEjercicio = @idEjer AND apu.IdCuenta = @codCuenta;
                .AddSelect(t, "apu")
                .AddFrom(t, "apu")
                //.AddJoin("INNER", typeof(CuentaMayor), "cuenta")
                //.AddOn(joinCondition)
                //.AddWhere(ownerConditions)
                .AddWhere(new SQLCondition("Id", "apu", inParams))// "@codCuenta", ""))
                .SemiColon();

            qBuilder.StoreParameter("codCuenta", dependentIdCodigo);
            qBuilder.StoreParameter("idCdad", base.CurrentCdadOwner);
            qBuilder.StoreParameter("idEjer", base.CurrentEjerOwner);
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
        public async Task<List<Apunte>> GetTodosApuntesCuenta(CuentaMayor cuenta)
        {
            this.CurrentCdadOwner = cuenta.IdOwnerComunidad;
            this.CurrentEjerOwner = cuenta.IdOwnerEjercicio;

            IEnumerable<int> ids;
            using (SqlConnection con = new SqlConnection(this._strCon))
            {
                await con.OpenAsync().ConfigureAwait(false);
                QueryBuilder qBuilder = ((IRepositoryDependent<CuentaMayor>)this).GetIdsDependentByMasterSelectSQL(cuenta.Id);

                ids = await con.QueryAsync<int>(qBuilder.Query, qBuilder.Parameters).ConfigureAwait(false);
                
                con.Close();
            }

            await base._RepoSphr.WaitAsync();
            //Remove currently existent objects
            if (this._CuentaDependenciesDict.ContainsKey(cuenta)) ids = ids.Except(this._CuentaDependenciesDict[cuenta]);
            base._RepoSphr.Release();

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
            Task.Run(async () =>
            {
                await base._RepoSphr.WaitAsync();

                if (!this._CuentaDependenciesDict.ContainsKey(cuenta)) this._CuentaDependenciesDict.TryAdd(cuenta, ids.ToList());
                else this._CuentaDependenciesDict[cuenta] = this._CuentaDependenciesDict[cuenta].Union(ids).ToList();

                foreach (Apunte ap in final) this._OriginalObjModels.TryAdd(ap.Id, ap);

                List<Apunte> copy = this._Mapper.Map<List<Apunte>>(result);
                foreach (Apunte ap in copy) this._ObjModels.TryAdd(ap.Id, ap);

                base._RepoSphr.Release();
            })
            .Forget().ConfigureAwait(false);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

            return final;
        }
        #endregion
        
        #region dependent Asiento
        private ConcurrentDictionary<Asiento, List<int>> _AsientoDependenciesDict;
        ConcurrentDictionary<Asiento, List<int>> IRepositoryDependent<Asiento>.DependenciesDict => _AsientoDependenciesDict;

        QueryBuilder IRepositoryDependent<Asiento>.GetAllDependentByMasterSelectSQL(int dependentIdCodigo)
        {
            throw new NotImplementedException();
        }

        QueryBuilder IRepositoryDependent<Asiento>.GetIdsDependentByMasterSelectSQL(int dependentIdCodigo)
        {
            throw new NotImplementedException();
        }

        QueryBuilder IRepositoryDependent<Asiento>.GetDependentByMasterSelectSQL(int dependentIdCodigo, IEnumerable<int> ids)
        {
            throw new NotImplementedException();
        }
        Task IRepositoryDependent<Asiento>.RollbackDependentAsync(IEnumerable<int> ids)
        {
            throw new NotImplementedException();
        }

        Task IRepositoryDependent<Asiento>.ApplyDependentAsync(IEnumerable<int> idsToAdd, IEnumerable<int> idsToRemove)
        {
            throw new NotImplementedException();
        }

        public async Task<List<Apunte>> GetTodosApuntesAsiento(int codigoCuenta)
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
