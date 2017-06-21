using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AdConta;

namespace ModuloContabilidad.ObjModels
{
    public class GastosPagosList<T> : aProtectedList<T> where T : GastosPagosBase
    {
        public GastosPagosList()
        {
            this._Total = 0;
        }
        public GastosPagosList(List<T> items) : base(items)
        {
            this._Total = 0;
            foreach (T item in this._List) this._Total += item.Importe;
        }

        #region fields
        private decimal _Total;
        #endregion

        #region properties
        public decimal Total { get { return this._Total; } }
        #endregion

        #region public methods
        public override void Add(T item)
        {
            this._List.Add(item);
            this._Total += item.Importe;
        }
        public override void RemoveAt(int index)
        {
            if(index < 0 || index > this.Count)
                throw new IndexOutOfRangeException();

            this._Total -= this[index].Importe;
            this._List.RemoveAt(index);
        }
        public override void Clear()
        {
            this._Total = 0;
            this._List.Clear();
        }
        public override void AddRange(IEnumerable<T> collection)
        {
            base._List.AddRange(collection);

            foreach (T gasto in collection)
            {
                this._Total += gasto.Importe;
            }
        }
        public override void RemoveRange(int index, int count)
        {
            if (index < 0 || index > this.Count || (index + count) > this.Count)
                throw new IndexOutOfRangeException();

            for (int i = index; i < count; i++)
            {
                this._Total -= this[i].Importe;
            }

            base._List.RemoveRange(index, count);
        }
        public new ReadOnlyGastosPagosList<T> AsReadOnly()
        {
            return new ReadOnlyGastosPagosList<T>(this._List, this._Total);
        }
        #endregion
    }

    public class ReadOnlyGastosPagosList<T> : aReadOnlyProtectedList<T> where T : GastosPagosBase
    {
        public ReadOnlyGastosPagosList(List<T> list, decimal? total = null) : base (list)
        {
            if (total != null)
                this._Total = (decimal)total;
            else if (list != null && list.Count != 0)
            {
                foreach (T item in list) this._Total += item.Importe;
            }
        }

        #region fields
        private decimal _Total;
        #endregion

        #region properties
        public decimal Total { get { return this._Total; } }
        #endregion
    }

}
