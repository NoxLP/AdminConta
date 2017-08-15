using QBuilder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository
{
    public interface IRepositoryOwnerCdad
    {
        int CurrentSingleOwner { get; }

        SQLCondition GetCurrentOwnerCondition(string condition = "=", string alias = "", string paramAlias = "");
    }

    public interface IRepositoryOwnerCdadEjer
    {
        int CurrentCdadOwner { get; }
        int CurrentEjerOwner { get; }

        IEnumerable<SQLCondition> GetCurrentOwnersCondition(string condition = "=", string separator = "AND", string tableAlias = "", string paramAlias = "");
    }
}
