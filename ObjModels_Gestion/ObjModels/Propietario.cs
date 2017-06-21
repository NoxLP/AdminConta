using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AdConta;
using AdConta.Models;

namespace ModuloGestion.ObjModels
{
    public class Propietario : Persona, IOwnerComunidad, IObjWithDLO<PropietarioDLO>
    {
        public Propietario(int id, int idComunidad, string nif, string nombre, bool forceInvalidNIF = false) 
            : base(id, nif, nombre, forceInvalidNIF)
        {
            this._Cuotas = new Dictionary<int, Cuota>();
            this._IdOwnerComunidad = idComunidad;
        }
        
        #region fields
        private int _IdOwnerComunidad;        
        private Dictionary<int, Cuota> _Cuotas;
        #endregion

        #region properties
        public int IdOwnerComunidad { get { return this._IdOwnerComunidad; } }
        public ReadOnlyDictionary<int, Cuota> Cuotas { get { return new ReadOnlyDictionary<int, Cuota>(this._Cuotas); } }
        #endregion

        #region public methods
        public void CambioNombrePropietario(string nombre)
        {
            base.Nombre = nombre;
        }
        public void RemoveCuotas(ref List<Cuota> cuotasToRemove, Date fechaInicial, Date fechaFinal)
        {
            IEnumerable<Cuota> cuotasEnum = this._Cuotas.Where(x => (
                x.Value.Mes > fechaInicial && x.Value.Mes < fechaFinal
                )) as IEnumerable<Cuota>;

            cuotasToRemove.AddRange(cuotasEnum);
            foreach (Cuota cuota in cuotasEnum)
                this._Cuotas.Remove(cuota.Id);
        }
        public void AddCuotas(ref List<Cuota> cuotasToAdd)
        {
            this._Cuotas.Union(cuotasToAdd.ToDictionary(x => x.Id)); //Ya hace distinct => no es necesario comprobar si tiene las id
        }
        #endregion

        #region DLO
        public PropietarioDLO GetDLO()
        {
            return new PropietarioDLO(Id, IdOwnerComunidad, Nombre, NIF.NIF, Direccion.GetDireccionSinCP(), CuentaBancaria.AccountNumber,
                Telefono1.Numero, Email);
        }
        #endregion
    }

    public class PropietarioDLO : IObjModelBase, IDataListObject
    {
        public PropietarioDLO() { }
        public PropietarioDLO(
            int id,
            int idCdad,
            string nombre,
            string nIF,
            string direccion,
            string cuentaBancaria,
            string telefono,
            string email)
        {
            this.Id = id;
            this.IdOwnerComunidad = idCdad;
            this.Nombre = nombre;
            this.NIF = nIF;
            this.Direccion = direccion;
            this.CuentaBancaria = cuentaBancaria;
            this.Telefono = telefono;
            this.Email = email;
        }

        public int Id { get; private set; }
        public int IdOwnerComunidad { get; private set; }
        public string Nombre { get; private set; }
        public string NIF { get; private set; }
        public string Direccion { get; private set; }
        public string CuentaBancaria { get; private set; }
        public string Telefono { get; private set; }
        public string Email { get; private set; }
    }
}
