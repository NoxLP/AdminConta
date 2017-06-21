using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using AdConta.ViewModel;
using TabbedExpanderCustomControl;

namespace AdConta
{
    /// <summary>
    /// Template selector for abletabcontrol
    /// </summary>
    public class TabItemTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            App Application = App.Current as App;
            aVMTabBase tab = item as aVMTabBase;

            switch (tab.Type)
            {
                case TabType.Mayor:
                    return (DataTemplate)Application.Resources["TabMayor"];
                case TabType.Diario:
                    return (DataTemplate)Application.Resources["TabDiario"];
                case TabType.Props:
                    return (DataTemplate)Application.Resources["TabProps"];
                case TabType.Cdad:
                    return (DataTemplate)Application.Resources["TabCdad"];
                default: return null;
            }
        }
    }

    /// <summary>
    /// Template selector for Tabbed expander headers
    /// </summary>
    public class TabExpHeaderTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            App Application = App.Current as App;
            iTabbedExpanderItemVM tab = item as iTabbedExpanderItemVM;

            if (!tab.Expandible)
            {
                switch (tab.Type)
                {
                    
                    default: return null;
                }
            }

            return (DataTemplate)Application.Resources["TabItemStyle1"];
        }
    }
}
