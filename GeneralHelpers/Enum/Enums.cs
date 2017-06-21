using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace AdConta
{
    #region data specific
    /// <summary>
    /// Tipo del nif: DNI persona física española, NIE extranjero, CIF persona jurídica
    /// </summary>
    public enum TipoNIF : int { DNI = 0, NIE, CIF, NULL}
    /// <summary>
    /// Tipo de teléfono
    /// </summary>
    public enum TipoTelefono : int { Principal, Secundario, Fax, Conyuge, Familiar, Trabajo, Movil, Fijo, Otros}
    /// <summary>
    /// Cómo paga las cuotas una finca: en efectivo por caja, ingreso/transf. bancaria, remesa.
    /// </summary>
    public enum TipoPagoCuotas : int { Caja = 0, IngTrf, Remesa}
    /// <summary>
    /// Efectivo, cheque, domiciliado
    /// </summary>
    public enum TipoPagoFacturas : int { Efectivo = 0, Cheque, Domiciliado}
    /// <summary>
    /// Enum for different parts of bank accounts.
    /// TODO?: añadir internacional?
    /// </summary>
    public enum AccountPart : int { IBAN = 0, Bank, Office, DC, Account }

    public enum SituacionReciboCobroEntaCta : int { Normal = 0, Devuelto}

    public enum TipoRepartoPresupuesto : int { Lineal = 0, SoloCoeficientes, CoeficientesYGrupos}

    public enum TipoGastoPago : int { Gasto = 0, Pago }
    /// <summary>
    /// Tipo de mensaje SQL para la clase AutoCodigo.
    /// </summary>
    public enum ACodigoSQLType { Next, Deleted, Check }
    /// <summary>
    /// Tipo de objeto en el que se realiza el chequeo de consistencia.
    /// </summary>
    public enum ACodigoCCheckType : int { All = 0, Comunidad, Fincas, Pptos, Asientos }
    #endregion

    #region accounting specific
    /// <summary>
    /// Debit/credit enum
    /// </summary>
    public enum DebitCredit
    {
        [DebitCreditAtttribute("False")]
        Debit = 0,
        [DebitCreditAtttribute("True")]
        Credit
    }
    [AttributeUsage(AttributeTargets.All)]
    public class DebitCreditAtttribute : DescriptionAttribute
    {
        public DebitCreditAtttribute(string description)
        {
            this.Description = bool.Parse(description);
        }

        public new bool Description { get; set; }
    }

    public enum TipoCuentaAcreedoraDeudora : int { Acreedora = 0, Deudora}
    #endregion

    #region app
    /// <summary>
    /// Enum for types of tabs that can be displayed in abletabcontrol.
    /// </summary>
    public enum TabType : int { None = 0, Mayor, Diario, Props, Cdad }
    /// <summary>
    /// Enum for different tabs that can be in TabMayor's bottom tabbed expander
    /// </summary>
    public enum TabExpTabType : int
    {
        NotExpandible = 0,
        Diario,
        Simple,
        Complejo,
        Mayor1_Cuenta,
        Mayor3_Buscar
    }
    /// <summary>
    /// Enum for specify top or bottom TabbedExpander
    /// </summary>
    public enum TabExpWhich : byte { Top = 0, Bottom}

    public enum ErrorSettingReciboDicts : int { None = 0, ImporteIncorrecto, VariasEACaMismaFinca}
    /// <summary>
    /// Enum of error trying to add/remove a range of objects to repository or/and DB:
    /// 
    /// </summary>
    public enum ErrorTryingDBRange : int {
        None = 0,
        DB_ObjectsEnumerableError,
        DB_NumberOfInsertedRows_LesserThanRange,
        DB_Other,
        Repo_ObjectsEnumerableError,
        Repo_Other }

    public enum ErrorCreatingObjModelInRepository : int { None = 0, ObjectAlreadyExistsInRepository, InsertToDBFails}

    public enum ErrorCuadreFactura : int
    {
        None = 0,
        [ErrorCuadreFacturaAtttribute(
            "Existe un error en la factura introducida: El importe del impuesto IGIC/IVA no concuerda con los datos introducidos.")]
        ErrorEnCalculoIGICIVA,
        [ErrorCuadreFacturaAtttribute(
            "Existe un error en la factura introducida: El importe del impuesto IRPF no concuerda con los datos introducidos.")]
        ErrorEnCalculoIRPF,
        [ErrorCuadreFacturaAtttribute(
            "Existe un error en la factura introducida: El importe total de la factura no concuerda con los datos introducidos.")]
        ErrorEnTotal,
        [ErrorCuadreFacturaAtttribute(
            "Existe un error en la factura introducida: El importe de los gastos introducidos para contabilizar no coincide con el subtotal de la factura.")]
        GastosNoCoincidenConSubtotal,
        [ErrorCuadreFacturaAtttribute(
            "Existe un error en la factura introducida: El importe de los pagos introducidos para contabilizar no coincide con el pendiente de la factura.")]
        PendienteDescuadrado
    }
    [AttributeUsage(AttributeTargets.All)]
    public class ErrorCuadreFacturaAtttribute : DescriptionAttribute
    {
        public ErrorCuadreFacturaAtttribute(string description)
        {
            this.Description = description;
        }

        public new string Description { get; set; }
    }
    
    public enum ConditionTCType { equal, diff, greater, lesser, greatOrEq, lessOrEq }
    #endregion
}