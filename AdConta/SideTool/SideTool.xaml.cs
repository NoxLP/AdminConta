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
using System.Data;
using AdConta.Models;
using AdConta.ViewModel;

namespace AdConta.SideTool
{
    /// <summary>
    /// USER CONTROL VIEW FOR SIDE EXPANDIBLE TOOL. Interaction logic for SideTool.xaml
    /// </summary>
    public partial class SideTool : UserControl, INotifyPropertyChanged
    {
        public SideTool()
        {
            InitializeComponent();

            this._model = new SideModel();
            this.ItemsSourceProperty = new ObservableCollection<CommItem>();

            foreach (DataRow row in this._model.Table.Rows)
            {
                int cod = this.GetValueFromTable<int>("Codigo", row);
                string name = this.GetValueFromTable<string>("Nombre", row);
                this.ItemsSourceProperty.Add(new CommItem(cod, name, this));
            }

            this._F2KeyBinding = new Command_F2KeyBinding(this);
        }

        #region fields
        private SideModel _model;
        #endregion

        #region commands
        private Command_F2KeyBinding _F2KeyBinding;
        public ICommand F2KeyBinding { get { return _F2KeyBinding; } }
        #endregion

        #region datatable helpers
        /// <summary>
        /// Gets value of type T from table column and row using ConvertFromDBVal. Doesn't check if value is of type T.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="column"></param>
        /// <param name="row"></param>
        /// <returns></returns>
        private T GetValueFromTable<T>(string column, DataRow row)
        {
            return ConvertFromDBVal<T>(row[column]);//this._model.Table.Rows[0][column]);
        }
        /// <summary>
        /// Convert DBVal to T. Doesn't check if data value is of type T.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        private T ConvertFromDBVal<T>(object obj)
        {
            if (obj == null || obj == DBNull.Value)
            {
                return default(T); // returns the default value for the type
            }
            else
            {
                return (T)obj;
            }
        }
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
        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ItemsSourcePropertyProperty = DependencyProperty.Register("ItemsSourceProperty",
            typeof(ObservableCollection<CommItem>), typeof(SideTool),
            new PropertyMetadata(OnItemsSourcePropertyChanged));

        public ObservableCollection<CommItem> ItemsSourceProperty
        {
            get { return (ObservableCollection<CommItem>)GetValue(ItemsSourcePropertyProperty); }
            set { SetValue(ItemsSourcePropertyProperty, value); }
        }

        public static void OnItemsSourcePropertyChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            SideTool control = source as SideTool;

            var old = e.OldValue as ObservableCollection<TabItem>;

            if (old != null)
            {
                // Unsubscribe from CollectionChanged on the old collection
                old.CollectionChanged -= control.ItemsSource_CollectionChanged;
            }

            var n = e.NewValue as ObservableCollection<TabItem>;

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
                foreach (CommItem item in e.NewItems)
                {
                    this.ItemsSourceProperty.Add(item);
                }
            }

            if (e.OldItems != null)
            {
                foreach (CommItem item in e.OldItems)
                {
                    this.ItemsSourceProperty.RemoveAt(e.OldStartingIndex);
                }
            }
        }

        // Using a DependencyProperty as the backing store for ExIsExpanded.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ExIsExpandedProperty = DependencyProperty.Register("ExIsExpanded",
            typeof(bool), typeof(SideTool), new PropertyMetadata(OnExIsExpandedChanged));

        public bool ExIsExpanded
        {
            get { return (bool)GetValue(ExIsExpandedProperty); }
            set { SetValue(ExIsExpandedProperty, value); }
        }

        public static void OnExIsExpandedChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            SideTool control = source as SideTool;
            bool newIsExp = (bool)e.NewValue;

            if (e.NewValue != null)
            {
                VMMain vm = App.Current.MainWindow.DataContext as VMMain;
                vm.ZIndex = (newIsExp == true ? -1 : 1);

                if (newIsExp != control.ExIsExpanded)
                    control.ExIsExpanded = newIsExp;
            }
        }
        #endregion
    }

    /// <summary>
    /// Modelview for sidetool treeviewitem
    /// </summary>
    public class CommItem : DependencyObject, INotifyPropertyChanged
    {
        public CommItem() { }

        public CommItem(SideTool parent) { this._buttonClick = new Command_ItemButtonClick(this, parent); }

        public CommItem(int comCod, string comName, SideTool parent)
        {
            this.ComCod = comCod;
            this.ItemHeader = string.Format("{0} - {1}", comCod, comName);
            this.Name += this.ItemHeader;
            this._buttonClick = new Command_ItemButtonClick(this, parent);
            //this.Header = this.ItemHeaderProperty;

            this.FillHierarchy(ref parent);
        }

        #region fields
        private int _comCod;
        private string _name = "ComItem";
        private TabType _type = TabType.None;
        private ObservableCollection<CommItem> _Hierarchy;
        private string _itemHeader;
        private Command_ItemButtonClick _buttonClick;
        private bool _isSelected = false;
        private bool _isExpanded = false;
        #endregion

        #region properties
        public int ComCod
        {
            get { return _comCod; }
            set { _comCod = value; }
        }
        public string Name
        {
            get { return _name; }
            set
            {
                _name = "ComItem" + value;
                NotifyPropChanged("Name");
            }
        }
        public int Type
        {
            get { return (int)_type; }
            set
            {
                _type = (TabType)value;
            }
        }
        public TabType tabType
        {
            get { return _type; }
            set
            {
                _type = value;
            }
        }
        public ObservableCollection<CommItem> Hierarchy { get { return _Hierarchy; } }
        public string ItemHeader
        {
            get { return _itemHeader; }
            set
            {
                _itemHeader = value;
                NotifyPropChanged("ItemHeader");
            }
        }
        public ICommand ButtonClick { get { return _buttonClick; } }
        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                if (value != _isSelected)
                {
                    _isSelected = value;
                    this.NotifyPropChanged("IsSelected");
                }
            }
        }
        public bool IsExpanded
        {
            get { return _isExpanded; }
            set
            {
                if (value != _isExpanded)
                {
                    _isExpanded = value;

                    if (value == true)
                        this.CloseAllItemsExceptThis();

                    this.NotifyPropChanged("IsExpanded");
                }
            }
        }
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
        private void CloseAllItemsExceptThis()
        {
            TreeView tv = (App.Current.MainWindow as MainWindow).SideTool.TreeView;

            foreach (CommItem item in tv.Items)
            {
                if (item != this)
                    item.IsExpanded = false;
            }
        }
        private void FillHierarchy(ref SideTool parent)
        {
            /*
                               <TreeViewItem Header="Comunidad" Style="{StaticResource triggerCdades}"/>
                               <TreeViewItem Header="Propietarios" Style="{StaticResource triggerProps}"/>
                               <TreeViewItem Header="Libro Mayor" Style="{StaticResource triggerMayor}"/>
                               <TreeViewItem Header="Libro Diario" Style="{StaticResource triggerDiario}"/>
            */

            this._Hierarchy = new ObservableCollection<CommItem>();
            CommItem item = new CommItem(parent);
            item.ItemHeader = "Comunidad";
            //item.Style = parent.Resources["triggerCdades"] as Style;
            item.ComCod = this.ComCod;
            item.tabType = TabType.Cdad;
            this.Hierarchy.Add(item);

            item = new CommItem(parent);
            item.ItemHeader = "Propietarios";
            //item.Style = parent.Resources["triggerProps"] as Style;
            item.ComCod = this.ComCod;
            item.tabType = TabType.Props;
            this.Hierarchy.Add(item);

            item = new CommItem(parent);
            item.ItemHeader = "Libro Mayor";
            //item.Style = parent.Resources["triggerMayor"] as Style;
            item.ComCod = this.ComCod;
            item.tabType = TabType.Mayor;
            this.Hierarchy.Add(item);

            item = new CommItem(parent);
            item.ItemHeader = "Libro Diario";
            //item.Style = parent.Resources["triggerDiario"] as Style;
            item.ComCod = this.ComCod;
            item.tabType = TabType.Diario;
            this.Hierarchy.Add(item);
            NotifyPropChanged("Hierarchy");
        }
        #endregion
    }
    /// <summary>
    /// Command for sub_commitem click: open new tab of type (TabType _Item._type) in abletabcontrol
    /// </summary>
    public class Command_ItemButtonClick : ICommand
    {
        private CommItem _Item;
        private SideTool _Side;

        public Command_ItemButtonClick(CommItem item, SideTool side)
        {
            _Item = item;
            _Side = side;
        }

        public bool CanExecute(object parameter)
        {
            return true;//(this._Item.tabType != TabType.None);
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        /*public T GetFirstParentOfType<T>(FrameworkElement child) where T : FrameworkElement
        {
            var parent = VisualTreeHelper.GetParent(child);

            while (!(parent is T) && !(parent is SideTool))
            {
                parent = VisualTreeHelper.GetParent(parent);
            }

            if (parent is T) return (T)parent;
            else return null;
        }*/

        public void Execute(object parameter)
        {
            this._Item.IsExpanded = true;

            if (this._Item.Hierarchy != null)
                return;

            this._Side.ExIsExpanded = false;
            VMMain mainVM = App.Current.MainWindow.DataContext as VMMain;
            mainVM.LastComCod = this._Item.ComCod;
            mainVM.AddTab(this._Item.tabType);
        }
    }

    public class Command_F2KeyBinding : ICommand
    {
        private SideTool _Side;

        public Command_F2KeyBinding(SideTool side)
        {
            _Side = side;
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
            this._Side.ExIsExpanded = !this._Side.ExIsExpanded;
        }
    }
}