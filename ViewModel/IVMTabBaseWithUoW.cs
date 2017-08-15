using System.Threading.Tasks;

namespace AdConta.ViewModel
{
    public interface IVMTabBaseWithUoW
    {
        Task CleanUnitOfWorkAsync();
        void InitializeUoW();
    }
}
