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
    public sealed partial class Settings : global::System.Configuration.ApplicationSettingsBase {
        
        private static Settings defaultInstance = ((Settings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new Settings())));
        
        public static Settings Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("SELECT * FROM Cdades")]
        public string SELECTCDADES {
            get {
                return ((string)(this["SELECTCDADES"]));
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("7")]
        public int DIGITOSCUENTAS {
            get {
                return ((int)(this["DIGITOSCUENTAS"]));
            }
            set {
                this["DIGITOSCUENTAS"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("0")]
        public int MINCODCUENTAS {
            get {
                return ((int)(this["MINCODCUENTAS"]));
            }
            set {
                this["MINCODCUENTAS"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("0")]
        public int MAXCODCUENTAS {
            get {
                return ((int)(this["MAXCODCUENTAS"]));
            }
            set {
                this["MAXCODCUENTAS"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("13")]
        public double USERFONTSIZE {
            get {
                return ((double)(this["USERFONTSIZE"]));
            }
            set {
                this["USERFONTSIZE"] = value;
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("SELECT * FROM")]
        public string SELECTDIARIO {
            get {
                return ((string)(this["SELECTDIARIO"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("ORDER BY NApunte")]
        public string ORDERDIARIO {
            get {
                return ((string)(this["ORDERDIARIO"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("SELECT * FROM")]
        public string SELECTMAYOR {
            get {
                return ((string)(this["SELECTMAYOR"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("ORDER BY NAsiento, Fecha, FechaValor")]
        public string ORDERMAYOR {
            get {
                return ((string)(this["ORDERMAYOR"]));
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("5720001")]
        public string CUENTADEFAULT {
            get {
                return ((string)(this["CUENTADEFAULT"]));
            }
            set {
                this["CUENTADEFAULT"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool ASIENTOSIMPLE_WINDOWED {
            get {
                return ((bool)(this["ASIENTOSIMPLE_WINDOWED"]));
            }
            set {
                this["ASIENTOSIMPLE_WINDOWED"] = value;
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("WHERE Baja=0")]
        public string WHERECDADES_NOBAJA {
            get {
                return ((string)(this["WHERECDADES_NOBAJA"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("SELECT * FROM Props")]
        public string SELECTPROPS {
            get {
                return ((string)(this["SELECTPROPS"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("ORDER BY Dni")]
        public string ORDERPROPS {
            get {
                return ((string)(this["ORDERPROPS"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.SpecialSettingAttribute(global::System.Configuration.SpecialSetting.ConnectionString)]
        [global::System.Configuration.DefaultSettingValueAttribute("Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=e:\\AdConta\\AdConta\\AdConta\\co" +
            "nta1.mdf;Integrated Security=True")]
        public string conta1ConnectionString {
            get {
                return ((string)(this["conta1ConnectionString"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("ORDER BY Codigo")]
        public string ORDERCDADES {
            get {
                return ((string)(this["ORDERCDADES"]));
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("Banco")]
        public string NOMBRECUENTADEFAULT {
            get {
                return ((string)(this["NOMBRECUENTADEFAULT"]));
            }
            set {
                this["NOMBRECUENTADEFAULT"] = value;
            }
        }
    }
}