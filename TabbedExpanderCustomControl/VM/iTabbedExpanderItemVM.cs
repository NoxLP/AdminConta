using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using AdConta;

namespace TabbedExpanderCustomControl
{
    public interface iTabbedExpanderItemVM : iTabbedExpanderItemBase
    {
        TabExpTabType Type { get; }
        bool IsSelected { get; set; }
        string Header { get; }
        double DGridHeight { get; }
    }

    public interface iTabbedExpanderItemBase
    {
        /// <summary>
        /// Tells tabbed expander is this tab can expand(true) or it works as a toolbar(false)
        /// </summary>
        bool Expandible { get; set; }
        /// <summary>
        /// Only if Expandible == false
        /// </summary>
        ControlTemplate TEHeaderTemplate { get; set; }
    }
}
