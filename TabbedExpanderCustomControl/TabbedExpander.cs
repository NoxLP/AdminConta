using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.ComponentModel;
using System.Windows.Threading;
using Extensions;
using System.Collections;
using System.Collections.Specialized;
using AdConta;

namespace TabbedExpanderCustomControl
{
    /// <summary>
    /// Follow steps 1a or 1b and then 2 to use this custom control in a XAML file.
    ///
    /// Step 1a) Using this custom control in a XAML file that exists in the current project.
    /// Add this XmlNamespace attribute to the root element of the markup file where it is 
    /// to be used:
    ///
    ///     xmlns:MyNamespace="clr-namespace:TabbedExpanderCustomControl"
    ///
    ///
    /// Step 1b) Using this custom control in a XAML file that exists in a different project.
    /// Add this XmlNamespace attribute to the root element of the markup file where it is 
    /// to be used:
    ///
    ///     xmlns:MyNamespace="clr-namespace:TabbedExpanderCustomControl;assembly=TabbedExpanderCustomControl"
    ///
    /// You will also need to add a project reference from the project where the XAML file lives
    /// to this project and Rebuild to avoid compilation errors:
    ///
    ///     Right click on the target project in the Solution Explorer and
    ///     "Add Reference"->"Projects"->[Select this project]
    ///
    ///
    /// Step 2)
    /// Go ahead and use your control in the XAML file.
    ///
    ///     <MyNamespace:CustomControl1/>
    ///
    /// </summary>
    public class TabbedExpander : TabControl, INotifyPropertyChanged
    {
        static TabbedExpander()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TabbedExpander), new FrameworkPropertyMetadata(typeof(TabbedExpander)));
        }

        #region fields
        private List<ToggleButton> TogsList;
        private List<TabItem> TabsList;
        //private bool _CurrentChangingEventHandlerAdded = false;
#pragma warning disable CS0169
        private int _SelectedIndex;
#pragma warning restore CS0169
        #endregion

        #region properties
        public double _MaxHeight { get; set; }
        public double _MinHeight { get; set; }
        public VerticalAlignment _VerticalAlignment { get; set; }
        public double LastHeight { get; set; }
        #endregion

        #region commands
        private Command_ChangeExpandedCommand _ChangeExpandedCommand;
        public ICommand ChangeExpandedCommand { get { return this._ChangeExpandedCommand; } }
        #endregion

        #region dependency properties
        #region public
        public bool IsExpanded
        {
            get { return (bool)GetValue(IsExpandedProperty); }
            set { SetValue(IsExpandedProperty, value); }
        }
        public double EXPANDER_OFFSET
        {
            get { return (double)GetValue(EXPANDER_OFFSETProperty); }
            set { SetValue(EXPANDER_OFFSETProperty, value); }
        }
        public double EXPANDER_NOTEXPANDED_HEIGHT
        {
            get { return (double)GetValue(EXPANDER_NOTEXPANDED_HEIGHTProperty); }
            set { SetValue(EXPANDER_NOTEXPANDED_HEIGHTProperty, value); }
        }
        public double EXPANDER_EXPANDED_HEIGHT
        {
            get { return (double)GetValue(EXPANDER_EXPANDED_HEIGHTProperty); }
            set { SetValue(EXPANDER_EXPANDED_HEIGHTProperty, value); }
        }
        public Brush PanelBackground
        {
            get { return (Brush)GetValue(PanelBackgroundProperty); }
            set { SetValue(PanelBackgroundProperty, value); }
        }
        public Brush SelectedBackground
        {
            get { return (Brush)GetValue(SelectedBackgroundProperty); }
            set { SetValue(SelectedBackgroundProperty, value); }
        }
        public Brush ContentBorderBrush
        {
            get { return (Brush)GetValue(ContentBorderBrushProperty); }
            set { SetValue(ContentBorderBrushProperty, value); }
        }
        public Thickness ContentBorderThickness
        {
            get { return (Thickness)GetValue(ContentBorderThicknessProperty); }
            set { SetValue(ContentBorderThicknessProperty, value); }
        }
        #endregion

        #region static
        public static readonly DependencyProperty IsExpandedProperty =
            DependencyProperty.Register("IsExpanded",
                typeof(bool),
                typeof(TabbedExpander),
                new PropertyMetadata(false, OnIsExpandedChanged));
        private static void OnIsExpandedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue == null || e.OldValue == null) return;
            TabbedExpander control = d as TabbedExpander;

            if ((bool)e.NewValue)
            {
                control.MinHeight = control._MinHeight;
                control.VerticalAlignment = VerticalAlignment.Stretch;
                control.Height = control.EXPANDER_EXPANDED_HEIGHT;
                /*control.SetBinding(
                    TabbedExpander.HeightProperty,
                    new Binding()
                    {
                        Source = control,
                        Path = new PropertyPath("EXPANDER_EXPANDED_HEIGHT"),
                        Mode = BindingMode.OneWay
                    });*/
            }
            else
            {
                control.MinHeight = control.EXPANDER_NOTEXPANDED_HEIGHT;
                control.VerticalAlignment = control._VerticalAlignment;
                control.Height = control.EXPANDER_NOTEXPANDED_HEIGHT;// - control.EXPANDER_OFFSET;
                /*switch(control.TabStripPlacement)
                {
                    case Dock.Bottom:
                        control.VerticalAlignment = VerticalAlignment.Bottom;
                        break;
                    case Dock.Left:
                        control.HorizontalAlignment = HorizontalAlignment.Left;
                        break;
                    case Dock.Right:
                        control.HorizontalAlignment = HorizontalAlignment.Right;
                        break;
                    case Dock.Top:
                        control.VerticalAlignment = VerticalAlignment.Top;
                        break;
                }*/
            }
            control.NotifyPropChanged("IsExpanded");
        }

        [TypeConverter(typeof(double))]
        public static readonly DependencyProperty EXPANDER_OFFSETProperty =
            DependencyProperty.Register("EXPANDER_OFFSET",
                typeof(double),
                typeof(TabbedExpander),
                new PropertyMetadata(102d, OnOffsetChanged));
        private static void OnOffsetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue == null || e.OldValue == null) return;
            TabbedExpander control = d as TabbedExpander;

            if (!control.IsExpanded)
                control.Height = control.EXPANDER_NOTEXPANDED_HEIGHT - (double)e.NewValue;
        }
        [TypeConverter(typeof(double))]
        public static readonly DependencyProperty EXPANDER_NOTEXPANDED_HEIGHTProperty =
            DependencyProperty.Register("EXPANDER_NOTEXPANDED_HEIGHT",
                typeof(double),
                typeof(TabbedExpander),
                new PropertyMetadata(31.0, OnNotExpandedHeightChanged));
        private static void OnNotExpandedHeightChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue == null || e.OldValue == null) return;
            TabbedExpander control = d as TabbedExpander;

            if (!control.IsExpanded)
                control.Height = (double)e.NewValue;// - control.EXPANDER_OFFSET;
            
            control.NotifyPropChanged("EXPANDER_NOTEXPANDED_HEIGHT");
        }
        [TypeConverter(typeof(double))]
        public static readonly DependencyProperty EXPANDER_EXPANDED_HEIGHTProperty =
            DependencyProperty.Register("EXPANDER_EXPANDED_HEIGHT",
                typeof(double),
                typeof(TabbedExpander),
                new PropertyMetadata(100d, OnExpandedHeightChanged));
        private static void OnExpandedHeightChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue == null || e.OldValue == null) return;
            TabbedExpander control = d as TabbedExpander;

            if (control.IsExpanded)
                control.Height = (double)e.NewValue;

            control.NotifyPropChanged("EXPANDER_EXPANDED_HEIGHT");
            //control.EXPANDER_EXPANDED_HEIGHT = (double)e.NewValue;
        }

        public static readonly DependencyProperty PanelBackgroundProperty =
            DependencyProperty.Register("PanelBackground",
                typeof(Brush),
                typeof(TabbedExpander),
                new PropertyMetadata(Brushes.Gray));
        public static readonly DependencyProperty SelectedBackgroundProperty =
            DependencyProperty.Register("SelectedBackground",
                typeof(Brush),
                typeof(TabbedExpander),
                new PropertyMetadata(Brushes.White));

        public static readonly DependencyProperty ContentBorderBrushProperty =
            DependencyProperty.Register("ContentBorderBrush", 
                typeof(Brush), 
                typeof(TabbedExpander), 
                new PropertyMetadata(Brushes.Transparent));
        public static readonly DependencyProperty ContentBorderThicknessProperty =
            DependencyProperty.Register("ContentBorderThickness", 
                typeof(Thickness), 
                typeof(TabbedExpander), 
                new PropertyMetadata(new Thickness(0)));
        #endregion
        #endregion

        #region PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        protected void NotifyPropChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion

        #region helpers
        private void AllIsSelectedFalseExcept(TabItem tab)
        {
            IEnumerable<TabItem> thisTabs = this.FindVisualChildren<TabItem>();

            foreach (TabItem _tab in thisTabs)
            {
                if (_tab != tab)
                    _tab.IsSelected = false;
            }
        }
        private void ManageInitTogsAndTabs(FrameworkElement ParentGrid)
        {
            IEnumerable children = ParentGrid.FindVisualChildren<TabItem>();
            foreach (TabItem tab in children)
            {
                tab.Tag = tab.DataContext;
                this.TabsList.Add(tab);

                TabExpTabItemBaseVM tabExp = tab.DataContext as TabExpTabItemBaseVM;
                if (tabExp != null && tabExp.Expandible == false)
                {
                    Border bd = tab.Template.FindName("Bd", tab) as Border;
                    ContentControl cc = new ContentControl();
                    cc.Name = "Content";
                    cc.Content = tabExp.TEHeaderTemplate;
                    cc.Template = tabExp.TEHeaderTemplate;
                    bd.Child = cc;
                }
            }

            children = ParentGrid.FindVisualChildren<ToggleButton>();
            foreach (ToggleButton tog in children)
            {
                this.TogsList.Add(tog);
                tog.Click += ToggleButtonClick;
            }
        }
        public double CorrectHeight(double height)
        {
            if (height < base.MinHeight) return base.MinHeight;
            else if (height > base.MaxHeight) return base.MaxHeight;
            return height;
        }
        #endregion

        #region events
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            this._ChangeExpandedCommand = new Command_ChangeExpandedCommand(this);
            this._MaxHeight = base.MaxHeight;
            this._MinHeight = base.MinHeight;
            this._VerticalAlignment = base.VerticalAlignment;

            this.TogsList = new List<ToggleButton>();
            this.TabsList = new List<TabItem>();
            this.IsExpanded = false;
            FrameworkElement ParentGrid = this.Template.FindName("PART_ParentGrid", this) as FrameworkElement;

            this.Dispatcher.BeginInvoke(DispatcherPriority.Loaded, (Action)(() => ManageInitTogsAndTabs(ParentGrid)));

            DependencyPropertyChangedEventArgs e = new DependencyPropertyChangedEventArgs(IsExpandedProperty, !this.IsExpanded, this.IsExpanded);
            OnIsExpandedChanged(this, e);
        }
        protected override void OnItemsSourceChanged(IEnumerable oldValue, IEnumerable newValue)
        {
            if (newValue == oldValue) return;

            if (newValue != null)
            {
               foreach (object item in newValue)
                {
                    TabItem tab = (this.ItemContainerGenerator.ContainerFromItem(item) as TabItem);
                    tab.Dispatcher.BeginInvoke(DispatcherPriority.Loaded, (Action)(() =>
                    {
                        
                        var tabVM = tab.DataContext;
                        TabExpTabItemBaseVM tabExp = item as TabExpTabItemBaseVM;

                        if (tabExp == null || tabExp.Expandible)
                        {
                            if (!this.TabsList.Contains(tab))
                            {
                                tab.Tag = tabVM;
                                this.TabsList.Add(tab);
                            }

                            ToggleButton tog = tab.FindFirstVisualChildOfType<ToggleButton>();
                            if (!this.TogsList.Contains(tog) && tog != null)
                            {
                                this.TogsList.Add(tog);
                                tog.Click += ToggleButtonClick;
                            }
                        }
                        else
                        {
                            Border bd = tab.Template.FindName("Bd", tab) as Border;
                            if (bd == null) return;
                            ContentControl cc = new ContentControl();
                            cc.Name = "Content";
                            cc.Content = tabExp.TEHeaderTemplate;
                            cc.Template = tabExp.TEHeaderTemplate;
                            bd.Child = cc;
                        }
                    }));
                }
            }

            if (oldValue != null)
            {
                foreach (object item in oldValue)
                {
                    //If the DataContext is not saved in other place, the reference is lost BEFORE this event launches and the 
                    //click event of the togglebutton can't ever be unsubscribed(ContainerFromItem don't work, NOTHING work, e.OldItems
                    //have NO reference to the tabitem when it reaches this event), so all tabitems are saved in a list field 
                    //and each datacontext saved as each tabitem's tag (OnApplyTemplate and OnItemsChanged if(e.NewItems != null) ), 
                    //so the tag and the tabitem persist, can be found in this event, and togglebutton click can be unsubscribed
                    TabItem tab = this.TabsList.Find(x => x.Tag == item);
                    //If the tab is not expandible, it have no tog button and no event, so don't unsubscribe
                    if (tab != null)
                    {
                        ToggleButton tog = tab.FindFirstVisualChildOfType<ToggleButton>();
                        tog.Click -= ToggleButtonClick;
                        this.TogsList.Remove(tog);
                    }
                    this.TabsList.Remove(tab);
                }
            }
            
            base.OnItemsSourceChanged(oldValue, newValue);
        }
        protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems == e.OldItems) return;
            if (e.Action == NotifyCollectionChangedAction.Reset) this.Items.Clear();

            if (e.NewItems != null)
            {
                foreach (object item in e.NewItems)
                {
                    Dispatcher.BeginInvoke(DispatcherPriority.Loaded, (Action)(() =>
                    {
                        TabItem tab = (this.ItemContainerGenerator.ContainerFromItem(item) as TabItem);
                        var tabVM = tab.DataContext;
                        TabExpTabItemBaseVM tabExp = item as TabExpTabItemBaseVM;

                        if (tabExp == null || tabExp.Expandible)
                        {
                            if (!this.TabsList.Contains(tab))
                            {
                                tab.Tag = tabVM;
                                this.TabsList.Add(tab);
                            }

                            ToggleButton tog = tab.FindFirstVisualChildOfType<ToggleButton>();
                            if (!this.TogsList.Contains(tog))
                            {
                                this.TogsList.Add(tog);
                                tog.Click += ToggleButtonClick;
                            }
                        }
                        else
                        {
                            Border bd = tab.Template.FindName("Bd", tab) as Border;
                            ContentControl cc = new ContentControl();
                            cc.Name = "Content";
                            cc.Content = tabExp.TEHeaderTemplate;
                            cc.Template = tabExp.TEHeaderTemplate;
                            bd.Child = cc;
                        }
                    }));
                }
            }

            if (e.OldItems != null)
            {
                foreach (object item in e.OldItems)
                {
                    //If the DataContext is not saved in other place, the reference is lost BEFORE this event launches and the 
                    //click event of the togglebutton can't ever be unsubscribed(ContainerFromItem don't work, NOTHING work, e.OldItems
                    //have NO reference to the tabitem when it reaches this event), so all tabitems are saved in a list field 
                    //and each datacontext saved as each tabitem's tag (OnApplyTemplate and OnItemsChanged if(e.NewItems != null) ), 
                    //so the tag and the tabitem persist, can be found in this event, and togglebutton click can be unsubscribed
                    TabItem tab = this.TabsList.Find(x => x.Tag == item);
                    //If the tab is not expandible, it have no tog button and no event, so don't unsubscribe
                    if (tab != null)
                    {
                        ToggleButton tog = tab.FindFirstVisualChildOfType<ToggleButton>();
                        tog.Click -= ToggleButtonClick;
                        this.TogsList.Remove(tog);
                    }
                    this.TabsList.Remove(tab);
                }
            }

            base.OnItemsChanged(e);
        }
        /*protected override void OnSelectionChanged(SelectionChangedEventArgs e)
        {
            TabExpTabItemBaseVM item = e.AddedItems[0] as TabExpTabItemBaseVM;
            if (item != null && item.Expandible)
            {
                this._SelectedIndex = base.Items.IndexOf(item);
                base.OnSelectionChanged(e);
            }
            else
            {
                base.SelectedIndex = this._SelectedIndex;
                e.AddedItems.Clear();
                e.RemovedItems.Clear();
                e.Handled = true;

                base.OnSelectionChanged(e);
            }
        }*/
        private void ToggleButtonClick(object sender, RoutedEventArgs e)
        {
            ToggleButton tog = sender as ToggleButton;
            TabItem tab = tog.FindFirstParentOfType<TabItem>();

            if (!this.IsExpanded)
            {
                this.IsExpanded = true;
                tab.IsSelected = true;
            }
            else if (tab.IsSelected && this.IsExpanded)
                this.IsExpanded = false;
            else
            {
                tab.IsSelected = true;
                this.TogsList.ForEach(x => x.IsChecked = true);
            }

            e.Handled = true;
        }
        #endregion
    }

    public class Command_ChangeExpandedCommand : ICommand
    {
        private TabbedExpander _TE;

        public Command_ChangeExpandedCommand(TabbedExpander TE)
        {
            this._TE = TE;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public void Execute(object parameter)
        {
            this._TE.IsExpanded = !this._TE.IsExpanded;
            this._TE.Focus();
        }
    }
}