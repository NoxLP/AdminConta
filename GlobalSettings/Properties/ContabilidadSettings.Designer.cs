﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace GlobalSettings.Properties {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "14.0.0.0")]
    public sealed partial class ContabilidadSettings : global::System.Configuration.ApplicationSettingsBase {
        
        private static ContabilidadSettings defaultInstance = ((ContabilidadSettings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new ContabilidadSettings())));
        
        public static ContabilidadSettings Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("¿Desea cancelar el asiento (el asiento se borrará y perderá toda la información) " +
            "o volver a la ventana sin guardar el asiento?")]
        public string MENSAJESERROR_ASIENTO_ERRORALGUARDARASIENTO {
            get {
                return ((string)(this["MENSAJESERROR_ASIENTO_ERRORALGUARDARASIENTO"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("El asiento no se ha guardado, si cierra esta pestaña perderá los cambios que haya" +
            " realizado hasta ahora. ¿Quiere cerrar esta pestaña de todas formas?")]
        public string MENSAJESCONFIRMACION_ASIENTO_CONFIRMACIONCERRARASIENTOSINGUARDAR {
            get {
                return ((string)(this["MENSAJESCONFIRMACION_ASIENTO_CONFIRMACIONCERRARASIENTOSINGUARDAR"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("Parece que no está haciendo un Asiento Simple. ¿Desea completar el asiento en la " +
            "vista de Asiento Complejo?")]
        public string MENSAJESCONFIRMACION_ASIENTO_CONFIRMACIONSIMPLEACOMPLEJO {
            get {
                return ((string)(this["MENSAJESCONFIRMACION_ASIENTO_CONFIRMACIONSIMPLEACOMPLEJO"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("Aquí debe introducir un importe numérico, no puede escribir letras o símbolos sal" +
            "vo coma (,) o punto (.) para separar los decimales.")]
        public string MENSAJESERROR_ASIENTO_LETRASENCELDAIMPORTE {
            get {
                return ((string)(this["MENSAJESERROR_ASIENTO_LETRASENCELDAIMPORTE"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("Este asiento no se puede modificar por que la contabilidad está cerrada.")]
        public string MENSAJESERROR_ASIENTO_ASIENTOCERRADO {
            get {
                return ((string)(this["MENSAJESERROR_ASIENTO_ASIENTOCERRADO"]));
            }
        }
    }
}
