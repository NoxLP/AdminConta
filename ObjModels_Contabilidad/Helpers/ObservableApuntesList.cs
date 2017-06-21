using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using AdConta;
using System.ComponentModel;

namespace ModuloContabilidad.ObjModels
{
    public class ObservableApuntesList : ObservableCollection<Apunte>
    {
        public ObservableApuntesList(Asiento asiento)
        {
            this._Asiento = asiento;
        }
        public ObservableApuntesList(Asiento asiento, IEnumerable<Apunte> apuntes)
        {
            this._Asiento = asiento;

            IOrderedEnumerable<Apunte> oApuntes = apuntes.OrderBy(x => x.OrdenEnAsiento);
            for(int i = 0;i<oApuntes.Count();i++)
                SetItem(i, oApuntes.ElementAt(i));
        }

        #region fields
        private Asiento _Asiento;
        #endregion

        #region properties
        public decimal SumaDebe { get; private set; }
        public decimal SumaHaber { get; private set; }
        #endregion

        #region helpers
        private void Suma(Apunte item)
        {
            if (item.DebeHaber == DebitCredit.Debit) SumaDebe += item.Importe;
            else SumaHaber += item.Importe;
        }
        private void Resta(Apunte item)
        {
            if (item.DebeHaber == DebitCredit.Debit) SumaDebe -= item.Importe;
            else SumaHaber -= item.Importe;
        }
        #endregion

        #region public methods
        protected override void InsertItem(int index, Apunte item)
        {
            if (!_Asiento.Abierto) return;
            base.InsertItem(index, item);
            Suma(item);
            _Asiento.CalculaSaldo();
        }
        protected override void RemoveItem(int index)
        {
            if (!_Asiento.Abierto) return;
            base.RemoveItem(index);
            Resta(this.Items[index]);
            _Asiento.CalculaSaldo();
        }
        protected override void SetItem(int index, Apunte item)
        {
            if (!_Asiento.Abierto) return;
            base.SetItem(index, item);
            Resta(this.Items[index]);
            Suma(item);
            _Asiento.CalculaSaldo();
        }
        protected override void ClearItems()
        {
            if (!_Asiento.Abierto) return;
            base.ClearItems();
            SumaDebe = 0;
            SumaHaber = 0;
            _Asiento.CalculaSaldo();
        }
        #endregion
    }

    //public class ApuntesList : aProtectedList<Apunte>
    //{
    //    #region properties
    //    public decimal SumaDebe { get; private set; }
    //    public decimal SumaHaber { get; private set; }
    //    #endregion

    //    #region helpers
    //    private void Suma(Apunte item)
    //    {
    //        if (item.DebeHaber == DebitCredit.Debit) SumaDebe += item.Importe;
    //        else SumaHaber += item.Importe;
    //    }
    //    private void Resta(Apunte item)
    //    {
    //        if (item.DebeHaber == DebitCredit.Debit) SumaDebe -= item.Importe;
    //        else SumaHaber -= item.Importe;
    //    }
    //    #endregion

    //    #region public methods
    //    public override void Add(Apunte item)
    //    {
    //        Suma(item);
    //        this._List.Add(item);
    //    }
    //    public override void Clear()
    //    {
    //        SumaDebe = 0;
    //        SumaHaber = 0;
    //        this._List.Clear();
    //    }
    //    public void Remove(Apunte item)
    //    {
    //        int i = this._List.IndexOf(item);
    //        RemoveAt(i);
    //    }
    //    public override void RemoveAt(int index)
    //    {
    //        Apunte apunte = this._List[index];
    //        Resta(apunte);
    //        this._List.RemoveAt(index);
    //    }
    //    public override void AddRange(IEnumerable<Apunte> collection)
    //    {
    //        foreach(Apunte apunte in collection) Add(apunte);
    //    }
    //    public override void RemoveRange(int index, int count)
    //    {
    //        for (int i = index; i < (index + count); i++) RemoveAt(i);
    //    }
    //    public override aReadOnlyProtectedList<Apunte> AsReadOnly()
    //    {
    //        return new ReadOnlyApuntesList(this._List);
    //    }
    //    #endregion
    //}

    //public class ReadOnlyApuntesList : aReadOnlyProtectedList<Apunte>
    //{
    //    public ReadOnlyApuntesList(List<Apunte> listaApuntes) : base(listaApuntes) { }
    //}

}
