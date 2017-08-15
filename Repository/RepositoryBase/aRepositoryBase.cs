using AdConta.ViewModel;
using MQBStatic;
using QBuilder;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Repository
{
    public abstract class aRepositoryBase : MQBStatic_QBuilder, IDisposable
    {
        #region fields
        protected SemaphoreSlim _RepoSphr = new SemaphoreSlim(1, 1);
        protected readonly string _strCon = GlobalSettings.Properties.Settings.Default.conta1ConnectionString;
        
        protected HashSet<int> _Modified;
        protected ConcurrentDictionary<IViewModelBase, Dictionary<int, string[]>> _DirtyMembers = new ConcurrentDictionary<IViewModelBase, Dictionary<int, string[]>>();
        #endregion

        #region helpers
        public abstract Type GetObjModelType();
        #endregion

        #region SQL helpers
        protected virtual QueryBuilder GetSelectSQL(int id)
        {
            Type t = GetObjModelType();
            QueryBuilder qBuilder = new QueryBuilder();
            qBuilder
                .Select(t)
                .From(t)
                .Where(new SQLCondition("Id", "@id"));
            qBuilder.StoreParameter("id", id);
            return qBuilder;
        }
        protected virtual QueryBuilder GetUpdateSQL(int id, aVMTabBase VM) { throw new NotImplementedException(); }
        protected virtual QueryBuilder GetDeleteSQL(int id)
        {
            Type t = GetObjModelType();
            QueryBuilder qBuilder = new QueryBuilder();
            qBuilder
                .DeleteFrom(t)
                .Where(new SQLCondition("Id", "@id"));
            qBuilder.StoreParameter("id", id);
            return qBuilder;
        }
        #endregion

        #region public methods
        public bool GetHasBeenModifiedByThisUser(int id)
        {
            return this._Modified.Contains(id);
        }
        public void SetHasBeenModifiedByThisUser(int id)
        {
            this._Modified.Add(id);
        }
        public async Task<bool> TrySetDirtyMember(IViewModelBase VM, int id, string name)
        {
            if (!GetHasBeenModifiedByThisUser(id)) return false;

            await this._RepoSphr.WaitAsync();

            if (!this._DirtyMembers[VM].ContainsKey(id)) this._DirtyMembers[VM].Add(id, new string[] { name });
            else this._DirtyMembers[VM][id] = this._DirtyMembers[VM][id].Union(new string[1] { name }).ToArray();

            this._RepoSphr.Release();
            return true;
        }
        #endregion

        #region Dispose
        public virtual void Dispose()
        {
            if (this._RepoSphr != null)
            {
                this._RepoSphr.Dispose();
                this._RepoSphr = null;
            }
        }
        #endregion
    }
}
