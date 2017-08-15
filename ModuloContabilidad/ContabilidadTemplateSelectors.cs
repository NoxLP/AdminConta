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
                case TabExpTabType.Mayor1_Cuenta:
                    TabItem.Header = "Cuenta";
                    DataTemplate dtemp = (DataTemplate)Application.Current.Resources["TabExpTabMayor_Cuenta"];
                    return dtemp;
                case TabExpTabType.Mayor3_Buscar:
                    TabItem.Header = "Buscar";
                    dtemp = (DataTemplate)Application.Current.Resources["TabExpTabMayor_Buscar"];
                    return dtemp;
                case TabExpTabType.Inferior_AsientoSimple:
                    TabItem.Header = "Asiento simple";
                    dtemp = (DataTemplate)Application.Current.Resources["TabExpTabInferior_AsientoSimple"];
                    return dtemp;
                case TabExpTabType.Inferior_AsientoComplejo:
                    TabItem.Header = "Asiento complejo";
                    dtemp = (DataTemplate)Application.Current.Resources[""];
                    return dtemp;
                case TabExpTabType.Inferior_Diario:
                    TabItem.Header = "Vista Diario";
                    dtemp = (DataTemplate)Application.Current.Resources["TabExpTabInferior_Diario"];
                    return dtemp;
                default: return null;
            }
        }
    }
}
