using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Collections.Specialized;
using AdConta.ViewModel;
using ModuloContabilidad;
using Extensions;
using TabbedExpanderCustomControl;

namespace AdConta.AbleTabControl
{
    /// <summary>
    /// USER CONTROL VIEW FOR PRINCIPAL TAB CONTROL. CLOSEABLE AND DRAGGABLE TABS(see events). Interaction logic for TabControl.xaml
    /// </summary>
    public partial class AbleTabControl : UserControl, INotifyPropertyChanged
    {
        public AbleTabControl()
        {
            InitializeComponent();
            
            ItemsSourceProperty = new ObservableCollection<aVMTabBase>();
            this._NavigateSelectedTab = new Command_NavigateSelectedTab(this);

            this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Loaded, (Action)(() =>
            { 
                Grid BTEGrid = this.RootTabControl.FindVisualChild<Grid>(x => (x as Grid).Name == "TabControlGrid");
                RowDefinition rowDef = BTEGrid.RowDefinitions[3];
                TabbedExpander BTE = this.RootTabControl.FindVisualChild<TabbedExpander>(x => (x as FrameworkElement).Name == "BottomTabbedExpander");
                rowDef.Height = new GridLength(BTE.EXPANDER_NOTEXPANDED_HEIGHT);
            }));
        }

        #region fields
        private int _Index = 0;
        #endregion

        #region properties
        public int Index
        {
            get { return _Index; }
            set
            {
                _Index = value;
                NotifyPropChanged("Index");
            }
        }
        #endregion

        #region commands
        private Command_NavigateSelectedTab _NavigateSelectedTab;
        public ICommand NavigateSelectedTab { get { return this._NavigateSelectedTab; } }
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

        #region dependency properties
        public static readonly DependencyProperty ItemsSourcePropertyProperty = DependencyProperty.Register("ItemsSourceProperty",
            typeof(ObservableCollection<aVMTabBase>), typeof(AbleTabControl),
            new PropertyMetadata(OnItemsPropertyChanged));
        public ObservableCollection<aVMTabBase> ItemsSourceProperty
        {
            get { return (ObservableCollection<aVMTabBase>)GetValue(ItemsSourcePropertyProperty); }
            set { SetValue(ItemsSourcePropertyProperty, value); }
        }
        private static void OnItemsPropertyChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            AbleTabControl control = source as AbleTabControl;

            var old = e.OldValue as ObservableCollection<VMTabDiario>;

            if (old != null)
            {
                // Unsubscribe from CollectionChanged on the old collection
                old.CollectionChanged -= control.ItemsSource_CollectionChanged;
            }

            var n = e.NewValue as ObservableCollection<VMTabDiario>;

            if (n != null)
            {
                // Subscribe to CollectionChanged on the new collection
                n.CollectionChanged += control.ItemsSource_CollectionChanged;
            }
        }
        private void ItemsSource_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Reset) this.ItemsSourceProperty.Clear();
            
            if (e.NewItems != null)
            {
                foreach (aVMTabBase item in e.NewItems)
                {
                    this.ItemsSourceProperty.Add(item);
                    item.ChangeTabIndex(e.NewStartingIndex);
                }
            }

            if (e.OldItems != null)
            {
                foreach (VMTabDiario item in e.OldItems)
                {
                    this.ItemsSourceProperty.RemoveAt(e.OldStartingIndex);
                }
            }
        }


        public static readonly DependencyProperty SelectedIndexProperty = DependencyProperty.Register("SelectedIndexProperty",
            typeof(int), typeof(AbleTabControl),
            new PropertyMetadata(0, OnSelectedIndexPropertyChanged));
        public int SelectedIndex
        {
            get { return (int)GetValue(SelectedIndexProperty); }
            set { SetValue(SelectedIndexProperty, value); }
        }
        private static void OnSelectedIndexPropertyChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            AbleTabControl control = source as AbleTabControl;
            int newIndex = (int)e.NewValue;
            int oldIndex = (int)e.OldValue;
            if (newIndex > control.ItemsSourceProperty.Count - 1 || newIndex < 0 ||
                oldIndex > control.ItemsSourceProperty.Count - 1 || oldIndex < 0)
                return;

            aVMTabBase newTab = control.ItemsSourceProperty[newIndex];
            aVMTabBase oldTab = control.ItemsSourceProperty[oldIndex];
            oldTab.IsSelected = false;
            oldTab.PublicNotifyPropChanged("IsSelected");

            newTab.IsSelected = true;
            newTab.PublicNotifyPropChanged("IsSelected");
            //control.SelectedIndex = newIndex;            

            control.Dispatcher.BeginInvoke((Action)(() => control.RootTabControl.SelectedIndex = newIndex));
            //control.NotifyPropChanged("SelectedIndex");
            //control.RootTabControl.ContentTemplateSelector.SelectTemplate(oldTab, control.RootTabControl as DependencyObject);

            TabbedExpander TopTabExp = control.RootTabControl.FindVisualChild<TabbedExpander>(x => (x as FrameworkElement).Name == "TopTabbedExpander");
            TabbedExpander BottomTabExp = control.RootTabControl.FindVisualChild<TabbedExpander>(x => (x as FrameworkElement).Name == "BottomTabbedExpander");
            Grid BTEGrid = control.RootTabControl.FindVisualChild<Grid>(x => (x as Grid).Name == "TabControlGrid");
            RowDefinition rowDef = BTEGrid.RowDefinitions[3];
            TabbedExpanderBindingChanger filler = new TabbedExpanderBindingChanger(newTab as aTabsWithTabExpVM, ref TopTabExp, ref BottomTabExp, ref rowDef);
        }
        #endregion

        #region helpers
        #endregion

        #region events
        private void CloseTabButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            TabItem tab = button.FindFirstParentOfType<TabItem>();
            Task.Run(() => (tab.DataContext as aVMTabBase).CleanUnitOfWork()).Forget().ConfigureAwait(false);
            this.ItemsSourceProperty.RemoveAt(this.ItemsSourceProperty.IndexOf(tab.DataContext as aVMTabBase));
        }

        private void TabItem_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.OriginalSource is Button) return;

            TabItem tabItem = sender as TabItem;//this.RootTabControl.Items[this.Index] as TabItem;//this.ItemsSourceProperty[this.Index];

            if (tabItem == null)// || tabItem == (TabItem)tabControl.SelectedItem)
                return;

            if (Mouse.PrimaryDevice.LeftButton == MouseButtonState.Pressed)
            {
                DragDrop.DoDragDrop(tabItem, tabItem, DragDropEffects.All);
            }
        }

        private void TabItem_Drop(object sender, DragEventArgs e)
        {
            aVMTabBase tabItemTarget = (e.Source as TabItem).DataContext as aVMTabBase;

            aVMTabBase tabItemSource = (e.Data.GetData(typeof(TabItem)) as TabItem).DataContext as aVMTabBase;

            if (!tabItemTarget.Equals(tabItemSource))
            {
                int sourceIndex = this.ItemsSourceProperty.IndexOf(tabItemSource);
                int targetIndex = this.ItemsSourceProperty.IndexOf(tabItemTarget);

                this.ItemsSourceProperty.Remove(tabItemSource);
                this.ItemsSourceProperty.Insert(targetIndex, tabItemSource);

                this.ItemsSourceProperty.Remove(tabItemTarget);
                this.ItemsSourceProperty.Insert(sourceIndex, tabItemTarget);
                NotifyPropChanged("ItemsSourceProperty");
            }
        }
        #endregion
    }

    public class Command_NavigateSelectedTab : ICommand
    {
        private AbleTabControl _TabC;

        public Command_NavigateSelectedTab(AbleTabControl TabC)
        {
            this._TabC = TabC;
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
            string param = (string)parameter;
            if (string.IsNullOrEmpty(param)) return;
            
            switch(param)
            {
                case "right":
                    this._TabC.SelectedIndex = this._TabC.SelectedIndex+1;
                    return;
                case "left":
                    this._TabC.SelectedIndex = this._TabC.SelectedIndex - 1;
                    return;
                default: return;
            }
        }
    }
}

