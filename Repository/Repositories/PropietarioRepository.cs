using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AdConta.ViewModel;
using QBuilder;
using ModuloGestion.ObjModels;
using Mapper;

namespace Repository
{
    public sealed class PropietarioRepository : aRepositoryInternal<Propietario>, IRepositoryCRUD<Propietario>
    {
        public PropietarioRepository()
        {
            MapperStore store = new MapperStore();
            this._Mapper = (DapperMapper<Propietario>)store.GetMapper(GetObjModelType());
            this.Transactions = new ConcurrentDictionary<aVMTabBase, List<Tuple<QueryBuilder, IConditionToCommit>>>();
        }

        #region SQL helpers
        protected override QueryBuilder GetSelectSQL(int id)
        {
            throw new NotImplementedException();
        }
        protected override QueryBuilder GetUpdateSQL(int id, aVMTabBase VM)
        {
            throw new NotImplementedException();
        }
        private QueryBuilder GetInsertSQL(Propietario cuenta)
        {
            throw new NotImplementedException();
        }
        protected override QueryBuilder GetDeleteSQL(int id)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region public methods
        public async Task<Propietario> GetByIdAsync(int id, aVMTabBase VM)
        {
            throw new NotImplementedException();
        }
        public async Task<bool> AddNewAsync(Propietario PropietarioObj, aVMTabBase VM)
        {
            throw new NotImplementedException();
        }
        public async Task<bool> UpdateAsync(Propietario PropietarioObj, aVMTabBase VM)
        {
            throw new NotImplementedException();
        }
        public async Task<bool> RemoveAsync(Propietario PropietarioObj, aVMTabBase VM)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region Dispose
        public override void Dispose()
        {
            base.Dispose();
        }
        #endregion
    }


}
