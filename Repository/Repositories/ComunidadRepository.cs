using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AdConta.ViewModel;
using QBuilder;
using Mapper;
using AdConta.Models;
using ModuloContabilidad.ObjModels;

namespace Repository
{
    public sealed class ComunidadRepository : aRepositoryInternal<Comunidad>, IRepositoryCRUD<Comunidad>
    {
        public ComunidadRepository()
        {
            MapperStore store = new MapperStore();
            this._Mapper = (DapperMapper<Comunidad>)store.GetMapper(GetObjModelType());
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
        private QueryBuilder GetInsertSQL(Comunidad cuenta)
        {
            throw new NotImplementedException();
        }
        protected override QueryBuilder GetDeleteSQL(int id)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region public methods
        public async Task<Comunidad> GetByIdAsync(int id, aVMTabBase VM)
        {
            throw new NotImplementedException();
        }
        public async Task<bool> AddNewAsync(Comunidad ComunidadObj, aVMTabBase VM)
        {
            throw new NotImplementedException();
        }
        public async Task<bool> UpdateAsync(Comunidad ComunidadObj, aVMTabBase VM)
        {
            throw new NotImplementedException();
        }
        public async Task<bool> RemoveAsync(Comunidad ComunidadObj, aVMTabBase VM)
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
