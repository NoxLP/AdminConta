using Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModuloContabilidad
{
    public interface IVMConAsientoRepo : AdConta.ViewModel.IViewModelBase
    {
        AsientoRepository AsientoRepo { get; }
        UnitOfWork UOW { get; }
    }
}
