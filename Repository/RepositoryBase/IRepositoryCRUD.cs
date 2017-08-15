using AdConta.Models;
using AdConta.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository
{
    public interface IRepositoryCRUD<T> where T : IObjModelBase
    {
        Task<T> GetByIdAsync(int id, aVMTabBase VM);
        Task<bool> AddNewAsync(T p, aVMTabBase VM);
        Task<bool> UpdateAsync(T p, aVMTabBase VM);
        Task<bool> RemoveAsync(T p, aVMTabBase VM);
    }
}
