using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;

namespace ModuloContabilidad.ObjModels
{
    //public class AsientoSimple : aAsiento
    //{
    //    public AsientoSimple(int nAsiento)
    //    {
    //        base._Id = nAsiento;
    //        this.Fecha = DateTime.Today;
    //        base._Apuntes = new ObservableCollection<Apunte>();
    //        this.Balance = 0;
    //        this.IsNew = true;
    //    }
    //    public AsientoSimple(DateTime date, ObservableCollection<Apunte> apuntes, bool isNew)
    //    {
    //        if (apuntes.Count < 2)
    //        {
    //            System.Windows.MessageBox.Show("No se puede crear un asiento con menos de dos apuntes.");
    //            return;
    //        }
    //        else if (GetSaldoDe(apuntes) != 0)
    //        {
    //            System.Windows.MessageBox.Show("El asiento que está intentando crear no está cuadrado.");
    //            return;
    //        }

    //        this.Fecha = date;
    //        base._Apuntes = apuntes;
    //        this.SetSaldo();
    //        this.IsNew = isNew;
    //    }

    //    #region properties
    //    public override DateTime Fecha { get; set; }
    //    public override decimal Balance { get; protected set; }
    //    public override bool IsNew { get; set; }
    //    public override Apunte this[int i]
    //    {
    //        get { return this._Apuntes[i]; }
    //    }
    //    /*public IEnumerable Apuntes
    //    {
    //        get
    //        {
    //            foreach (Apunte ap in this._Apuntes)
    //                yield return ap;
    //        }
    //    }*/
    //    public override ObservableCollection<Apunte> Apuntes
    //    {
    //        get { return base._Apuntes; }
    //    }
    //    #endregion

    //    #region helpers
    //    /// <summary>
    //    /// Get accounting balance of list apuntes.
    //    /// </summary>
    //    /// <param name="apuntes"></param>
    //    /// <returns></returns>
    //    protected override decimal GetSaldoDe(ObservableCollection<Apunte> apuntes)
    //    {
    //        throw new NotImplementedException();
    //    }
    //    /// <summary>
    //    /// Modify this.Balance property to new accounting balance given that apunte have been changed.
    //    /// </summary>
    //    /// <param name="apunte"></param>
    //    /// <param name="oldamount"></param>
    //    public override void CambiaSaldo(Apunte apunte, decimal oldamount)
    //    {
    //        throw new NotImplementedException();
    //    }
    //    #endregion
    //}
}
