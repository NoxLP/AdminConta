using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Collections.ObjectModel;
using TabbedExpanderCustomControl;

namespace AdConta.ViewModel
{
    public abstract class aTabsWithTabExpVM : aVMTabBase
    {
        #region fields
        private bool _BTEExpanded;
        private GridLength _BTEGridHeight;
        private GridLength _LastBTEGridHeight = new GridLength(60);
        #endregion

        #region properties
        public abstract ObservableCollection<TabExpTabItemBaseVM> TopTabbedExpanderItemsSource { get; set; }
        public abstract ObservableCollection<TabExpTabItemBaseVM> BottomTabbedExpanderItemsSource { get; set; }
        public abstract int TopTabbedExpanderSelectedIndex { get; set; }
        public abstract int BottomTabbedExpanderSelectedIndex { get; set; }
        public double NotExpandedHeight { get; set; }
        public bool BTEExpanded
        {
            get { return this._BTEExpanded; }
            set
            {
                if (this._BTEExpanded != value)
                {
                    this._BTEExpanded = value;

                    if (!value) this.BTEGridHeight = new GridLength(this.NotExpandedHeight+8d);
                    else this.BTEGridHeight = this.LastBTEGridHeight;
                }
            }
        }
        public GridLength BTEGridHeight
        {
            get { return this._BTEGridHeight; }
            set
            {
                if(this._BTEGridHeight != value)
                {
                    this._BTEGridHeight = value;
                    
                    if (this.BTEExpanded && value != GridLength.Auto) this.LastBTEGridHeight = value;
                    NotifyPropChanged("BTEGridHeight");
                    NotifyPropChanged("ExpandedHeight");
                }
            }
        }
        public double ExpandedHeight
        {
            get
            {
                if (this.BTEExpanded)
                    return this.BTEGridHeight.Value;
                else return this.LastBTEGridHeight.Value;
            }
        }
        public GridLength LastBTEGridHeight
        {
            get { return this._LastBTEGridHeight; }
            set
            {
                if (this._LastBTEGridHeight != value)
                    this._LastBTEGridHeight = value;
            }
        }
        #endregion

        #region helpers
        /// <summary>
        /// Add tabVM to tabbed expander of type WhichTabExp(top or bottom) through ItemsSource. 
        /// Used when new tabs are added or selected in AbleTabControl.
        /// </summary>
        /// <param name="tabVM"></param>
        /// <param name="WhichTabExp"></param>
        public virtual void AddTabInTabbedExpander(TabExpTabItemBaseVM tabVM, TabExpWhich WhichTabExp)
        {
            if (WhichTabExp == TabExpWhich.Top)
            {
                TopTabbedExpanderItemsSource.Add(tabVM);
                NotifyPropChanged("TopTabbedExpanderItemsSource");
            }
            else
            {
                BottomTabbedExpanderItemsSource.Add(tabVM);
                NotifyPropChanged("BottomTabbedExpanderItemsSource");
            }
        }
        /// <summary>
        /// Add tabVM to tabbed expander of type WhichTabExp(top or bottom) through ItemsSource.
        /// Used when tabs are added or changed in any tabbed expander.
        /// </summary>
        /// <param name="tabVM"></param>
        /// <param name="WhichTabExp"></param>
        public virtual void AddAndSelectTabInTabbedExpander(TabExpTabItemBaseVM tabVM, TabExpWhich WhichTabExp)
        {
            if(WhichTabExp == TabExpWhich.Top)
            {
                TopTabbedExpanderItemsSource.Add(tabVM);
                TopTabbedExpanderSelectedIndex = this.TopTabbedExpanderItemsSource.IndexOf(tabVM);
                NotifyPropChanged("TopTabbedExpanderItemsSource");
            }
            else
            {
                BottomTabbedExpanderItemsSource.Add(tabVM);
                BottomTabbedExpanderSelectedIndex = this.BottomTabbedExpanderItemsSource.IndexOf(tabVM);
                NotifyPropChanged("BottomTabbedExpanderItemsSource");
            }            
        }
        #endregion
    }
}