using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using AdConta;
using AdConta.ViewModel;
using AdConta.Models;
using Mapper;
using QBuilder;
using Extensions;

namespace Repository
{
    public sealed class PersonaRepository : aRepositoryInternal<Persona>, IRepositoryCRUD<Persona>, IDisposable
    {
        public PersonaRepository()
        {
            MapperStore store = new MapperStore();
            this._Mapper = (DapperMapper<Persona>)store.GetMapper(typeof(Persona));
            this.Transactions = new ConcurrentDictionary<aVMTabBase, List<Tuple<QueryBuilder, IConditionToCommit>>>();
        }

        #region SQL helpers
        protected override QueryBuilder GetUpdateSQL(int id, aVMTabBase VM)
        {
            if (base._NewObjects[VM].Select(p => p.Id).Contains(id) || //If the object have been newly created it needs an INSERT not an UPDATE
                !base._DirtyMembers[VM].ContainsKey(id)) //If there are no dirty members the object haven't been modified
                return null;

            Type t = GetObjModelType();
            QueryBuilder qBuilder = new QueryBuilder();
            qBuilder
                .Update(t)
                .UpdateSet(this._DirtyMembers[VM][id])
                .Where(new SQLCondition("Id", "@id"));
            qBuilder.StoreParametersFrom(this._ObjModels[id]);
            qBuilder.StoreParameter("id", id);
            return qBuilder;
        }
        private QueryBuilder GetInsertSQL(Persona p)
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
            qBuilder.StoreParametersFrom(p);
            return qBuilder;
        }
        #endregion

        #region public methods
        public async Task<Persona> GetByIdAsync(int id, aVMTabBase VM)
        {
            Persona p = null;
            if(!this._ObjModels.TryGetValue(id, out p))
            {
                Task<QueryBuilder> qBuilder = Task.Run(() => GetSelectSQL(id));
                dynamic result;
                using (SqlConnection con = new SqlConnection(this._strCon))
                {
                    await con.OpenAsync().ConfigureAwait(false);
                    await qBuilder.ConfigureAwait(false);
                    result = await con.QueryAsync(qBuilder.Result.Query, qBuilder.Result.Parameters).ConfigureAwait(false);
                    con.Close();
                }
                p = await Task.Run(() => this._Mapper.Map(result)).ConfigureAwait(false);
                AddToDictionariesObjectRetrievedFromDBAsync(p, Task.Run(() => base._Mapper.Map(result)).Result, VM)
                    .Forget()
                    .ConfigureAwait(false);
            }
            return p;
        }
        public async Task<bool> AddNewAsync(Persona p, aVMTabBase VM)
        {
            if (!this._NewObjects[VM].Add(p)) return false;

            Task<QueryBuilder> SQL = Task.Run(() => GetInsertSQL(p));
            ConditionToCommitScalar<int> condition = new ConditionToCommitScalar<int>(ConditionTCType.equal, 1);
            var tuple = new Tuple<QueryBuilder, IConditionToCommit>(await SQL.ConfigureAwait(false), condition);

            if (!await AddTransactionWaitingForCommitAsync(tuple, VM))
            {
                base._NewObjects[VM].Remove(p);
                return false;
            }
            return true;
        }
        public async Task<bool> UpdateAsync(Persona p, aVMTabBase VM)
        {
            Task<QueryBuilder> SQL = Task.Run(() => GetUpdateSQL(p.Id, VM));
            ConditionToCommitScalar<int> condition = new ConditionToCommitScalar<int>(ConditionTCType.equal, 1);

            if (await SQL.ConfigureAwait(false) == null) return false;            
            var tuple = new Tuple<QueryBuilder, IConditionToCommit>(SQL.Result, condition);
            return await AddTransactionWaitingForCommitAsync(tuple, VM);
        }
        public async Task<bool> RemoveAsync(Persona p, aVMTabBase VM)
        {
            if (!this._ObjModels.ContainsKey(p.Id)) return false;

            Task<QueryBuilder> SQL = Task.Run(() => GetDeleteSQL(p.Id));
            ConditionToCommitScalar<int> condition = new ConditionToCommitScalar<int>(ConditionTCType.equal, 1);
            var tuple = new Tuple<QueryBuilder, IConditionToCommit>(await SQL.ConfigureAwait(false), condition);

            if (!await AddTransactionWaitingForCommitAsync(tuple, VM)) return false;
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            RemoveObjectWaitingForCommitAsync(p, VM).Forget().ConfigureAwait(false);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
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
