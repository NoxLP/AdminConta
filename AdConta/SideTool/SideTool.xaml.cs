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
    public partial class SideTool : UserControl
    {
        public SideTool()
        {
            InitializeComponent();

            //this._model = new SideModel();
            //this.ItemsSourceProperty = new ObservableCollection<SideToolTabItem>();

            //foreach (DataRow row in this._model.Table.Rows)
            //{
            //    int cod = this.GetValueFromTable<int>("Codigo", row);
            //    string name = this.GetValueFromTable<string>("Nombre", row);
            //    this.ItemsSourceProperty.Add(new SideToolTabItem(cod, name, this));
            //}

            //this._F2KeyBinding = new Command_F2KeyBinding(this);
        }
    }

    #region old
    //#region fields
    //private SideModel _model;
    //#endregion

    //#region commands
    //private Command_F2KeyBinding _F2KeyBinding;
    //public ICommand F2KeyBinding { get { return _F2KeyBinding; } }
    //#endregion

    //#region datatable helpers
    ///// <summary>
    ///// Gets value of type T from table column and row using ConvertFromDBVal. Doesn't check if value is of type T.
    ///// </summary>
    ///// <typeparam name="T"></typeparam>
    ///// <param name="column"></param>
    ///// <param name="row"></param>
    ///// <returns></returns>
    //private T GetValueFromTable<T>(string column, DataRow row)
    //{
    //    return ConvertFromDBVal<T>(row[column]);//this._model.Table.Rows[0][column]);
    //}
    ///// <summary>
    ///// Convert DBVal to T. Doesn't check if data value is of type T.
    ///// </summary>
    ///// <typeparam name="T"></typeparam>
    ///// <param name="obj"></param>
    ///// <returns></returns>
    //private T ConvertFromDBVal<T>(object obj)
    //{
    //    if (obj == null || obj == DBNull.Value)
    //    {
    //        return default(T); // returns the default value for the type
    //    }
    //    else
    //    {
    //        return (T)obj;
    //    }
    //}
    //#endregion

    //#region PropertyChanged
    //public event PropertyChangedEventHandler PropertyChanged = delegate { };

    //protected void NotifyPropChanged(string propertyName)
    //{
    //    if (PropertyChanged != null)
    //    {
    //        PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
    //    }
    //}
    //#endregion

    //#region dependency properties
    //// Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
    //public static readonly DependencyProperty ItemsSourcePropertyProperty = DependencyProperty.Register("ItemsSourceProperty",
    //    typeof(ObservableCollection<SideToolTabItem>), typeof(SideTool),
    //    new PropertyMetadata(OnItemsSourcePropertyChanged));

    //public ObservableCollection<SideToolTabItem> ItemsSourceProperty
    //{
    //    get { return (ObservableCollection<SideToolTabItem>)GetValue(ItemsSourcePropertyProperty); }
    //    set { SetValue(ItemsSourcePropertyProperty, value); }
    //}

    //public static void OnItemsSourcePropertyChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
    //{
    //    SideTool control = source as SideTool;

    //    var old = e.OldValue as ObservableCollection<TabItem>;

    //    if (old != null)
    //    {
    //        // Unsubscribe from CollectionChanged on the old collection
    //        old.CollectionChanged -= control.ItemsSource_CollectionChanged;
    //    }

    //    var n = e.NewValue as ObservableCollection<TabItem>;

    //    if (n != null)
    //    {
    //        // Subscribe to CollectionChanged on the new collection
    //        n.CollectionChanged += control.ItemsSource_CollectionChanged;
    //    }
    //}

    //private void ItemsSource_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    //{
    //    if (e.Action == NotifyCollectionChangedAction.Reset) this.ItemsSourceProperty.Clear();

    //    if (e.NewItems != null)
    //    {
    //        foreach (SideToolTabItem item in e.NewItems)
    //        {
    //            this.ItemsSourceProperty.Add(item);
    //        }
    //    }

    //    if (e.OldItems != null)
    //    {
    //        foreach (SideToolTabItem item in e.OldItems)
    //        {
    //            this.ItemsSourceProperty.RemoveAt(e.OldStartingIndex);
    //        }
    //    }
    //}

    //// Using a DependencyProperty as the backing store for ExIsExpanded.  This enables animation, styling, binding, etc...
    //public static readonly DependencyProperty ExIsExpandedProperty = DependencyProperty.Register("ExIsExpanded",
    //    typeof(bool), typeof(SideTool), new PropertyMetadata(OnExIsExpandedChanged));

    //public bool ExIsExpanded
    //{
    //    get { return (bool)GetValue(ExIsExpandedProperty); }
    //    set { SetValue(ExIsExpandedProperty, value); }
    //}

    //public static void OnExIsExpandedChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
    //{
    //    SideTool control = source as SideTool;
    //    bool newIsExp = (bool)e.NewValue;

    //    if (e.NewValue != null)
    //    {
    //        VMMain vm = App.Current.MainWindow.DataContext as VMMain;
    //        vm.ZIndex = (newIsExp == true ? -1 : 1);

    //        if (newIsExp != control.ExIsExpanded)
    //            control.ExIsExpanded = newIsExp;
    //    }
    //}
    //#endregion
    #endregion
}