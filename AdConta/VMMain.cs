using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using ModuloContabilidad;
using ModuloGestion;
using TabbedExpanderCustomControl;
using Extensions;
using AdConta.ViewModel;
using AdConta.Models;

namespace AdConta
{
    /// <summary>
    /// Application main view-model.
    /// </summary>
    public class VMMain : ViewModelBase
    {
        public VMMain()
        {
            Tabs = new ObservableCollection<aVMTabBase>();
            this.SetMinMaxLedgeAccountsCod();
        }

        #region fields
        private ObservableCollection<aVMTabBase> _Tabs;
        private int _zIndex = 1;
        #endregion

        #region properties
        /// <summary>
        /// Task ejecutada por App.xaml.cs para inicializar programa: repositorios, mappers, logueo, etc.
        /// </summary>
        public NotifyTask InitTask { get; private set; }
        /// <summary>
        /// Collection Abletabcontrol tabs. Binded here.
        /// </summary>
        public ObservableCollection<aVMTabBase> Tabs
        {
            get { return this._Tabs; }
            set
            {
                if (this._Tabs != value)
                    this._Tabs = value;
            }
        }
        /// <summary>
        /// Llega a traves de MainWindow con OneWayToSource.
        /// </summary>
        public int SelectedTab { get; set; }
        /// <summary>
        /// Código de la comunidad de la última pestaña añadida. Owner.
        /// </summary>
        public int LastTabCodigoComunidad { get; private set; }
        /// <summary>
        /// Código del ejercicio de la última pestaña añadida. Owner.
        /// </summary>
        public int LastTabCodigoEjercicio { get; private set; }
        /// <summary>
        /// Tipo de la última pestaña añadida.
        /// </summary>
        public TabType LastTabType { get; private set; }
        /// <summary>
        /// Si esto es false se ha presionado un botón de comunidad de la SideTool y no se ha abierto pestaña, por tanto hay que abrir pestaña nueva.
        /// Si se presiona un botón de ejercicio siendo esto true, ya hay una pestaña abierta, por tanto solo cambia el ejercicio.
        /// </summary>
        public bool TabComunidadClickResuelto { get; private set; }

        /// <summary>
        /// Side tool zIndex.
        /// </summary>
        public int ZIndex
        {
            get { return this._zIndex; }
            set
            {
                if (this._zIndex != value)
                {
                    this._zIndex = value;
                    this.NotifyPropChanged("ZIndex");
                }
            }
        }
        #endregion

        #region helpers
        public void SetMinMaxLedgeAccountsCod()
        {
            int digits = GlobalSettings.Properties.Settings.Default.DIGITOSCUENTAS;
            int sufDigits = GlobalSettings.Properties.Settings.Default.DIGITOSCUENTAS - 3;
            int min = (int)Math.Truncate(Math.Pow(10, digits - 1)) + 1;
            int max = ((int)Math.Truncate(Math.Pow(10, digits)) - 1) - ((int)Math.Truncate(Math.Pow(10, sufDigits)) * 5);

            GlobalSettings.Properties.Settings.Default.MINCODCUENTAS = min;
            GlobalSettings.Properties.Settings.Default.MAXCODCUENTAS = max;
        }
        private string BuildTabEjercicioHeader(EjercicioDLOParaSideTool DLO)
        {
            DateTime comienzo = DateTime.ParseExact(DLO.FechaComienzo, "d", null);
            DateTime final = DateTime.ParseExact(DLO.FechaFinal, "d", null);
            string tabEjercicioHeader = "";
            //Regla de 3: 12 meses son M para 4(trimestre) meses son X => 12/M = 4/X => X = 4M/12 (si se utiliza el último mes no es necesario redondear)
            //para cuatrimestres y semestres es lo mismo cambiando el 4
            if (comienzo.AreAnOfficialTrimester(final))
                tabEjercicioHeader = $"{(4 * final.Month) / 12} Trimestre {final.Year}";
            else if (comienzo.AreAnOfficialFourMonth(final))
                tabEjercicioHeader = $"{(3 * final.Month) / 12} Cuatrimestre {final.Year}";
            else if (comienzo.AreAnOfficialSemester(final))
                tabEjercicioHeader = $"{(2 * final.Month) / 12} Semestre {final.Year}";
            else if (comienzo.Year == final.Year)
                tabEjercicioHeader = comienzo.Year.ToString();
            else
                tabEjercicioHeader = $"{DLO.FechaComienzo} a {DLO.FechaFinal}";
            return tabEjercicioHeader;
        }
        private string BuildTabEjercicioHeader(Ejercicio ejercicio)
        {
            DateTime comienzo = ejercicio.FechaComienzo;
            DateTime final = ejercicio.FechaFinal;
            string tabEjercicioHeader = "";
            //Regla de 3: 12 meses son M para 4(trimestre) meses son X => 12/M = 4/X => X = 4M/12 (si se utiliza el último mes no es necesario redondear)
            //para cuatrimestres y semestres es lo mismo cambiando el 4
            if (comienzo.AreAnOfficialTrimester(final))
                tabEjercicioHeader = $"{(4 * final.Month) / 12} Trimestre {final.Year}";
            else if (comienzo.AreAnOfficialFourMonth(final))
                tabEjercicioHeader = $"{(3 * final.Month) / 12} Cuatrimestre {final.Year}";
            else if (comienzo.AreAnOfficialSemester(final))
                tabEjercicioHeader = $"{(2 * final.Month) / 12} Semestre {final.Year}";
            else if (comienzo.Year == final.Year)
                tabEjercicioHeader = comienzo.Year.ToString();
            else
                tabEjercicioHeader = $"{comienzo.ToString("d")} a {final.ToString("d")}";
            return tabEjercicioHeader;
        }
        public string BuildTabHeader(int codigoComunidad, EjercicioDLOParaSideTool DLO, TabType tabType)
        {
            string tabEjercicioHeader = BuildTabEjercicioHeader(DLO);
            TabHeader TabHeaders = new TabHeader();

            return $"{codigoComunidad} - {tabEjercicioHeader} - {TabHeaders[this.LastTabType]}";
        }
        public string BuildTabHeader(int codigoComunidad, Ejercicio ejercicio, TabType tabType)
        {
            string tabEjercicioHeader = BuildTabEjercicioHeader(ejercicio);
            TabHeader TabHeaders = new TabHeader();
            return $"{codigoComunidad} - {tabEjercicioHeader} - {TabHeaders[this.LastTabType]}";
        }
        #endregion

        #region public methods
        public void SetNewTabCodigoComunidad(int comCod, TabType type)
        {
            this.LastTabCodigoComunidad = comCod;
            this.LastTabType = type;
            this.TabComunidadClickResuelto = false;
            Messenger.Messenger.RegisterMsg("LastComCod", comCod);
        }
        public void SetNewTabEjercicio(EjercicioDLOParaSideTool DLO)
        {
            this.LastTabCodigoEjercicio = DLO.Id;
            Messenger.Messenger.RegisterMsg("LastEjerCod", DLO.Id);

            if (!this.TabComunidadClickResuelto)
            {
                this.TabComunidadClickResuelto = true;
                AddTab(DLO);
            }
            else
            {
                var selectedTab = this.Tabs[this.SelectedTab];
                selectedTab.TabCodigoEjercicio = DLO.Id;
                selectedTab.Header = BuildTabHeader(selectedTab.TabCodigoComunidad, DLO, selectedTab.TabType);
            }
        }
        /// <summary>
        /// Add new tab of type to abletabcontrol.
        /// </summary>
        /// <param name="type">See enum</param>
        public void AddTab(EjercicioDLOParaSideTool DLO)
        {
            aVMTabBase tab;
            AbleTabControl.AbleTabControl ATC = (App.Current.MainWindow as MainWindow).AbleTabControl;
            TabbedExpander TopTabExp = ATC.FindVisualChild<TabbedExpander>(x => (x as TabbedExpander).Name == "TopTabbedExpander");
            TabbedExpander BottomTabExp = ATC.FindVisualChild<TabbedExpander>(x => (x as TabbedExpander).Name == "BottomTabbedExpander");

            ATC.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Loaded, (Action)(() =>
            {
                Grid BTEGrid = ATC.FindVisualChild<Grid>(x => (x as Grid).Name == "TabControlGrid");
                RowDefinition rowDef = BTEGrid.RowDefinitions[3];
                string standardHeader = BuildTabHeader(this.LastTabCodigoComunidad, DLO, this.LastTabType);//  $"{this.LastTabCodigoComunidad} - {this.LastTabEjerHeader} - {TabHeaders[this.LastTabType]}";

                switch (this.LastTabType)
                {
                    case TabType.Mayor:
                        tab = new VMTabMayor();
                        tab.Header = standardHeader;
                        TabbedExpanderFiller_Mayor TabExpFillerM = new TabbedExpanderFiller_Mayor(
                            tab as VMTabMayor,
                            ref TopTabExp,
                            ref BottomTabExp,
                            ref rowDef,
                            true);
                        break;
                    case TabType.Diario:
                        tab = new VMTabDiario();
                        tab.Header = standardHeader;
                        TabbedExpanderFiller_Diario TabExpFillerD = new TabbedExpanderFiller_Diario(
                            tab as VMTabDiario,
                            ref TopTabExp,
                            ref BottomTabExp,
                            ref rowDef,
                            true);
                        break;
                    case TabType.Props:
                        tab = new VMTabProps();
                        tab.Header = standardHeader;
                        break;
                    case TabType.Cdad:
                        tab = new VMTabCdad();
                        tab.Header = standardHeader;
                        break;
                    default:
                        tab = new VMTabMayor();
                        tab.Header = standardHeader;
                        TabExpFillerM = new TabbedExpanderFiller_Mayor(
                            tab as VMTabMayor,
                            ref TopTabExp,
                            ref BottomTabExp,
                            ref rowDef,
                            true);
                        break;
                }
                this.Tabs.Add(tab);
                NotifyPropChanged("Tabs");
            }));
        }
        #endregion
    }
}

