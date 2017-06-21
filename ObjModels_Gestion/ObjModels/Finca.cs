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
    public class Finca : 
        IObjModelBase, IObjModelConCodigoConComunidad, IOwnerComunidad, //<- owner Incluido en iObjModelConCodigoConComunidad
        IBaja, IObjWithDLO<FincaDLO>
    {
        #region constructors
        private Finca() { }

        public Finca(int id, int idOwnerComunidad, bool baja, string nombre, double coeficiente, int codigo, AutoCodigoData ACData)
        {
            this._Id = id;
            this._IdOwnerComunidad = idOwnerComunidad;
            this._Baja = baja;
            this._Nombre = nombre;
            this._Coeficiente = coeficiente;
            this.Codigo = new AutoCodigoOwnerCdad<Finca>(ACData, ACodigoCCheckType.Fincas, codigo);
        }
        public Finca(
            int id,
            int idOwnerComunidad,
            bool baja,
            string nombre,
            double coeficiente,
            int codigo,
            AutoCodigoData ACData,
            Propietario propietarioActual,
            Dictionary<DateTime,int> historicoProps)
        {
            this._Id = id;
            this._IdOwnerComunidad = idOwnerComunidad;
            this._Baja = baja;
            this._Nombre = nombre;
            this._Coeficiente = coeficiente;
            this.Codigo = new AutoCodigoOwnerCdad<Finca>(ACData, ACodigoCCheckType.Fincas, codigo);
            this._PropietarioActual = propietarioActual;
            this._HistoricoPropietarios = historicoProps;
        }
        public Finca(
            int id,
            int idOwnerComunidad,
            bool baja,
            string nombre,
            double coeficiente,
            int codigo,
            AutoCodigoData ACData,
            Propietario propietarioActual,
            Dictionary<DateTime, int> historicoProps,
            Dictionary<int,Cuota> cuotas,
            EntACtaDict EAC = null,
            DevolucionesList devoluciones = null)
        {
            this._Id = id;
            this._IdOwnerComunidad = idOwnerComunidad;
            this._Baja = baja;
            this._Nombre = nombre;
            this._Coeficiente = coeficiente;
            this.Codigo = new AutoCodigoOwnerCdad<Finca>(ACData, ACodigoCCheckType.Fincas, codigo);
            this._PropietarioActual = propietarioActual;
            this._HistoricoPropietarios = historicoProps;
            this._Cuotas = cuotas;
            this._EntregasACuenta = EAC;
            this._Devoluciones = devoluciones;
        }
        #endregion
        
        #region fields
        private int _Id;
        private int _IdOwnerComunidad;
        private bool _Baja;
        private string _Nombre;
        private double _Coeficiente;
        
        private Propietario _PropietarioActual;
        private Dictionary<DateTime, int> _HistoricoPropietarios;

        private int[] _IdCopropietarios = new int[3];
        private Tuple<int, TipoPagoCuotas>[] _IdPagadores = new Tuple<int, TipoPagoCuotas>[3];
        private Dictionary<int, Cuota> _Cuotas;
        private EntACtaDict _EntregasACuenta;
        private DevolucionesList _Devoluciones;
        #endregion
        
        #region properties
        public int Id { get { return this._Id; } }
        public int IdOwnerComunidad { get { return this._IdOwnerComunidad; } }
        public bool Baja { get { return this._Baja; } }
        public string Nombre { get { return this._Nombre; } }
        public double Coeficiente { get { return this._Coeficiente; } }
        public aAutoCodigoBase Codigo { get; private set; }

        public DireccionPostal Direccion { get; set; }
        public DireccionPostal Direccion2 { get; set; }
        public TipoPagoCuotas DefaultTipoPagoCuotas { get; set; }

        public Propietario PropietarioActual { get { return this._PropietarioActual; } }
        public ReadOnlyDictionary<DateTime, int> HistoricoPropietarios { get { return new ReadOnlyDictionary<DateTime, int>(this._HistoricoPropietarios); } }

        public sTelefono Telefono1 { get; set; }
        public sTelefono Telefono2 { get; set; }
        public sTelefono Telefono3 { get; set; }
        public sTelefono Fax { get; set; }
        public string Email { get; set; }
        public int[] IdCopropietarios
        {
            get { return this._IdCopropietarios; }
            set { this._IdCopropietarios = value; }
        }
        public Tuple<int, TipoPagoCuotas>[] IdPagadores
        {
            get { return this._IdPagadores; }
            set { this._IdPagadores = value; }
        }
        public int[] IdAsociadas { get; set; }
        public ReadOnlyDictionary<int, Cuota> Cuotas { get { return new ReadOnlyDictionary<int, Cuota>(this._Cuotas); } }
        public EntACtaDict EntregasACuenta { get { return this._EntregasACuenta; } }
        public DevolucionesList Devoluciones { get { return this._Devoluciones; } }

        public string Notas { get; set; }
        #endregion

        #region helpers
        private decimal DeudaPorCuotasImpagadas()
        {
            decimal deuda = 0;

            foreach (KeyValuePair<int, Cuota> kvp in this.Cuotas)
            {
                deuda += kvp.Value.GetDeuda();
            }

            return deuda;
        }
        /// <summary>
        /// Ingresos hasta el día de la fecha
        /// </summary>
        /// <param name="fechaFinal"></param>
        /// <returns></returns>
        private decimal DeudaPorCuotasImpagadas(Date fechaFinal)
        {
            decimal deuda = 0;

            foreach (KeyValuePair<int, Cuota> kvp in this.Cuotas)
            {
                if(kvp.Value.Mes <= fechaFinal) deuda += kvp.Value.GetDeuda();
            }

            return deuda;
        }
        /// <summary>
        /// Ingresos hasta el día de la fecha
        /// </summary>
        /// <param name="fechaInicial"></param>
        /// <param name="fechaFinal"></param>
        /// <returns></returns>
        private decimal DeudaPorCuotasImpagadas(Date fechaInicial, Date fechaFinal)
        {
            decimal deuda = 0;

            foreach (KeyValuePair<int, Cuota> kvp in this.Cuotas)
            {
                if (kvp.Value.Mes >= fechaInicial && kvp.Value.Mes <= fechaFinal)
                    deuda += kvp.Value.GetDeuda();
            }

            return deuda;
        }
        /// <summary>
        /// Ingresos hasta fechaIngresos
        /// </summary>
        /// <param name="fechaInicial"></param>
        /// <param name="fechaFinal"></param>
        /// <param name="fechaIngresos"></param>
        /// <returns></returns>
        private decimal DeudaPorCuotasImpagadas(Date fechaInicial, Date fechaFinal, Date fechaIngresos)
        {
            decimal deuda = 0;

            foreach (KeyValuePair<int, Cuota> kvp in this.Cuotas)
            {
                if (kvp.Value.Mes >= fechaInicial && kvp.Value.Mes <= fechaFinal)
                    deuda += kvp.Value.GetDeuda(fechaIngresos);
            }

            return deuda;
        }
        private decimal TotalEntregasACuentaAFecha(Date fechaInicial, Date fechaFinal)
        {
            decimal total = 0;

            foreach (KeyValuePair<int, EntACta> kvp in this.EntregasACuenta.GetEnumerable())
            {
                if (kvp.Value.Fecha <= fechaInicial && kvp.Value.Fecha >= fechaFinal)
                    total += kvp.Value.Importe;
            }
            foreach (IngresoDevuelto ingreso in this.Devoluciones.GetIngresosDevueltosEnumerable())
            {
                if (ingreso.Fecha <= fechaInicial && ingreso.Fecha >= fechaFinal)
                    total -= (ingreso.Importe + ingreso.Gastos);
            }

            return total;
        }
        #endregion

        #region public methods
        #region deuda methods
        public Dictionary<int,Cuota> GetCuotasImpagadas()
        {
            if (Baja) throw new CustomException_ObjModels("La finca está dada de baja");
            return this._Cuotas.Where(x => x.Value.GetDeuda() > 0) as Dictionary<int,Cuota>;
        }
        /// <summary>
        /// Ingresos hasta el día de la fecha
        /// </summary>
        /// <returns></returns>
        public decimal DeudaALaFecha()
        {
            if (Baja) throw new CustomException_ObjModels("La finca está dada de baja");
            return DeudaPorCuotasImpagadas() - this.EntregasACuenta.Total - Devoluciones.Total - Devoluciones.TotalGastos;
        }
        /// <summary>
        /// Ingresos hasta el día de la fecha
        /// </summary>
        /// <param name="fechaInicial"></param>
        /// <param name="fechaFinal"></param>
        /// <returns></returns>
        public decimal DeudaALaFecha(Date fechaInicial, Date fechaFinal)
        {
            if (Baja) throw new CustomException_ObjModels("La finca está dada de baja");
            return DeudaPorCuotasImpagadas(fechaInicial, fechaFinal) - TotalEntregasACuentaAFecha(fechaInicial, fechaFinal);
        }
        /// <summary>
        /// Ingresos hasta final de ejercicio
        /// </summary>
        /// <param name="ejercicio"></param>
        /// <returns></returns>
        public decimal DeudaALaFecha(Ejercicio ejercicio)
        {
            if (Baja) throw new CustomException_ObjModels("La finca está dada de baja");
            return DeudaPorCuotasImpagadas(ejercicio.FechaComienzo, ejercicio.FechaFinal) - 
                TotalEntregasACuentaAFecha(ejercicio.FechaComienzo, ejercicio.FechaFinal);
        }
        /// <summary>
        /// Ingresos hasta fechaIngresos
        /// </summary>
        /// <param name="fechaInicial"></param>
        /// <param name="fechaFinal"></param>
        /// <param name="fechaIngresos"></param>
        /// <returns></returns>
        public decimal DeudaALaFecha(Date fechaInicial, Date fechaFinal, Date fechaIngresos)
        {
            if (Baja) throw new CustomException_ObjModels("La finca está dada de baja");
            return DeudaPorCuotasImpagadas(fechaInicial, fechaFinal, fechaIngresos) - TotalEntregasACuentaAFecha(fechaInicial, fechaIngresos);
        }
        /// <summary>
        /// Llena deuda con un diccionario (idPropietario, cuotas con deuda) siguiendo los propietarios que aparecen en this.HistoricoPropietarios
        /// SIN ENTREGAS A CUENTA
        /// </summary>
        /// <param name="deuda"></param>
        public void DeudaPorPropietario(ref Dictionary<int, List<Cuota>> deuda, Date primeraFecha)
        {
            if (Baja) throw new CustomException_ObjModels("La finca está dada de baja");
            IOrderedEnumerable<KeyValuePair<DateTime, int>> orderedHistorico = this.HistoricoPropietarios.OrderBy(x => x.Key);
            
            foreach (KeyValuePair<DateTime, int> kvp in orderedHistorico)
            {
                deuda.Add(kvp.Value,
                    this.Cuotas.Where(x => x.Value.IdOwnerPropietario == kvp.Value && x.Value.GetDeuda() > 0)
                    .Select(x => x.Value)
                    .ToList<Cuota>());
            }
        }
        #endregion

        public bool TryRemoveCuota(int key)
        {
            if (!this._Cuotas.ContainsKey(key) || Baja) return false;

            this._Cuotas.Remove(key);
            return true;
        }
        public bool TryAddCuota(int key, ref Cuota cuota)
        {
            if (this._Cuotas.ContainsKey(key) || Baja) return false;

            this._Cuotas.Add(key, cuota);
            return true;
        }        

        #region propietario methods
        /// <summary>
        /// Cambio de propietario de la finca: 
        /// 1.- las cuotas desde fechaInicial a fechaFinal pasan a ser de newPropietario en vez de this.PropietarioAcutal
        /// 2.- se añade newPropietario a this.HistoricoPropietarios
        /// 3.- se cambia this._PropietarioActual por newPropietario
        /// </summary>
        /// <param name="cuotas"></param>
        /// <param name="newPropietario"></param>
        /// <param name="fechaInicial"></param>
        /// <param name="fechaFinal"></param>
        public void CambioPropietario(ref List<Cuota> cuotas, ref Propietario newPropietario, Date fechaInicial, Date fechaFinal)
        {
            if (Baja) throw new CustomException_ObjModels("La finca está dada de baja");
            this.PropietarioActual.RemoveCuotas(ref cuotas, fechaInicial, fechaFinal);
            newPropietario.AddCuotas(ref cuotas);
            this._HistoricoPropietarios.Add(fechaFinal.GetDateTime(), newPropietario.Id);
            this._PropietarioActual = newPropietario;
        }
        public string CambioNIFPropietario(string nif, bool forceNIF = false)
        {
            if (Baja) return "La finca está dada de baja.";
            string invalidMsg = this.PropietarioActual.NIF.TryModifyNIF(ref nif);

            if(invalidMsg != null && forceNIF)
                this.PropietarioActual.NIF.ForceInvalidNIF(ref nif);

            return invalidMsg;
        }
        public void CambioNombrePropietario(string nombre)
        {
            if (Baja) throw new CustomException_ObjModels("La finca está dada de baja");
            this.PropietarioActual.CambioNombrePropietario(nombre);
        }
        public bool DarDeBaja()
        {
            if (Baja) return false;
            this._Baja = true;
            return true;
        }
        public bool RecuperarBaja()
        {
            if (!Baja) return false;
            this._Baja = false;
            return true;
        }

        public bool TrySetCodigo(int codigo, ref List<int> codigos)
        {
            throw new NotImplementedException();
        }
        #endregion

        public int GetOwnerId()
        {
            return this.IdOwnerComunidad;
        }
        #endregion

        #region DLO
        public FincaDLO GetDLO()
        {
            return new FincaDLO(Id, IdOwnerComunidad, Baja, Nombre, Codigo.CurrentCodigo, PropietarioActual.Nombre, Telefono1.Numero, Email,
                IdAsociadas, Notas);
        }
        #endregion
    }

    public class FincaDLO : IObjModelBase, IOwnerComunidad, IDataListObject
    {
        public FincaDLO() { }
        public FincaDLO(
            int id,
            int idCdad,
            bool baja,
            string nombre,
            int codigo,
            string nombreProp,
            string telefono,
            string email,
            int[] idAsociadas,
            string notas)
        {
            this.Id = id;
            this.IdOwnerComunidad = idCdad;
            this.Baja = baja;
            this.Nombre = nombre;
            this.Codigo = codigo;
            this.NombrePropietarioActual = nombreProp;
            this.Telefono1 = telefono;
            this.Email = email;
            this.IdAsociadas = idAsociadas;
            this.Notas = notas;
        }

        public int Id { get; private set; }
        public int IdOwnerComunidad { get; private set; }
        public bool Baja { get; private set; }
        public string Nombre { get; private set; }
        public int Codigo { get; private set; }
        public string NombrePropietarioActual { get; private set; }
        public string Telefono1 { get; private set; }
        public string Email { get; private set; }
        public int[] IdAsociadas { get; private set; }
        public string Notas { get; private set; }
    }
}
