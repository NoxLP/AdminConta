using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using TabbedExpanderCustomControl;
using System.Windows;
using System.Windows.Data;
using Converters;

namespace AdConta.ViewModel
{
    /// <summary>
    /// Base class for filling both tabbed expander. Used when new tabs are added or selected to AbleTabControl.
    /// </summary>
    public abstract class aTabbedExpanderFillerBase <T> where T : aTabsWithTabExpVM
    {
        public aTabbedExpanderFillerBase(
            T container, 
            int numberTabs, 
            ref TabbedExpander topTE, 
            ref TabbedExpander bottomTE, 
            ref RowDefinition rowDef,
            bool fill)
        {
            this._TabExpContainer = container;
            this._numberOfTabs = numberTabs;

            if (fill)
            {
                FillTopTabExp();
                FillBottomTabExp();
            }
            BindTabbedExpanders(ref topTE, ref bottomTE, ref container, ref rowDef);
        }

        #region fields
        private aTabsWithTabExpVM _TabExpContainer;
        private List<TabExpTabItemBaseVM> _Tabs;
        private int _numberOfTabs;
        #endregion

        #region properties
        protected aTabsWithTabExpVM TabExpContainer
        {
            get { return this._TabExpContainer; }
            set { this._TabExpContainer = value; }
        }
        protected List<TabExpTabItemBaseVM> Tabs
        {
            get { return this._Tabs; }
            set { this._Tabs = value; }
        }
        protected int numberOfTabs
        {
            get { return this._numberOfTabs; }
            set { this._numberOfTabs = value; }
        }
        #endregion

        #region methods
        protected abstract void FillTopTabExp();
        protected abstract void FillBottomTabExp();

        protected void BindTabbedExpanders(ref TabbedExpander TopTE, ref TabbedExpander BottomTE, ref T tab, ref RowDefinition rowDef)
        {
            TopTE.SetBinding(
                TabbedExpander.ItemsSourceProperty,
                new Binding()
                {
                    Source = tab,
                    //Path = new PropertyPath((tab as aTabsWithTabExpVM).TopTabbedExpanderItemsSource),
                    Path = new PropertyPath("TopTabbedExpanderItemsSource"),
                    Mode = BindingMode.OneWay
                });
            TopTE.SetBinding(
                TabbedExpander.SelectedIndexProperty,
                new Binding()
                {
                    Source = tab,
                    Path = new PropertyPath("TopTabbedExpanderSelectedIndex"),
                    Mode = BindingMode.TwoWay
                });

            BottomTE.SetBinding(
                TabbedExpander.ItemsSourceProperty,
                new Binding()
                {
                    Source = tab,
                    Path = new PropertyPath("BottomTabbedExpanderItemsSource"),
                    Mode = BindingMode.TwoWay
                });
            BottomTE.SetBinding(
                TabbedExpander.SelectedIndexProperty,
                new Binding()
                {
                    Source = tab,
                    Path = new PropertyPath("BottomTabbedExpanderSelectedIndex"),
                    Mode = BindingMode.TwoWay
                });
            BottomTE.SetBinding(
                TabbedExpander.IsExpandedProperty,
                new Binding()
                {
                    Source = tab,
                    Path = new PropertyPath("BTEExpanded"),
                    Mode = BindingMode.OneWayToSource
                });
            BottomTE.SetBinding(
                TabbedExpander.EXPANDER_EXPANDED_HEIGHTProperty,
                new Binding()
                {
                    Source = tab,
                    Path = new PropertyPath("ExpandedHeight"),
                    Mode = BindingMode.OneWay
                });

            double height = BottomTE.EXPANDER_NOTEXPANDED_HEIGHT;
            BottomTE.SetBinding(
                TabbedExpander.EXPANDER_NOTEXPANDED_HEIGHTProperty,
                new Binding()
                {
                    Source = tab,
                    Path=new PropertyPath("NotExpandedHeight"),
                    Mode=BindingMode.OneWayToSource
                });
            BottomTE.EXPANDER_NOTEXPANDED_HEIGHT = height;

            rowDef.SetBinding(
                RowDefinition.HeightProperty,
                new Binding()
                {
                    Source = tab,
                    Path = new PropertyPath("BTEGridHeight"),
                    Mode = BindingMode.TwoWay
                });
            /*MultiBinding MBind = new MultiBinding();
            MBind.Bindings.Add(new Binding() { Source = BottomTE, Path = new PropertyPath("IsExpanded"), Mode = BindingMode.OneWay });
            MBind.Bindings.Add(new Binding() { Source = tab, Path = new PropertyPath("BTEGridHeight"), Mode = BindingMode.TwoWay });
            MBind.Bindings.Add(new Binding() { Source = BottomTE, Path = new PropertyPath("EXPANDER_NOTEXPANDED_HEIGHT"), Mode = BindingMode.OneWay });
            MBind.Converter = new BoolHeightToHeightMulticonverter();
            MBind.ConverterParameter = "GRID";
            MBind.Mode = BindingMode.TwoWay;
            MBind.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            rowDef.SetBinding(RowDefinition.HeightProperty, MBind);*/
        }
        #endregion
    }

    public class TabbedExpanderBindingChanger : aTabbedExpanderFillerBase<aTabsWithTabExpVM>
    {
        public TabbedExpanderBindingChanger(
            aTabsWithTabExpVM TabExpVMContainer,
            ref TabbedExpander topTE,
            ref TabbedExpander bottomTE,
            ref RowDefinition rowDef) 
            : base(TabExpVMContainer, 3, ref topTE, ref bottomTE, ref rowDef, false)
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
