using AdConta.ViewModel;
using MQBStatic;
using QBuilder;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
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
        protected ConcurrentDictionary<aVMTabBase, Dictionary<int, string[]>> _DirtyMembers = new ConcurrentDictionary<aVMTabBase, Dictionary<int, string[]>>();
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
                .AddSelect(t)
                .AddFrom(t)
                .AddWhere(new SQLCondition("Id", "@id"));
            qBuilder.StoreParameter("id", id);
            return qBuilder;
        }
        protected virtual QueryBuilder GetUpdateSQL(int id, aVMTabBase VM) { throw new NotImplementedException(); }
        protected virtual QueryBuilder GetDeleteSQL(int id)
        {
            Type t = GetObjModelType();
            QueryBuilder qBuilder = new QueryBuilder();
            qBuilder
                .AddDeleteFrom(t)
                .AddWhere(new SQLCondition("Id", "@id"));
            qBuilder.StoreParameter("id", id);
            return qBuilder;
        }
        #endregion

        #region public methods
        public bool GetIsBeenModifiedByThisUser(int id)
        {
            return this._Modified.Contains(id);
        }
        public void SetIsBeenModifiedByThisUser(int id)
        {
            this._Modified.Add(id);
        }
        public async Task<bool> TrySetDirtyMember(aVMTabBase VM, int id, string name)
        {
            if (!GetIsBeenModifiedByThisUser(id)) return false;

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

    public abstract class aRepositoryBaseWithOneOwner : aRepositoryBase, IRepositoryOwnerCdad
    {
        public int CurrentSingleOwner { get; protected set; }

        public SQLCondition GetCurrentOwnerCondition(string condition = "=", string alias = "", string paramAlias = "")
        {
            return new SQLCondition("IdOwnerComunidad", alias, $"@{paramAlias}idCdad", "", condition, "");
        }

        #region Dispose
        public override void Dispose()
        {
            if (this._RepoSphr != null)
            {
                this._RepoSphr.Dispose();
                this._RepoSphr = null;
            }
        }
        #endregion
    }

    public abstract class aRepositoryBaseWithTwoOwners : aRepositoryBase, IRepositoryOwnerCdadEjer
    {
        public int CurrentCdadOwner { get; protected set; }
        public int CurrentEjerOwner { get; protected set; }

        public IEnumerable<SQLCondition> GetCurrentOwnersCondition(string condition = "=", string separator = "AND", string tableAlias = "", string paramAlias = "")
        {
            yield return new SQLCondition("IdOwnerComunidad", tableAlias, $"@{paramAlias}idCdad", "", condition, separator);
            yield return new SQLCondition("IdOwnerEjercicio", tableAlias, $"@{paramAlias}idEjer", "", condition, "");
        }

        #region Dispose
        public override void Dispose()
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
