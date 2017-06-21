#define DEBUG

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using AdConta.Models;
using ModuloContabilidad.ObjModels;
using ModuloGestion.ObjModels;
using Mapper;
using Repository;
using Extensions;

namespace AdConta
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application, iAppRepositories
    {
        public App()
        {
            InitializeComponent();

            FrameworkElement.StyleProperty.OverrideMetadata(typeof(Window), new FrameworkPropertyMetadata
            {
                DefaultValue = FindResource(typeof(Window))
            });

            //*************************TODO: pide usuario y rellena propiedad
            this.UsuarioLogueado = new Usuario("yo", 0);
            //*************************
            
            this.ACData = new AutoCodigoData(this.UsuarioLogueado);
            Task.Run(() => ConfigMappersAsync()).Forget().ConfigureAwait(false);
            Task.Run(() => InitRepositoriesAsync()).Forget().ConfigureAwait(false);
            //Genera lista de objModels en excel
            //Propietario p = new Propietario(0, 1, "0", "hola", true);
            //NameSpaceObjectsList.NamespaceObjectsList objsList = new NameSpaceObjectsList.NamespaceObjectsList(
            //    new string[] { "ModuloContabilidad.ObjModels", "ModuloGestion.ObjModels", "AdConta.Models" });
            //    new string[] { "ModuloGestion.ObjModels" });
            //objsList.PrintTypesWithPropsFields(@"E:\GoogleDrive\Conta\_Diseño\ListaObjetosAutoGenerada3SoloGestion.xlsx", false, false, false);
            //this.PersonasRepository = new PersonaRepository();
        }

        #region properties
        public AutoCodigoData ACData { get; private set; }
        public Usuario UsuarioLogueado { get; private set; }
        #endregion

        #region repositories
        #region general
        public PersonaRepository PersonaRepo { get; private set; }
        public ComunidadRepository ComunidadRepo { get; private set; }
        #endregion

        #region contabilidad
        public CuentaMayorRepository CuentaMayorRepo { get; private set; }
        public AsientoRepository AsientoRepo { get; private set; }
        public ApunteRepository ApunteRepo { get; private set; }
        #endregion

        #region gestion
        public PropietarioRepository PropietarioRepo { get; private set; }
        #endregion
        #endregion

        protected override void OnStartup(StartupEventArgs e)
        {
            FrameworkElement.LanguageProperty.OverrideMetadata(
                typeof(FrameworkElement),
                new FrameworkPropertyMetadata(
                    System.Windows.Markup.XmlLanguage.GetLanguage(
                    System.Globalization.CultureInfo.CurrentCulture.IetfLanguageTag)));
            base.OnStartup(e);
        }

        #region helpers
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        private async Task InitRepositoriesAsync()
        {
            this.PersonaRepo = new PersonaRepository();
            this.ComunidadRepo = new ComunidadRepository();

            this.CuentaMayorRepo = new CuentaMayorRepository();
            this.AsientoRepo = new AsientoRepository();
            this.ApunteRepo = new ApunteRepository();

            this.PropietarioRepo = new PropietarioRepository();
        }
        private async Task ConfigMappersAsync()
        {
            string[] namespaces = new string[] { "ModuloContabilidad.ObjModels", "ModuloGestion.ObjModels", "AdConta.Models"};
            MapperConfig mConfig = new MapperConfig(namespaces);
            
            //Configuracion de todos los mappers:
            //Comunidad
            mConfig
                .AddConstructor<Comunidad>(x => new Comunidad(x.Id, x.CIF, x.Baja, x.Nombre, x.Codigo, this.ACData, true))
                .AddMemberCreator<Comunidad>("_CuentaBancaria1", x => new CuentaBancaria(x.CuentaBancaria))
                .AddMemberCreator<Comunidad>("_CuentaBancaria2", x => new CuentaBancaria(x.CuentaBancaria2))
                .AddMemberCreator<Comunidad>("_CuentaBancaria3", x => new CuentaBancaria(x.CuentaBancaria3))
                .AddNestedProperty<Comunidad, DireccionPostal>(false, x => x.Direccion)
                .AddNestedProperty<Comunidad, Persona>(false, x => x.Presidente, x => x.Secretario, x => x.Tesorero)
                .AddNestedProperty<Comunidad, HashSet<int>>(false, x => x.Vocales)
                .AddNestedProperty<Comunidad, Ejercicio>(false, x => x.EjercicioActivo)
                .EndConfig<Comunidad>();
            mConfig
                .AddConstructor<ComunidadDLO>(x => new ComunidadDLO(x.Id, x.Codigo, new NIFModel(x.CIF), x.Baja, x.Nombre, x.TipoVia + x.Direccion, 
                    x.CuentaBancaria, x.CuentaBancaria2, x.CuentaBancaria3, x.NombrePresidente, x.NombreSecretario, x.NombreTesorero, x.FechaPunteo,
                    x.UltimaFechaBanco))
                .MapOnlyConstructor<ComunidadDLO>()
                .EndConfig<ComunidadDLO>();
            //Ejercicio
            mConfig.EndConfig<Ejercicio>();
            mConfig
                .AddConstructor<EjercicioDLO>(x =>  new EjercicioDLO(x.Id, x.FechaComienzo, x.FechaFinal, x.IdOwnerComunidad, x.Cerrado))
                .MapOnlyConstructor<EjercicioDLO>()
                .EndConfig<EjercicioDLO>();
            //Persona
            mConfig
                .AddConstructor<Persona>(x => new Persona(x.Id, x.NIF, x.Nombre, true))
                .AddMemberCreator<Persona>("_CuentaBancaria", x => new CuentaBancaria(x.CuentaBancaria))
                .AddNestedProperty<Persona, DireccionPostal>(false, x => x.Direccion)
                .AddMemberCreator<Persona, sTelefono>(x => x.Telefono1, x => new sTelefono(x.Telefono, x.Tipo))
                .AddMemberCreator<Persona, sTelefono>(x => x.Telefono2, x => new sTelefono(x.Telefono2, x.Tipo2))
                .AddMemberCreator<Persona, sTelefono>(x => x.Telefono3, x => new sTelefono(x.Telefono3, x.Tipo3))
                .EndConfig<Persona>();
            //GrupoCuentas
            mConfig
                .AddConstructor<GrupoCuentas>(x => new GrupoCuentas(x.Id, x.IdOwnerComunidadNullable, x.Grupo))
                .EndConfig<GrupoCuentas>();
            //iGrupoGastos
            mConfig
                .AddInterfaceToObjectCondition<iGrupoGastos>(x => !(bool)x.Aceptado, typeof(GrupoGastos))
                .AddInterfaceToObjectCondition<iGrupoGastos>(x => (bool)x.Aceptado, typeof(GrupoGastosAceptado))
                .EndConfig<iGrupoGastos>();
            //GrupoGastos
            mConfig
                .AddDictionary<GrupoGastos>("_FincasCoeficientes", new string[] { "FincasCoeficientes", "Coeficiente" })
                .AddNestedProperty<GrupoGastos, List<Cuota>>(false, x => x.Cuotas)
                .AddNestedProperty<GrupoGastos>(false, "_Cuentas")
                .EndConfig<GrupoGastos>();
            mConfig
                .AddConstructor<GrupoGastosDLO>(x => new GrupoGastosDLO(x.Id, x.IdOwnerComunidad, x.IdPresupuesto, x.Nombre, 
                    x.CoeficientesCustom, x.Importe))
                .MapOnlyConstructor<GrupoGastosDLO>()
                .EndConfig<GrupoGastosDLO>();
            //GrupoGastosAceptado y structs
            mConfig.EndConfig<GrupoGastosAceptado.sDatosFincaGGAceptado>();
            mConfig.EndConfig<GrupoGastosAceptado.sDatosCuotaGGAceptado>();
            mConfig.EndConfig<GrupoGastosAceptado.sDatosCuentaGGAceptado>();
            mConfig
                .AddNestedProperty<GrupoGastosAceptado>(false, "_Fincas")
                .AddNestedProperty<GrupoGastosAceptado>(false, "_Cuotas")
                .AddNestedProperty<GrupoGastosAceptado>(false, "_Cuentas")
                .EndConfig<GrupoGastosAceptado>();
            //CuentaParaPresupuesto
            mConfig
                .AddNestedProperty<GrupoGastos.CuentaParaPresupuesto, CuentaMayor>(false, x => x.Cuenta)
                .EndConfig<GrupoGastos.CuentaParaPresupuesto>();
            //Presupuesto
            mConfig
                .AddConstructor<Presupuesto>(x => 
                    new Presupuesto(x.Id, x.IdOwnerComunidad, x.IdOwnerEjercicio, x.Codigo, this.ACData, x.Aceptado, (TipoRepartoPresupuesto)x.TipoReparto))
                .AddNestedProperty<Presupuesto>(true, "_GruposDeGasto")
                .EndConfig<Presupuesto>();
            mConfig
                .AddConstructor<PresupuestoDLO>(x => new PresupuestoDLO(x.Id, x.IdOwnerComunidad, x.Titulo, x.Total, x.Aceptado, 
                    (TipoRepartoPresupuesto)x.TipoReparto, x.Codigo))
                .MapOnlyConstructor<PresupuestoDLO>()
                .EndConfig<PresupuestoDLO>();
            //ObservableApuntesList
            mConfig
                .AddConstructor<ObservableApuntesList>(x =>
                {
                    var store = new MapperStore();
                    DapperMapper<Apunte> mapper = (DapperMapper<Apunte>)store.GetMapper(typeof(Apunte));
                    List<Apunte> lista = mapper.Map(x, "Id", false); //Obtiene lista de apuntes con sobrecarga de Map()

                    return new ObservableApuntesList(lista.First().Asiento, lista); //crea objeto a partir de esa lista
                })
                .MapOnlyConstructor<ObservableApuntesList>()
                .EndConfig<ObservableApuntesList>();
            //Apunte
            mConfig
                .AddNestedProperty<Apunte>(false, "_Asiento")
                .AddMemberCreator<Apunte>("_DebeHaber", x => (DebitCredit)x.DebeHaber)
                .AddPrefixes<Apunte>(new string[] { "apu" })
                .EndConfig<Apunte>();
            mConfig
                .AddConstructor<ApunteDLO>(x => new ApunteDLO(x.Id, x.IdOwnerComunidad, x.OrdenEnAsiento, x.Asiento, x.Concepto, x.DebeHaber,
                    x.Importe, x.IdCuenta, x.Punteo, x.Factura))
                .MapOnlyConstructor<ApunteDLO>()
                .EndConfig<ApunteDLO>();
            //Asiento
            mConfig
                .AddConstructor<Asiento>(x => new Asiento(x.Id, x.IdOwnerComunidad, x.IdOwnerEjercicio, x.Codigo, this.ACData, x.FechaValor))
                .AddNestedProperty<Asiento, ObservableApuntesList>(false, x => x.Apuntes)
                .AddIgnoreProperty<Asiento>("Item")
                .AddPrefixes<Asiento>(new string[] { "asi" })
                .EndConfig<Asiento>();
            //Gasto-Pago-GastosPagosBase
            mConfig
                .AddPrefixes<GastosPagosBase.sImporteCuenta>(new string[] { "acreedora_", "deudora_"})
                .EndConfig<GastosPagosBase.sImporteCuenta>();
            mConfig
                .AddConstructor<GastosPagosBase>(x => new GastosPagosBase(x.Id, x.IdOwnerComunidad, x.IdProveedor, x.IdOwnerFactura, x.Fecha))
                .AddMemberCreator<GastosPagosBase>("_CuentasAcreedoras", x=>
                {
                    MapperStore store = new MapperStore();
                    DapperMapper<GastosPagosBase.sImporteCuenta> mapper =
                        (DapperMapper<GastosPagosBase.sImporteCuenta>)store.GetMapper(typeof(GastosPagosBase.sImporteCuenta));
                    IEnumerable<dynamic> distinctAcreedX = mapper.GetDistinctDapperResult(x, false);
                    distinctAcreedX = distinctAcreedX.Select(dyn =>
                    {
                        IDictionary<string, object> dict = dyn as IDictionary<string, object>;
                        var result = new System.Dynamic.ExpandoObject() as IDictionary<string, object>;
                        foreach (KeyValuePair<string, object> kvp in dict)
                            if (kvp.Key.Contains("acreedora_")) result.Add(kvp.Key, kvp.Value);

                        return result;
                    });
                    
                    return mapper.Map<List<GastosPagosBase.sImporteCuenta>>(distinctAcreedX, "Id", false);
                })
                .AddMemberCreator<GastosPagosBase>("_CuentasDeudoras", x =>
                {
                    MapperStore store = new MapperStore();
                    DapperMapper<GastosPagosBase.sImporteCuenta> mapper =
                        (DapperMapper<GastosPagosBase.sImporteCuenta>)store.GetMapper(typeof(GastosPagosBase.sImporteCuenta));
                    IEnumerable<dynamic> distinctDeudX = mapper.GetDistinctDapperResult(x, false);
                    distinctDeudX = distinctDeudX.Select(dyn =>
                    {
                        IDictionary<string, object> dict = dyn as IDictionary<string, object>;
                        var result = new System.Dynamic.ExpandoObject() as IDictionary<string, object>;
                        foreach (KeyValuePair<string, object> kvp in dict)
                            if (kvp.Key.Contains("deudora_")) result.Add(kvp.Key, kvp.Value);

                        return result;
                    });

                    return mapper.Map<List<GastosPagosBase.sImporteCuenta>>(distinctDeudX, "Id", false);
                })
                //.AddIgnoreProperty<GastosPagosBase>("_CuentasAcreedoras")
                //.AddIgnoreProperty<GastosPagosBase>("_CuentasDeudoras")
                //.AddIgnoreProperty<GastosPagosBase, System.Collections.ObjectModel.ReadOnlyCollection<GastosPagosBase.sImporteCuenta>>(
                //    x => x.CuentasAcreedoras)
                //.AddIgnoreProperty<GastosPagosBase, System.Collections.ObjectModel.ReadOnlyCollection<GastosPagosBase.sImporteCuenta>>(
                //    x => x.CuentasDeudoras)
                .EndConfig<GastosPagosBase>();
            mConfig
                .AddConstructor<Gasto>(x => new Gasto(x.Id, x.IdOwnerComunidad, x.IdProveedor, x.IdOwnerFactura, x.Fecha))
                .EndConfig<Gasto>();
            mConfig
                .AddConstructor<Pago>(x => new Pago(x.Id, x.IdOwnerComunidad, x.IdProveedor, x.IdOwnerFactura, x.Fecha))
                .EndConfig<Pago>();
            //GastosPagosList-ReadOnly
            mConfig
                .AddConstructor<GastosPagosList<Gasto>>(x =>
                {
                    MapperStore store = new MapperStore();
                    DapperMapper<Gasto> mapper = (DapperMapper<Gasto>)store.GetMapper(typeof(Gasto));
                    List<Gasto> lista = mapper.Map<List<Gasto>>(x, "Id", false);

                    return new GastosPagosList<Gasto>(lista);
                })
                .MapOnlyConstructor<GastosPagosList<Gasto>>()
                .EndConfig<GastosPagosList<Gasto>>();
            mConfig
                .AddConstructor<GastosPagosList<Pago>>(x =>
                {
                    MapperStore store = new MapperStore();
                    DapperMapper<Pago> mapper = (DapperMapper<Pago>)store.GetMapper(typeof(Pago));
                    List<Pago> lista = mapper.Map<List<Pago>>(x, "Id", false);

                    return new GastosPagosList<Pago>(lista);
                })
                .MapOnlyConstructor<GastosPagosList<Pago>>()
                .EndConfig<GastosPagosList<Pago>>();
            mConfig
                .AddConstructor<ReadOnlyGastosPagosList<Gasto>>(x =>
                {
                    MapperStore store = new MapperStore();
                    DapperMapper<Gasto> mapper = (DapperMapper<Gasto>)store.GetMapper(typeof(Gasto));
                    List<Gasto> lista = mapper.Map<List<Gasto>>(x, "Id", false);

                    return new ReadOnlyGastosPagosList<Gasto>(lista);
                })
                .MapOnlyConstructor<ReadOnlyGastosPagosList<Gasto>>()
                .EndConfig<ReadOnlyGastosPagosList<Gasto>>();
            mConfig
                .AddConstructor<ReadOnlyGastosPagosList<Pago>>(x =>
                {
                    MapperStore store = new MapperStore();
                    DapperMapper<Pago> mapper = (DapperMapper<Pago>)store.GetMapper(typeof(Pago));
                    List<Pago> lista = mapper.Map<List<Pago>>(x, "Id", false);

                    return new ReadOnlyGastosPagosList<Pago>(lista);
                })
                .MapOnlyConstructor<ReadOnlyGastosPagosList<Pago>>()
                .EndConfig<ReadOnlyGastosPagosList<Pago>>();
            //Factura
            mConfig
                .AddNestedProperty<Factura>(false, "_GastosFra")
                .AddIgnoreProperty<Factura, ReadOnlyGastosPagosList<Gasto>>(x => x.GastosFra)
                .AddNestedProperty<Factura>(false, "_PagosFra")
                .AddIgnoreProperty<Factura, ReadOnlyGastosPagosList<Pago>>(x => x.PagosFra)
                .AddMemberCreator<Factura, int?>(x => x.IdOwnerProveedor, x => x.IdProveedor)
                .AddMemberCreator<Factura, TipoPagoFacturas>(x => x.TipoPago, x => (TipoPagoFacturas)x.TipoPago)
                .AddIgnoreProperty<Factura, decimal>(x => x.TotalImpuestos)
                .EndConfig<Factura>();
            mConfig
                .AddConstructor<FacturaDLO>(x => new FacturaDLO(x.Id, x.IdProveedor, x.IdOwnerComunidad, x.NFactura, x.Fecha, x.Concepto, x.Total, 
                    x.Pendiente, (TipoPagoFacturas)x.TipoPago))
                .MapOnlyConstructor<FacturaDLO>()
                .EndConfig<FacturaDLO>();
            //CuentaMayor
            mConfig
                .AddConstructor<CuentaMayor>(x => new CuentaMayor(x.Codigo, x.Id, x.IdOwnerComunidad, x.IdOwnerEjercicio, x.Nombre))
                .MapOnlyConstructor<CuentaMayor>()
                .EndConfig<CuentaMayor>();
            mConfig
                .AddConstructor<CuentaMayorDLO>(x => new CuentaMayorDLO(int.Parse(x.Codigo), x.Id, x.IdOwnerComunidad, x.IdOwnerEjercicio, x.Nombre))
                .MapOnlyConstructor<CuentaMayorDLO>()
                .EndConfig<CuentaMayorDLO>();
            //Proveedor
            mConfig
                .AddConstructor<Proveedor>(x => new Proveedor(x.Id, x.IdPersona, x.NIF, x.Nombre, true))
                .AddNestedProperty<Proveedor>(false, "_CuentaContableGasto")
                .AddNestedProperty<Proveedor>(false, "_CuentaContablePago")
                .AddNestedProperty<Proveedor>(false, "_CuentaContableProveedor")
                .AddMemberCreator<Proveedor, TipoPagoFacturas>(x => x.DefaultTipoPagoFacturas, x => (TipoPagoFacturas)x.DefaultTipoPagoFacturas)
                .EndConfig<Proveedor>();
            mConfig
                .AddConstructor<ProveedorDLO>(x => new ProveedorDLO(x.Id, x.Nombre, x.NIF, string.Concat(x.TipoVia, " ", 
                    x.Direccion), x.CuentaBancaria, x.Telefono, x.Email, x.RazonSocial, x.CuentaContableGasto, x.CuentaContablePago, 
                    x.CuentaContableProveedor))
                .MapOnlyConstructor<ProveedorDLO>()
                .EndConfig<ProveedorDLO>();
            //Cobros-EntACta-iIngresoPropietario
            mConfig
                .AddInterfaceToObjectCondition<iIngresoPropietario>(x =>
                {
                    var dict = x as IDictionary<string, object>;
                    return dict.ContainsKey("IdOwnerCuota");
                },
                typeof(Cobro))
                .AddInterfaceToObjectCondition<iIngresoPropietario>(x =>
                {
                    var dict = x as IDictionary<string, object>;
                    return dict.ContainsKey("IdOwnerFinca");
                },
                typeof(EntACta))
                .EndConfig<iIngresoPropietario>();
            mConfig
                .AddConstructor<Cobro>(x => new Cobro(
                    x.Id, x.IdOwnerRecibo, x.IdOwnerCuota, x.Importe, x.Fecha, x.IdOwnerPersona, x.Total, (SituacionReciboCobroEntaCta)x.Situacion))
                .AddPrefixes<Cobro>(new string[1] { "cobro" })
                .MapOnlyConstructor<Cobro>()
                .EndConfig<Cobro>();
            mConfig
                .AddConstructor<EntACta>(x => new EntACta(
                    x.Id, x.IdOwnerRecibo, x.IdOwnerFinca, x.Importe, x.Fecha, x.IdOwnerPersona, (SituacionReciboCobroEntaCta)x.Situacion))
                .AddPrefixes<EntACtaDict>(new string[1] { "eacta" })
                .MapOnlyConstructor<Cobro>()
                .EndConfig<EntACta>();
            //Cobros/EntACta - List/Dict
            mConfig
                .AddConstructor<CobrosList>(x =>
                {
                    MapperStore store = new MapperStore();
                    DapperMapper<Cobro> mapper = (DapperMapper<Cobro>)store.GetMapper(typeof(Cobro));
                    List<Cobro> lista = mapper.Map<List<Cobro>>(x, "Id", false);

                    return new CobrosList(lista);
                })
                .MapOnlyConstructor<CobrosList>()
                .EndConfig<EntACtaList>();
            mConfig
                .AddConstructor<CobrosDict>(x =>
                {
                    MapperStore store = new MapperStore();
                    DapperMapper<Cobro> mapper = (DapperMapper<Cobro>)store.GetMapper(typeof(Cobro));
                    IEnumerable<Cobro> lista = mapper.Map<IEnumerable<Cobro>>(x);

                    return new CobrosDict(lista.ToDictionary(c => (int)c.Id, c => c));
                })
                .MapOnlyConstructor<CobrosDict>()
                .EndConfig<CobrosDict>();
            mConfig
                .AddConstructor<EntACtaList>(x =>
                {
                    MapperStore store = new MapperStore();
                    DapperMapper<EntACta> mapper = (DapperMapper<EntACta>)store.GetMapper(typeof(EntACta));
                    List<EntACta> lista = mapper.Map<List<EntACta>>(x, "Id", false);

                    return new EntACtaList(lista);
                })
                .MapOnlyConstructor<EntACtaList>()
                .EndConfig<EntACtaList>();
            mConfig
                .AddConstructor<EntACtaDict>(x =>
                {
                    MapperStore store = new MapperStore();
                    DapperMapper<EntACta> mapper = (DapperMapper<EntACta>)store.GetMapper(typeof(EntACta));
                    IEnumerable<EntACta> lista = mapper.Map<IEnumerable<EntACta>>(x);

                    return new EntACtaDict(lista.ToDictionary(c => (int)c.Id, c => c));
                })
                .MapOnlyConstructor<EntACtaDict>()
                .EndConfig<EntACtaDict>();
            //IngresoDevuelto
            mConfig
                .AddConstructor<IngresoDevuelto>(x =>
                {
                    MapperStore store = new MapperStore();
                    DapperMapper<iIngresoPropietario> mapper = (DapperMapper<iIngresoPropietario>)store.GetMapper(typeof(iIngresoPropietario));
                    var ingreso = mapper.Map(x);
                    return new IngresoDevuelto(x.Id, x.IdOwnerDevolucion, x.Fecha, ingreso, x.Total, x.Importe, x.Gastos);
                })
                .MapOnlyConstructor<IngresoDevuelto>()
                .EndConfig<IngresoDevuelto>();
            //Devolucion-List
            mConfig
                .AddConstructor<Devolucion>(x =>
                {
                    MapperStore store = new MapperStore();
                    DapperMapper<IngresoDevuelto> mapper = (DapperMapper<IngresoDevuelto>)store.GetMapper(typeof(IngresoDevuelto));
                    List<IngresoDevuelto> lista = mapper.Map<List<IngresoDevuelto>>(x);

                    return new Devolucion(x.Id, x.IdOwnerComunidad, x.Fecha, lista);
                })
                .MapOnlyConstructor<Devolucion>()
                .EndConfig<Devolucion>();
            mConfig
                .AddConstructor<DevolucionesList>(x =>
                {
                    MapperStore store = new MapperStore();
                    DapperMapper<Devolucion> mapper = (DapperMapper<Devolucion>)store.GetMapper(typeof(Devolucion));
                    IEnumerable<Devolucion> lista = mapper.Map<IEnumerable<Devolucion>>(x);

                    return new DevolucionesList(lista.ToList());
                })
                .MapOnlyConstructor<DevolucionesList>()
                .EndConfig<DevolucionesList>();
            //Cuota
            mConfig
                .AddNestedProperty<Cuota, Ejercicio>(false, x => x.Ejercicio)
                .AddNestedProperty<Cuota, Concepto>(false, x => x.Concepto)
                .AddNestedProperty<Cuota, CobrosDict>(false, x => x.Cobros)
                .AddMemberCreator<Cuota, SituacionReciboCobroEntaCta>(x => x.Situacion, x => (SituacionReciboCobroEntaCta)x.Situacion)
                .AddNestedProperty<Cuota, DevolucionesList>(false, x => x.Devoluciones)
                .AddPrefixes<Cuota>(new string[1] { "cuota" })
                .EndConfig<Cuota>();
            //Propietario
            mConfig
                .AddConstructor<Propietario>(x => new Propietario(x.Id, x.IdOwnerComunidad, x.NIF, x.Nombre, true))
                .AddNestedProperty<Propietario>(false, "_Cuotas")
                .AddDictionary<Propietario>("_Cuotas", new string[2] { "cuotaId", "Cuota" })
                .EndConfig<Propietario>();
            mConfig
                .AddConstructor<PropietarioDLO>(x => new PropietarioDLO(x.Id, x.IdOwnerComunidad, x.Nombre, x.NIF, x.Direccion, x.CuentaBancaria, 
                    x.Telefono, x.Email))
                .MapOnlyConstructor<PropietarioDLO>()
                .EndConfig<PropietarioDLO>();
            //Finca
            mConfig
                .AddConstructor<Finca>(x => new Finca(x.Id, x.IdOwnerComunidad, x.Baja, x.Nombre, x.Coeficiente, x.Codigo, this.ACData))
                .AddNestedProperty<Finca>(false, "_PropietarioActual")
                .AddDictionary<Finca>("_HistoricoPropietarios", new string[2] { "FechaFinal", "IdPropietario" })
                .AddNestedProperty<Finca>(false, "_Cuotas")
                .AddDictionary<Finca>("_Cuotas", new string[2] { "cuotaId", "Cuota" })
                .AddNestedProperty<Finca>(false, "_EntregasACuenta")
                .AddNestedProperty<Finca>(false, "_Devoluciones")
                .AddNestedProperty<Finca, DireccionPostal>(false, x => x.Direccion)
                .AddMemberCreator<Finca, DireccionPostal>(x => x.Direccion2,
                    x => new DireccionPostal(x.TipoVia2, x.Direccion2, x.CP2, x.Localidad2, x.Provincia2))
                .AddMemberCreator<Finca, TipoPagoCuotas>(x => x.DefaultTipoPagoCuotas, x => (TipoPagoCuotas)x.DefaultTipoPagoCuotas)
                .AddMemberCreator<Finca, sTelefono>(x => x.Telefono1, x => new sTelefono(x.Telefono, x.TipoTelefono))
                .AddMemberCreator<Finca, sTelefono>(x => x.Telefono2, x => new sTelefono(x.Telefono2, x.TipoTelefono2))
                .AddMemberCreator<Finca, sTelefono>(x => x.Telefono3, x => new sTelefono(x.Telefono3, x.TipoTelefono3))
                .AddMemberCreator<Finca, sTelefono>(x => x.Fax, x => new sTelefono(x.Fax, TipoTelefono.Fax))
                .AddMemberCreator<Finca, int[]>(x=>x.IdAsociadas, x=>
                {
                    IEnumerable<dynamic> ex = (IEnumerable<dynamic>)x;
                    return ex
                        .Select(dyn => dyn.IdFincaAsociada)
                        .Distinct()
                        .ToArray();
                })
                .AddMemberCreator<Finca>("_IdCopropietarios", x =>
                {
                    IEnumerable<dynamic> ex = (IEnumerable<dynamic>)x;
                    return ex
                        .Select(dyn => dyn.IdPersonaCoProp)
                        .Distinct()
                        .ToArray();
                })
                .AddMemberCreator<Finca>("_IdPagadores", x =>
                {
                    IEnumerable<dynamic> ex = (IEnumerable<dynamic>)x;
                    return ex
                        .Select(dyn => new Tuple<int, TipoPagoCuotas>(dyn.IdPersonaCoPag, (TipoPagoCuotas)dyn.TipoPagoCuotas))
                        .Distinct()
                        .ToArray();
                })
                .EndConfig<Finca>();
            mConfig
                .AddConstructor<FincaDLO>(x =>
                {
                    IEnumerable<dynamic> ex = (IEnumerable<dynamic>)x;
                    int[] asociadas = ex
                        .Select(dyn => (int)dyn.IdFincaAsociada)
                        .Distinct()
                        .ToArray();
                    FincaDLO instance = new FincaDLO(x.Id, x.IdOwnerComunidad, x.Baja, x.Nombre, x.Codigo, x.NombreProp, x.Telefono, x.Email, 
                        asociadas, x.Notas);
                    return instance;
                })
                .MapOnlyConstructor<FincaDLO>()
                .EndConfig<FincaDLO>();
            //Recibo
            mConfig
                .AddNestedProperty<Recibo>(false, "_Cobros")
                .AddNestedProperty<Recibo>(false, "_EntregasACuenta")
                .EndConfig<Recibo>();
        }
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        #endregion

        /*protected override void OnExit(ExitEventArgs e)
        {
            this._AppModelControl.UnsubscribeModelControlEvents();
            base.OnExit(e);
        }*/
    }
}
