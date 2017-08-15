using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Extensions;

namespace AdConta.SideTool
{
    /// <summary>
    /// Modelview for sidetool treeviewitem
    /// </summary>
    public class SideToolTabItem : DependencyObject, INotifyPropertyChanged
    {
        public SideToolTabItem() { }

        public SideToolTabItem(VMSideTool parent) { this._ButtonClick = new Command_TabItemButtonClick(this, parent); }

        public SideToolTabItem(int comCod, string comName, VMSideTool parent)
        {
            this.ComCod = comCod;
            this.ItemHeader = $"{comCod} - {comName}";
            this.Name += this.ItemHeader;
            this._ButtonClick = new Command_TabItemButtonClick(this, parent);
            //this.Header = this.ItemHeaderProperty;

            this.FillHierarchy(ref parent);
        }

        public SideToolTabItem(VMSideTool parent, string header, int comCod, TabType tabType)
        {
            this.ComCod = comCod;
            this.ItemHeader = header;
            this.Name += header;
            this._ButtonClick = new Command_TabItemButtonClick(this, parent);
            this.tabType = tabType;
        }

        #region fields
        private int _ComCod;
        private string _Name = "TabItem";
        private TabType _Type = TabType.None;
        private ObservableCollection<SideToolTabItem> _Hierarchy;
        private string _ItemHeader;
        private Command_TabItemButtonClick _ButtonClick;
        private bool _IsSelected = false;
        private bool _IsExpanded = false;
        #endregion

        #region properties
        public int ComCod
        {
            get { return _ComCod; }
            set { _ComCod = value; }
        }
        public string Name
        {
            get { return _Name; }
            set
            {
                _Name = "TabItem" + value;
                NotifyPropChanged("Name");
            }
        }
        public int Type
        {
            get { return (int)_Type; }
            set
            {
                _Type = (TabType)value;
            }
        }
        public TabType tabType
        {
            get { return _Type; }
            set
            {
                _Type = value;
            }
        }
        public ObservableCollection<SideToolTabItem> Hierarchy { get { return _Hierarchy; } }
        public string ItemHeader
        {
            get { return _ItemHeader; }
            set
            {
                _ItemHeader = value;
                NotifyPropChanged("ItemHeader");
            }
        }
        public ICommand ButtonClick { get { return _ButtonClick; } }
        public bool IsSelected
        {
            get { return _IsSelected; }
            set
            {
                if (value != _IsSelected)
                {
                    _IsSelected = value;
                    this.NotifyPropChanged("IsSelected");
                }
            }
        }
        public bool IsExpanded
        {
            get { return _IsExpanded; }
            set
            {
                if (value != _IsExpanded)
                {
                    _IsExpanded = value;

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

            foreach (SideToolTabItem item in tv.Items)
            {
                if (item != this)
                    item.IsExpanded = false;
            }
        }
        internal void AddToHierarchy(SideToolTabItem item)
        {
            if (this._Hierarchy == null)
                throw new InvalidOperationException("SideToolItem.AddToHierarchy: La colección de SideToolItems no está inicializada. No se pueden añadir items");
            if (item == null)
                throw new ArgumentNullException();
            this._Hierarchy.Add(item);
        }
        private void FillHierarchy(ref VMSideTool parent)
        {
            /*
                               <TreeViewItem Header="Comunidad" Style="{StaticResource triggerCdades}"/>
                               <TreeViewItem Header="Propietarios" Style="{StaticResource triggerProps}"/>
                               <TreeViewItem Header="Libro Mayor" Style="{StaticResource triggerMayor}"/>
                               <TreeViewItem Header="Libro Diario" Style="{StaticResource triggerDiario}"/>
            */

            this._Hierarchy = new ObservableCollection<SideToolTabItem>();
            //SideToolTabItem item = new SideToolTabItem(parent);
            //item.ItemHeader = "Comunidad";
            ////item.Style = parent.Resources["triggerCdades"] as Style;
            //item.ComCod = this.ComCod;
            //item.tabType = TabType.Cdad;
            SideToolTabItem item = new SideToolTabItem(parent, "Comunidad", this.ComCod, TabType.Cdad);
            this.Hierarchy.Add(item);

            //item = new SideToolTabItem(parent);
            //item.ItemHeader = "Propietarios";
            ////item.Style = parent.Resources["triggerProps"] as Style;
            //item.ComCod = this.ComCod;
            //item.tabType = TabType.Props;
            item = new SideToolTabItem(parent, "Propietarios", this.ComCod, TabType.Props);
            this.Hierarchy.Add(item);

            //item = new SideToolTabItem(parent);
            //item.ItemHeader = "Libro Mayor";
            ////item.Style = parent.Resources["triggerMayor"] as Style;
            //item.ComCod = this.ComCod;
            //item.tabType = TabType.Mayor;
            item = new SideToolTabItem(parent, "Libro Mayor", this.ComCod, TabType.Mayor);
            this.Hierarchy.Add(item);

            //item = new SideToolTabItem(parent);
            //item.ItemHeader = "Libro Diario";
            ////item.Style = parent.Resources["triggerDiario"] as Style;
            //item.ComCod = this.ComCod;
            //item.tabType = TabType.Diario;
            item = new SideToolTabItem(parent, "Libro Diario", this.ComCod, TabType.Diario);
            this.Hierarchy.Add(item);
            NotifyPropChanged("Hierarchy");
        }
        #endregion
    }

    /// <summary>
    /// Command for sub_commitem click: select comunidad
    /// </summary>
    public class Command_TabItemButtonClick : ICommand
    {
        private SideToolTabItem _Item;
        private VMSideTool _Side;

        public Command_TabItemButtonClick(SideToolTabItem item, VMSideTool side)
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

        public void Execute(object parameter)
        {
            this._Item.IsExpanded = true;

            //Si tabType==null el item es de comunidad, no debe abrir tab sino expandirse. Si Hierarchy!=null el item es de tab pero ya está expandido
            if (this._Item.Hierarchy != null || this._Item.tabType == TabType.None)
                return;

            //this._Side.TaskCargando.Add(NotifyTask.Create(
            //    this._Side.OnTabItemButtonClickAsync(this._Item.ComCod, this._Item.tabType),
            //    x => this._Side.TaskCargando.Remove(x)));
            this._Side.AddToTaskCargando(this._Side.OnTabItemButtonClickAsync(this._Item.ComCod, this._Item.tabType));
        }
    }
}
