using QBuilder;
using System.Collections.Generic;
using AdConta.Models;

namespace Repository
{
    #region 1 owner
    public abstract class aRepositoryBaseWithOneOwner<T> : aRepositoryInternal<T>, IRepositoryOwnerCdad where T : IObjModelBase
    {
        public int CurrentSingleOwner { get; protected set; }

        public SQLCondition GetCurrentOwnerCondition(string condition = "=", string alias = "", string paramAlias = "")
        {
            return new SQLCondition("IdOwnerComunidad", alias, $"@{paramAlias}idCdad", "", condition, "");
        }

        #region Dispose
        public override void Dispose()
        {
            base.Dispose();
        }
        #endregion
    }

    public abstract class aRepositoryBaseWith1Owner1DLO<T, TDLO> : aRepositoryInternalWithDLO<T, TDLO>, IRepositoryOwnerCdad 
        where T : IObjModelBase
        where TDLO : IDataListObject
    {
        public int CurrentSingleOwner { get; protected set; }

        public SQLCondition GetCurrentOwnerCondition(string condition = "=", string alias = "", string paramAlias = "")
        {
            return new SQLCondition("IdOwnerComunidad", alias, $"@{paramAlias}idCdad", "", condition, "");
        }
        public void StoreCurrentOwnerConditionParameter(QueryBuilder qBuilder)
        {
            qBuilder.StoreParameter("idCdad", this.CurrentSingleOwner);
        }

        #region Dispose
        public override void Dispose()
        {
            base.Dispose();
        }
        #endregion
    }

    public abstract class aRepositoryBaseWith1Owner2DLO<T, TDLO, TDLO2> : aRepositoryInternalWithTwoDLOs<T, TDLO, TDLO2>, IRepositoryOwnerCdad
        where T : IObjModelBase
        where TDLO : IDataListObject
        where TDLO2 : IDataListObject
    {
        public int CurrentSingleOwner { get; protected set; }

        public SQLCondition GetCurrentOwnerCondition(string condition = "=", string alias = "", string paramAlias = "")
        {
            return new SQLCondition("IdOwnerComunidad", alias, $"@{paramAlias}idCdad", "", condition, "");
        }
        public void StoreCurrentOwnerConditionParameter(QueryBuilder qBuilder)
        {
            qBuilder.StoreParameter("idCdad", this.CurrentSingleOwner);
        }

        #region Dispose
        public override void Dispose()
        {
            base.Dispose();
        }
        #endregion
    }
    #endregion

    #region 2 owners
    public abstract class aRepositoryBaseWithTwoOwners<T> : aRepositoryInternal<T>, IRepositoryOwnerCdadEjer where T : IObjModelBase
    {
        public int CurrentCdadOwner { get; protected set; }
        public int CurrentEjerOwner { get; protected set; }

        public IEnumerable<SQLCondition> GetCurrentOwnersCondition(string condition = "=", string separator = "AND", string tableAlias = "", string paramAlias = "")
        {
            yield return new SQLCondition("IdOwnerComunidad", tableAlias, $"@{paramAlias}idCdad", "", condition, separator);
            yield return new SQLCondition("IdOwnerEjercicio", tableAlias, $"@{paramAlias}idEjer", "", condition, "");
        }
        public void StoreCurrentOwnerConditionsParameters(QueryBuilder qBuilder)
        {
            qBuilder.StoreParameter("idCdad", this.CurrentCdadOwner);
            qBuilder.StoreParameter("idEjer", this.CurrentEjerOwner);
        }

        #region Dispose
        public override void Dispose()
        {
            base.Dispose();
        }
        #endregion
    }

    public abstract class aRepositoryBaseWith2Owners1DLO<T, TDLO> : aRepositoryInternalWithDLO<T, TDLO>, IRepositoryOwnerCdadEjer 
        where T : IObjModelBase
        where TDLO : IDataListObject
    {
        public int CurrentCdadOwner { get; protected set; }
        public int CurrentEjerOwner { get; protected set; }

        public IEnumerable<SQLCondition> GetCurrentOwnersCondition(string condition = "=", string separator = "AND", string tableAlias = "", string paramAlias = "")
        {
            yield return new SQLCondition("IdOwnerComunidad", tableAlias, $"@{paramAlias}idCdad", "", condition, separator);
            yield return new SQLCondition("IdOwnerEjercicio", tableAlias, $"@{paramAlias}idEjer", "", condition, "");
        }
        public void StoreCurrentOwnerConditionsParameters(QueryBuilder qBuilder)
        {
            qBuilder.StoreParameter("idCdad", this.CurrentCdadOwner);
            qBuilder.StoreParameter("idEjer", this.CurrentEjerOwner);
        }

        #region Dispose
        public override void Dispose()
        {
            base.Dispose();
        }
        #endregion
    }

    public abstract class aRepositoryBaseWith2Owners2DLO<T, TDLO, TDLO2> : aRepositoryInternalWithTwoDLOs<T, TDLO, TDLO2>, IRepositoryOwnerCdadEjer
        where T : IObjModelBase
        where TDLO : IDataListObject
        where TDLO2 : IDataListObject
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
            base.Dispose();
        }
        #endregion
    }
    #endregion
}
