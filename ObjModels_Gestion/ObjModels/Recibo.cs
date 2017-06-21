using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AdConta;
using AdConta.Models;

namespace ModuloGestion.ObjModels
{
    public class Recibo : IObjModelBase, IOwnerComunidad
    {
        private Recibo() { }
        public Recibo(int id, int idCdad, decimal importe, Date fecha, string concepto, List<Cobro> cobros, List<EntACta> entACtas)
        {
            this._Id = id;
            this._IdOwnerComunidad = idCdad;
            this._Importe = importe;
            this.Fecha = fecha;
            this.Concepto = concepto;
            this._Cobros = new CobrosDict(cobros.ToDictionary(c => c.Id, c => c));
            this._EntregasACuenta = new EntACtaDict(entACtas.ToDictionary(e => e.Id, e => e));
        }

        #region fields
        private int _Id;
        private int _IdOwnerComunidad;
        private decimal _Importe;

        private CobrosDict _Cobros;
        private EntACtaDict _EntregasACuenta;
        #endregion

        #region properties
        public int Id { get { return this._Id; } }
        public int IdOwnerComunidad { get { return this._IdOwnerComunidad; } }
        public decimal Importe { get { return this._Importe; } }
        public Date Fecha { get; set; }
        public string Concepto { get; set; }

        public CobrosDict Cobros { get { return this._Cobros; } }
        public EntACtaDict EntregasACuenta { get { return this._EntregasACuenta; } }
        #endregion

        #region public methods
        public ErrorSettingReciboDicts TrySetCobrosEntACta(ref CobrosDict cobros, ref EntACtaDict entregasACuenta)
        {
            decimal importeTotal = cobros.Total + entregasACuenta.Total;

            if (importeTotal != this.Importe)
                return ErrorSettingReciboDicts.ImporteIncorrecto;            

            List<int> fincas = new List<int>(entregasACuenta.Values.Select(x => x.IdOwnerFinca).Distinct());
            if (fincas.Count() != entregasACuenta.Count)
                return ErrorSettingReciboDicts.VariasEACaMismaFinca;

            this._Cobros = cobros;
            this._EntregasACuenta = entregasACuenta;
            return ErrorSettingReciboDicts.None;
        }
        #endregion
    }

}
