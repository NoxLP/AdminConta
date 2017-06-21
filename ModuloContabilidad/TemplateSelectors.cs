using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using AdConta;
using TabbedExpanderCustomControl;
using Extensions;

namespace ModuloContabilidad
{
    /// <summary>
    /// Template selector for tabmayor tabbed expander
    /// </summary>
    public class TabbedExpTemplateSelector_ModContabilidad : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            TabExpTabItemBaseVM TabItem = item as TabExpTabItemBaseVM;
            if (TabItem==null) return null;

            TabExpTabType type = (item as TabExpTabItemBaseVM).TabExpType;

            switch (type)
            {
                case TabExpTabType.Diario:
                    TabItem.Header = "Vista Diario";
                    DataTemplate dtemp = (DataTemplate)Application.Current.Resources["TabExpTabDiario"];
                    return dtemp;
                case TabExpTabType.Simple:
                    TabItem.Header = "Asiento simple";
                    dtemp = (DataTemplate)Application.Current.Resources["TabExpTabAsSimple"];
                    return dtemp;
                case TabExpTabType.Complejo:
                    TabItem.Header = "Asiento complejo";
                    dtemp = (DataTemplate)Application.Current.Resources[""];
                    return dtemp;
                case TabExpTabType.Mayor1_Cuenta:
                    TabItem.Header = "Cuenta";
                    dtemp = (DataTemplate)Application.Current.Resources["TabExpTabCuenta"];
                    return dtemp;
                case TabExpTabType.Mayor3_Buscar:
                    TabItem.Header = "Buscar";
                    dtemp = (DataTemplate)Application.Current.Resources["TabExpTabBuscar"];
                    return dtemp;
                default: return null;
            }
        }
    }
}
