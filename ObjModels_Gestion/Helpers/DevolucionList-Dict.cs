using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AdConta;

namespace ModuloGestion.ObjModels
{
    public class DevolucionesList : aProtectedList<Devolucion>
    {
        public DevolucionesList(List<Devolucion> lista)
        {
            this._Total = 0;
            this._TotalGastos = 0;
            this._List = lista;

            foreach(Devolucion dev in this._List)
            {
                this._Total += dev.ImporteTotal;
                this._TotalGastos += dev.GastosTotal;
            }
        }

        #region fields
        private decimal _Total;
        private decimal _TotalGastos;
        #endregion

        #region properties
        public decimal Total { get { return this._Total; } }
        public decimal TotalGastos { get { return this._TotalGastos; } }
        #endregion

        #region public methods
        public override void Add(Devolucion item)
        {
            this._List.Add(item);
            this._Total += item.ImporteTotal;
            this._TotalGastos += item.GastosTotal;
        }
        public override void AddRange(IEnumerable<Devolucion> collection)
        {
            base.AddRange(collection);
        }
        public override void RemoveAt(int index)
        {
            if (index < 0 || index > this.Count)
                throw new IndexOutOfRangeException();

            this._Total -= this[index].ImporteTotal;
            this._TotalGastos -= this[index].GastosTotal;
            base._List.RemoveAt(index);
        }
        public override void RemoveRange(int index, int count)
        {
            if (index < 0 || index > this.Count || (index + count) > this.Count)
                throw new IndexOutOfRangeException();

            for (int i = index; i < count; i++)
            {
                this._Total -= this[i].ImporteTotal;
                this._TotalGastos -= this[i].GastosTotal;
            }

            base._List.RemoveRange(index, count);
        }
        public override void Clear()
        {
            this._Total = 0;
            this._TotalGastos = 0;
            this._List.Clear();
        }
        public IEnumerable<IngresoDevuelto> GetIngresosDevueltosEnumerable()
        {
            return (IEnumerable<IngresoDevuelto>)this._List.Select(x => x.IngresosDevueltos.AsEnumerable<IngresoDevuelto>());
        }
        #endregion
    }

}
