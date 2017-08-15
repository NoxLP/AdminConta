using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using QBuilder;
using AdConta.Models;

namespace Repository
{
    public interface IRepositoryDependent<T> where T : IObjModelBase
    {
        ConcurrentDictionary<T, List<int>> DependenciesDict { get; }

        QueryBuilder GetAllDependentByMasterSelectSQL(int dependentIdCodigo);
        QueryBuilder GetIdsDependentByMasterSelectSQL(int dependentIdCodigo);
        QueryBuilder GetDependentByMasterSelectSQL(int dependentIdCodigo, IEnumerable<int> ids);
        Task RollbackDependentAsync(IEnumerable<int> ids);
        Task ApplyDependentAsync(IEnumerable<int> idsToAdd, IEnumerable<int> idsToRemove);
    }
}