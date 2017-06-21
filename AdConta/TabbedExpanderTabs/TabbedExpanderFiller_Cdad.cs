using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AdConta.ViewModel;
using TabbedExpanderCustomControl;

namespace AdConta.TabbedExpanderTabs
{
    public class TabbedExpanderFiller_Cdad : aTabbedExpanderFillerBase<VMTabCdad>
    {
        public TabbedExpanderFiller_Cdad(
            VMTabCdad TabExpVMContainer,
            ref TabbedExpander topTE,
            ref TabbedExpander bottomTE,
            ref System.Windows.Controls.RowDefinition rowDef,
            bool fill) 
            : base(TabExpVMContainer, 3/*<------------OJO*/, ref topTE, ref bottomTE, ref rowDef, fill)
        { }

        #region overriden methods
        protected override void FillTopTabExp()
        {
            throw new NotImplementedException();
        }

        protected override void FillBottomTabExp()
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
