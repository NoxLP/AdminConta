using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using System.Transactions;
using AdConta.Models;
using AdConta.ViewModel;
using Extensions;
using QBuilder;
using System.Dynamic;
using Dapper;

namespace Repository
{
    public interface iUnitOfWork
    {
        bool RollbackAllIfRollback { get; set; }
        ConditionsToCommitSQL ConditionsToCommit { get; }
        IEnumerable<dynamic> LastResults { get; }

        Task<bool> CommitAsync(bool storeResults);
        Task RollbackAsync();
        Task RemoveVMTabReferencesFromRepos();
    }

    public class UnitOfWork : iUnitOfWork
    {
        public UnitOfWork(IEnumerable<IRepository> repositories, aVMTabBase tab, bool rollbackAllIfRollback = false)
        {
            this.RollbackAllIfRollback = rollbackAllIfRollback;
            this.Repositories = (HashSet<IRepositoryInternal>)repositories.Select(x=>(IRepositoryInternal)x);
            this._Tab = tab;
            InitRepositories();
        }

        #region fields
        protected readonly string _strCon = GlobalSettings.Properties.Settings.Default.conta1ConnectionString;
        private aVMTabBase _Tab;
        private IDictionary<string,object> _Values;
        private ConditionsToCommitSQL _ConditionsToCommit = new ConditionsToCommitSQL();
        #endregion

        #region properties
        internal HashSet<IRepositoryInternal> Repositories { get; private set; }
        public bool RollbackAllIfRollback { get; set; }
        public ConditionsToCommitSQL ConditionsToCommit { get { return this._ConditionsToCommit; } }
        public IEnumerable<dynamic> LastResults { get; private set; }
        #endregion

        #region helpers
        private void InitRepositories() { Parallel.ForEach(this.Repositories, repo => repo.NewVM(this._Tab)); }
        private string PrepareTransaction()
        {
            string SQL = $"START TRANSACTION;{Environment.NewLine}";
            
            foreach (IRepository repo in this.Repositories)
            {
                List<Tuple<QueryBuilder, IConditionToCommit>> tuples = repo.Transactions[this._Tab];
                foreach(Tuple<QueryBuilder, IConditionToCommit> tuple in tuples)
                {
                    this._Values = this._Values
                        .Union(tuple.Item1 as IDictionary<string, object>)
                        .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
                    tuple.Item1.Append(Environment.NewLine);
                    SQL = SQL.Append(tuple.Item1.Query);
                    this._ConditionsToCommit.Add(tuple.Item2);
                }
            }
            
            return SQL;
        }
        #endregion

        #region public methods
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async Task RemoveVMTabReferencesFromRepos() { Parallel.ForEach(this.Repositories, repo => repo.RemoveVMTabReferences(this._Tab)); }

        public async Task RollbackAsync()
        {
            Parallel.ForEach(this.Repositories, repo => repo.RollbackRepoAsync(this._Tab).Forget().ConfigureAwait(false));
            this._ConditionsToCommit.Clear();
            this._Values.Clear();
            this.LastResults = null;
        }
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously

        public async Task<bool> CommitAsync(bool storeResults)
        {
            string transaction = await Task.Run(() => PrepareTransaction()).ConfigureAwait(false);
            bool commit;

            using (SqlConnection con = new SqlConnection(this._strCon))
            {
                await con.OpenAsync().ConfigureAwait(false);

                var result = await con.QueryMultipleAsync(transaction, this._Values as ExpandoObject).ConfigureAwait(false);
                commit = this._ConditionsToCommit.GetIfMatchAllConditions(result);

                if (!commit)
                    await con.ExecuteAsync("ROLLBACK;").ConfigureAwait(false);
                else
                {
                    if(storeResults) this.LastResults = (IEnumerable<dynamic>)result;
                    await con.ExecuteAsync("COMMIT;").ConfigureAwait(false);
                }

                con.Close();
            }

            if (!commit)
            {
                if(this.RollbackAllIfRollback)
#pragma warning disable CS4014
                    RollbackAsync().Forget().ConfigureAwait(false);
#pragma warning restore CS4014
                return false;
            }
            else
            {
                //using (SqlConnection con = new SqlConnection(this._strCon))
                //{
                //    await con.OpenAsync().ConfigureAwait(false);
                //    await con.ExecuteAsync("COMMIT;").ConfigureAwait(false);
                //    con.Close();
                //}

                Parallel.ForEach(this.Repositories, repo => repo.ApplyChangesAsync(this._Tab).Forget().ConfigureAwait(false));
                this._ConditionsToCommit.Clear();
                this._Values.Clear();
                return true;
            }
        }
        #endregion
    }

}
