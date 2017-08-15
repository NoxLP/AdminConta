using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AdConta.ViewModel;
using Mapper;
using System.Collections.ObjectModel;
using AdConta.Models;

namespace Repository
{
    public interface IRepositoryDLO<TDLO> where TDLO : IDataListObject
    {
        ReadOnlyCollection<TDLO> DLOs { get; }
        Task UpdateDLOsDictionariesAsync(IEnumerable<int> owners);
        Task CleanDLOsDictionariesAsync();
    }

    public interface IRepository2DLOs<TDLO, TDLO2>  where TDLO : IDataListObject
    {
        ReadOnlyCollection<TDLO> DLOs1 { get; }
        ReadOnlyCollection<TDLO2> DLOs2 { get; }
        Task UpdateDLOsDictionariesAsync(IEnumerable<int> owners);
        Task CleanDLOs1DictionariesAsync();
        Task CleanDLOs2DictionariesAsync();
    }
}
