using AdConta;
using AdConta.Models;

namespace ModuloContabilidad.ObjModels
{
    public class Apunte : IObjModelBase, IOwnerComunidad, IObjWithDLO<ApunteDLO>
    {
        public Apunte() { }
        public Apunte(int id, int idComunidad, Asiento asiento, string FacturaId = null)
        {
            this._Id = id;
            this._IdOwnerComunidad = idComunidad;
            this._Asiento = asiento;
            this._Factura = FacturaId;
        }

        #region fields
        private int _Id;
        private int _IdOwnerComunidad;
#pragma warning disable CS0649
        private int _OrdenEnAsiento;
#pragma warning restore CS0649
        private Asiento _Asiento;
        private DebitCredit _DebeHaber;
        private decimal _Importe;
        private string _Factura;
        #endregion

        #region properties
        public int Id { get { return this._Id; } }        
        public int IdOwnerComunidad { get { return this._IdOwnerComunidad; } }
        public int OrdenEnAsiento { get { return this._OrdenEnAsiento; } }
        public Asiento Asiento { get { return this._Asiento; } }
        public string Concepto { get; set; }        
        public DebitCredit DebeHaber
        {
            get { return this._DebeHaber; }
            set
            {
                if (this._DebeHaber != value)
                {                    
                    if(!_Asiento.Abierto)
                        throw new CustomException_ObjModels(
                            $"Error cambiando DebeHaber de apunte numero {Id} de asiento numero {Asiento.Id}. Asiento cerrado.");
                    this._Asiento.CambiaSaldo(this, value);
                    this._DebeHaber = value;
                }
            }
        }        
        public decimal Importe
        {
            get { return this._Importe; }
            set
            {
                if (this._Importe != value)
                {
                    if (!_Asiento.Abierto)
                        throw new CustomException_ObjModels(
                            $"Error cambiando DebeHaber de apunte numero {Id} de asiento numero {Asiento.Id}. Asiento cerrado.");
                    this._Asiento.CambiaSaldo(this, value);
                    this._Importe = value;
                }
            }
        }
        public CuentaMayor Cuenta { get; set; }
        public bool Punteo { get; set; }        
        public string Factura { get { return this._Factura; } }
        #endregion

        #region DLO
        public ApunteDLO GetDLO()
        {
            return new ApunteDLO(Id, IdOwnerComunidad, OrdenEnAsiento, Asiento.Codigo.CurrentCodigo, Concepto, DebeHaber, Importe, Cuenta.NumCuenta.ToString(),
                Punteo, Factura);
        }
        #endregion
    }

    public sealed class ApunteDLO : IObjModelBase, IDataListObject
    {
        public ApunteDLO() { }
        public ApunteDLO(
            int id,
            int idCdad,
            int ordenEnAsiento,
            int codigoAsiento,
            string concepto,
            DebitCredit debeHaber,
            decimal importe,
            string cuenta,
            bool punteo,
            string factura)
        {
            this.Id = id;
            this.IdOwnerComunidad = idCdad;
            this.OrdenEnAsiento = ordenEnAsiento;
            this.CodigoAsiento = codigoAsiento;
            this.Concepto = concepto;
            this.DebeHaber = debeHaber;
            this.Importe = importe;
            this.Cuenta = cuenta;
            this.Punteo = punteo;
            this.Factura = factura;
        }

        public int Id { get; private set; }
        public int IdOwnerComunidad { get; private set; }
        public int OrdenEnAsiento { get; private set; }
        public int CodigoAsiento { get; private set; }
        public string Concepto { get; private set; }
        public DebitCredit DebeHaber { get; private set; }
        public decimal Importe { get; private set; }
        public string Cuenta { get; private set; }
        public bool Punteo { get; private set; }
        public string Factura { get; private set; }
    }


    #region old
    //public class Apunte : iObjModelBase, iOwnerComunidad
    //{
    //    //public Apunte() { }
    //    public Apunte(aAsiento asiento)
    //    {
    //        this._Asiento = asiento;
    //    }

    //    #region fields
    //    private int _Id;
    //    private int _IdOwnerComunidad;
    //    private decimal _Importe;
    //    private DebitCredit _DebeHaber;
    //    private aAsiento _Asiento;
    //    #endregion

    //    #region properties
    //    public int Id { get { return this._Id; } }
    //    public int IdOwnerComunidad { get { return this._IdOwnerComunidad; } }
    //    public CuentaMayor Account { get; set; }
    //    public decimal Importe
    //    {
    //        get { return this._Importe; }
    //        set
    //        {
    //            if (this._Importe != value)
    //            {
    //                decimal OldAmount = this._Importe;
    //                this._Importe = value;
    //                this._Asiento.CambiaSaldo(this, OldAmount);
    //            }
    //        }
    //    }
    //    public DebitCredit DebeHaber
    //    {
    //        get { return this._DebeHaber; }
    //        set
    //        {
    //            if (this._DebeHaber != value)
    //            {
    //                this._DebeHaber = value;
    //                this._Asiento.CambiaSaldo(this, this._Importe);
    //            }
    //        }
    //    }
    //    public string Concepto { get; set; }
    //    public bool Punteado { get; set; }
    //    //TODO definir bien los recibos y facturas
    //    public string Recibo { get; set; }
    //    public string Factura { get; set; }
    //    #endregion

    //    /*#region helpers
    //    public void SetAsiento(Asiento asiento)
    //    {
    //        this._Asiento = asiento;
    //    }
    //    #endregion*/
    //}

    //public abstract class aAsiento : iObjModelBase, iOwnerComunidad
    //{
    //    #region fields
    //    protected ObservableCollection<Apunte> _Apuntes;
    //    protected int _Id;
    //    private int _IdOwnerComunidad;
    //    #endregion

    //    #region properties
    //    public int Id { get { return this._Id; } }
    //    public int IdOwnerComunidad { get { return this._IdOwnerComunidad; } }
    //    public abstract DateTime Fecha { get; set; }
    //    public abstract decimal Balance { get; protected set; }
    //    public abstract bool IsNew { get; set; }
    //    public abstract Apunte this[int i] { get; }
    //    public virtual ObservableCollection<Apunte> Apuntes { get; }
    //    #endregion

    //    #region helpers
    //    /// <summary>
    //    /// Get accounting balance of list apuntes.
    //    /// </summary>
    //    /// <param name="apuntes"></param>
    //    /// <returns></returns>
    //    protected abstract decimal GetSaldoDe(ObservableCollection<Apunte> apuntes);
    //    /// <summary>
    //    /// Set this.Balance property as the accounting balance of the apuntes stored in property this._Apuntes.
    //    /// </summary>
    //    protected virtual void SetSaldo()
    //    {
    //        decimal sum = 0;
    //        int sign;
    //        foreach (Apunte ap in this._Apuntes)
    //        {
    //            sign = (ap.DebeHaber == DebitCredit.Debit) ? 1 : -1;
    //            sum += (ap.Importe * sign);
    //        }

    //        this.Balance = sum;
    //    }
    //    /// <summary>
    //    /// Modify this.Balance property to new accounting balance given the new apunte had been effectively added to the property this._Apuntes.
    //    /// </summary>
    //    /// <param name="apunte"></param>
    //    public virtual void SetSaldo(Apunte apunte)
    //    {
    //        int sign = (apunte.DebeHaber == DebitCredit.Debit) ? 1 : -1;
    //        this.Balance += (apunte.Importe * sign);
    //    }
    //    /// <summary>
    //    /// Modify this.Balance property to new accounting balance given that apunte have been changed.
    //    /// </summary>
    //    /// <param name="apunte"></param>
    //    /// <param name="oldamount"></param>
    //    public abstract void CambiaSaldo(Apunte apunte, decimal oldamount);
    //    #endregion

    //    #region public methods
    //    /// <summary>
    //    /// Get all apuntes on debit/credit.
    //    /// </summary>
    //    /// <param name="target"></param>
    //    /// <returns></returns>
    //    public virtual List<Apunte> GetApuntesAl(DebitCredit target)
    //    {
    //        return this._Apuntes.ToList<Apunte>().FindAll(x => x.DebeHaber == target);
    //    }
    //    /// <summary>
    //    /// Add apunte and returns if sum=0. Devuelve true si el asiento queda cuadrado después de añadir apunte.
    //    /// </summary>
    //    /// <param name="apunte"></param>
    //    /// <returns></returns>
    //    public virtual bool AddApunte(Apunte apunte)
    //    {
    //        this._Apuntes.Add(apunte);
    //        this.SetSaldo(apunte);
    //        return this.Balance == 0;
    //    }
    //    /// <summary>
    //    /// Remove apunte and returns if sum=0. Devuelve true si el asiento queda cuadrado después de borrar apunte.
    //    /// </summary>
    //    /// <param name="apunte"></param>
    //    /// <returns></returns>
    //    public virtual bool RemoveApunte(Apunte apunte)
    //    {
    //        this._Apuntes.Remove(apunte);
    //        this.SetSaldo(apunte);
    //        return this.Balance == 0;
    //    }
    //    /*/// <summary>
    //    /// Set amount of apunte.
    //    /// </summary>
    //    /// <param name="apunte"></param>
    //    /// <param name="amount"></param>
    //    /// <returns></returns>
    //    public bool SetApunteAmount(Apunte apunte, decimal amount)
    //    {
    //        Apunte ap = this._Apuntes.Find(x => x == apunte);
    //        ap.Amount = amount;
    //        return this.Balance == 0;
    //    }
    //    /// <summary>
    //    /// Set amount of apunte.
    //    /// </summary>
    //    /// <param name="index"></param>
    //    /// <param name="amount"></param>
    //    /// <returns></returns>
    //    public bool SetApunteAmount(int index, decimal amount)
    //    {
    //        Apunte ap = this._Apuntes[index];
    //        ap.Amount = amount;
    //        this.SetBalance(ap);
    //        return this.Balance == 0;
    //    }*/
    //    /// <summary>
    //    /// Get index of apunte.
    //    /// </summary>
    //    /// <param name="apunte"></param>
    //    /// <returns></returns>
    //    public virtual int GetIndexOfApunte(Apunte apunte)
    //    {
    //        return this._Apuntes.IndexOf(apunte);
    //    }
    //    #endregion
    //}
    #endregion
}
