﻿using System;
using System.Collections.Generic;
using System.Linq;
using AdConta;
using AdConta.Models;

namespace ModuloContabilidad.ObjModels
{
    public class Asiento : IObjModelBase, IAsiento, IObjModelConCodigoConComunidadYEjercicio, IOwnerComunidad, IOwnerEjercicio
    //<- owners Incluidos en iObjModelConCodigoConComunidad
    {
        public Asiento(int id, int idComunidad, int idEjercicio, int codigo, AutoCodigoData ACData, bool abierto = false)
        {
            this._Id = id;
            this._IdOwnerComunidad = idComunidad;
            this._IdOwnerEjercicio = idEjercicio;
            _Abierto = abierto;
            FechaValor = DateTime.Today;
            this.Codigo = new AutoCodigoOwnerCdEj<Asiento>(ACData, codigo);
        }
        public Asiento(int id, int idComunidad, int idEjercicio, int codigo, AutoCodigoData ACData, DateTime fechaValor, IEnumerable<Apunte> apuntes, bool abierto = false)
        {
            this._Id = id;
            this._IdOwnerComunidad = idComunidad;
            this._IdOwnerEjercicio = idEjercicio;
            _Abierto = abierto;
            FechaValor = FechaValor;
            this.Codigo = new AutoCodigoOwnerCdEj<Asiento>(ACData, codigo);

            this.Apuntes = new ObservableApuntesList(this, apuntes);
            CalculaSaldo();
        }

        #region fields
        private int _Id;
        private int _IdOwnerComunidad;
        private int _IdOwnerEjercicio;
        private decimal _Saldo;
        private bool _Abierto;
        #endregion

        #region properties
        public int Id { get { return this._Id; } }
        public int IdOwnerComunidad { get { return this._IdOwnerComunidad; } }
        public int IdOwnerEjercicio { get { return this._IdOwnerEjercicio; } }
        public aAutoCodigoBase Codigo { get; private set; }
        public ObservableApuntesList Apuntes { get; private set; }
        public DateTime Fecha { get; set; }
        public DateTime FechaValor { get; private set; }
        public decimal Saldo { get { return this._Saldo; } }
        public Apunte this[int i] { get { return this.Apuntes[i]; } }
        public bool Abierto { get { return this._Abierto; } }
        #endregion

        #region helpers
        /// <summary>
        /// Set this.Balance property as the accounting balance of the apuntes currently stored in this._Apuntes.
        /// </summary>
        public void CalculaSaldo()
        {
            _Saldo = Apuntes.SumaDebe - Apuntes.SumaHaber;
        }
        /// <summary>
        /// Get accounting balance of list apuntes.
        /// </summary>
        /// <param name="apuntes"></param>
        /// <returns></returns>
        public decimal GetSaldoDe(IEnumerable<Apunte> apuntes)
        {
            decimal sum = 0;
            //int sign;
            foreach (Apunte ap in this.Apuntes)
            {
                //sign = (ap.DebeHaber == DebitCredit.Debit) ? 1 : -1;
                //sum += (ap.Importe * sign);
                sum += ap.ImporteAlDebe;
                sum -= ap.ImporteAlHaber;
            }

            return sum;
        }
        #endregion

        #region public methods
        public void SetApuntesList(IEnumerable<Apunte> apuntes)
        {
            if (!this.Abierto) return;
            this.Apuntes = new ObservableApuntesList(this, apuntes);
            CalculaSaldo();
        }
        /// <summary>
        /// Modify this._Saldo property to new accounting balance given that apunte have not been changed yet.
        /// </summary>
        /// <param name="apunte"></param>
        /// <param name="oldamount"></param>
        public void CambiaSaldo(Apunte apunte, decimal nuevoImporte)
        {
            //nuevo saldo = saldo + (-debehaber * importe viejo) + (debehaber * importe nuevo) = saldo + debehaber * (-importe viejo + importe nuevo)
            this._Saldo += (((int)apunte.DebeHaber) * (nuevoImporte - apunte.Importe));
            //if (apunte.DebeHaber == DebitCredit.Debit) _Saldo = _Saldo - apunte.Importe + nuevoImporte;
            //else _Saldo = _Saldo + apunte.Importe - nuevoImporte;
        }
        /// <summary>
        /// Modify this._Saldo property to new accounting balance given that apunte have not been changed yet.
        /// nuevoDebeHaber es diferente que apunte.DebeHaber.
        /// </summary>
        /// <param name="apunte"></param>
        /// <param name="oldamount"></param>
        public void CambiaSaldo(Apunte apunte, DebitCredit nuevoDebeHaber)
        {
            //nuevo saldo = saldo - (debehaber * importe * 2) => debehaber -1 si haber y +1 si debe
            this._Saldo -= (((int)nuevoDebeHaber) * apunte.Importe * 2);
            //if (apunte.DebeHaber == DebitCredit.Debit) _Saldo = _Saldo - (apunte.Importe * 2);
            //else _Saldo = _Saldo + (apunte.Importe * 2);
        }
        /// <summary>
        /// Modify this._Saldo property to new accounting balance given that apunte have not been changed yet.
        /// nuevoDebeHaber es diferente que apunte.DebeHaber.
        /// nuevoImporte es diferente que apunte.Importe.
        /// </summary>
        /// <param name="apunte"></param>
        /// <param name="nuevoImporte"></param>
        /// <param name="nuevoDebeHaber"></param>
        public void CambiaSaldo(Apunte apunte, decimal nuevoImporte, DebitCredit nuevoDebeHaber)
        {
            //nuevo saldo = saldo - (Viejodebehaber * viejo importe) - (Viejodebehaber * nuevo importe) = saldo - Viejodebehaber(viejo importe + nuevo importe)
            this._Saldo += (((int)nuevoDebeHaber) * (apunte.Importe + nuevoImporte));
        }
        /// <summary>
        /// Get all apuntes on specified debit/credit.
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public List<Apunte> GetApuntesAl(DebitCredit target)
        {
            List<Apunte> result = this.Apuntes.Where(apunte => apunte.DebeHaber == target).ToList();
            return result;
        }
        /// <summary>
        /// Saldo al debe o al haber del asiento.
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public decimal GetSaldoAl(DebitCredit target)
        {
            return GetSaldoDe(GetApuntesAl(target));
        }
        /// <summary>
        /// Si está abierto el asiento puede estar descuadrado (saldo != 0) y se puede modificar.
        /// Si intenta cerrarse con saldo descuadrado, devuelve false y no cierra.
        /// Si intenta cerrarse sin fecha (this.Fecha == null), devuelve false y no cierra.
        /// Si ya estaba cerrado, devuelve verdadero.
        /// El asiento no se puede modificar si no está abierto.
        /// </summary>
        /// <returns></returns>
        public bool TryCerrar()
        {
            if (!Abierto) return true;
            if (Saldo != 0) return false;
            if (Fecha == null) return false;

            _Abierto = false;
            return true;
        }

        public Tuple<int, int> GetOwnersIds()
        {
            return new Tuple<int, int>(this.IdOwnerComunidad, this.IdOwnerEjercicio);
        }
        #endregion
    }
}

/*
/// <summary>
        /// Add apunte and returns if sum=0. Devuelve true si el asiento queda cuadrado después de añadir apunte.
        /// </summary>
        /// <param name="apunte"></param>
        /// <returns></returns>
        public bool AddApunte(Apunte apunte)
        {
            if (!Abierto) return false;

            CalculaSaldo();
            return true;
        }
        /// <summary>
        /// Remove apunte and returns if sum=0. Devuelve true si el asiento queda cuadrado después de borrar apunte.
        /// </summary>
        /// <param name="apunte"></param>
        /// <returns></returns>
        public bool RemoveApunte(Apunte apunte)
        {
            if (!Abierto) return false;

            CalculaSaldo();
            return true;
        }
        public bool ReplaceApunte(Apunte oldApunte, Apunte newApunte)
        {
            if (!Abierto) return false;

            if (oldApunte.DebeHaber == DebitCredit.Debit) _Saldo -= oldApunte.Importe;
            else _Saldo += oldApunte.Importe;

            if (newApunte.DebeHaber == DebitCredit.Debit) _Saldo += newApunte.Importe;
            else _Saldo -= newApunte.Importe;

            return true;
        }
*/
