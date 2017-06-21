using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AdConta;
using AdConta.Models;

namespace ModuloGestion.ObjModels
{
    public interface iIngresoPropietario : IObjModelBase, IOwnerPersona, IOwnerRecibo
    {
        Date Fecha { get; }
        decimal Importe { get; }
        SituacionReciboCobroEntaCta Situacion { get; }
    }

    public class Cobro : IObjModelBase, iIngresoPropietario, IOwnerCuota
    {
        public int Id { get; private set; }
        public int IdOwnerRecibo { get; private set; }
        public int IdOwnerPersona { get; private set; }
        public int IdOwnerCuota { get; private set; }
        public Date Fecha { get; private set; }
        public bool Total { get; private set; }
        public decimal Importe { get; private set; }
        public SituacionReciboCobroEntaCta Situacion { get; private set; }

        public Cobro(int id, int idrecibo, int idcuota, decimal importe, Date fecha, int idPersona, 
            bool total = true, SituacionReciboCobroEntaCta situacion = SituacionReciboCobroEntaCta.Normal)
        {
            if (id < 0 || idrecibo < 0 || idcuota < 0) throw new System.Exception("sCobro's Ids have to be > 0");
            
            this.Id = id;
            this.IdOwnerRecibo = idrecibo;
            this.IdOwnerCuota = idcuota;
            this.IdOwnerPersona = idPersona;
            this.Fecha = fecha;
            this.Total = total;
            this.Importe = importe;
            this.Situacion = situacion;
        }
    }

    public class EntACta : IObjModelBase, iIngresoPropietario, IOwnerFinca
    {
        public int Id { get; private set; }
        public int IdOwnerRecibo { get; private set; }        
        public int IdOwnerPersona { get; private set; }
        public int IdOwnerFinca { get; private set; }
        public Date Fecha { get; private set; }
        public decimal Importe { get; private set; }
        public SituacionReciboCobroEntaCta Situacion { get; private set; }

        public EntACta(int id, int idrecibo, int idfinca, decimal importe, Date fecha, int idPersona,
            SituacionReciboCobroEntaCta situacion = SituacionReciboCobroEntaCta.Normal)
        {
            if (id < 0 || idrecibo < 0) throw new System.Exception("sEntACta's Ids have to be > 0");

            this.Id = id;
            this.IdOwnerRecibo = idrecibo;
            this.IdOwnerFinca = idfinca;
            this.IdOwnerPersona = idPersona;
            this.Fecha = fecha;
            this.Importe = importe;
            this.Situacion = situacion;
        }
    }
}
