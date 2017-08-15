using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using AdConta.Models;
using System.Windows.Input;
using Extensions;

namespace AdConta.SideTool
{
    public class SideToolEjercicioItem : DependencyObject, INotifyPropertyChanged
    {
        public SideToolEjercicioItem(VMSideTool parent, EjercicioDLOParaSideTool DLO)
        {
            this._Ejercicio = DLO;
            this._ItemHeader = $"{DLO.FechaComienzo} - {DLO.FechaFinal}";
            this._Name += this._ItemHeader;
            this._ButtonClick = new Command_EjercicioItemButtonClick(this, parent);
        }

        #region fields
        private EjercicioDLOParaSideTool _Ejercicio;
        private string _Name = "EjerItem";
        //private ObservableCollection<SideToolTabItem> _Hierarchy;
        private string _ItemHeader;
        private Command_EjercicioItemButtonClick _ButtonClick;
        private bool _IsSelected = false;
        #endregion

        #region properties
        public EjercicioDLOParaSideTool EjercicioDLO
        {
            get { return _Ejercicio; }
            private set { _Ejercicio = value; }
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
            get { return (int)TabType.None; }
        }
        public TabType tabType
        {
            get { return TabType.None; }
        }
        //public ObservableCollection<SideToolTabItem> Hierarchy { get { return _Hierarchy; } }
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
                    ButtonClick.Execute(null);
                    this.NotifyPropChanged("IsSelected");
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
    }

    /// <summary>
    /// Command for sub_commitem click: select ejercicio
    /// </summary>
    public class Command_EjercicioItemButtonClick : ICommand
    {
        private SideToolEjercicioItem _Item;
        private VMSideTool _Side;

        public Command_EjercicioItemButtonClick(SideToolEjercicioItem item, VMSideTool side)
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
            //this._Side.TaskCargando.Add(NotifyTask.Create(
            //    this._Side.OnEjercicioItemButtonClickAsync(this._Item.EjercicioDLO),
            //    x => this._Side.TaskCargando.Remove(x)));
            this._Side.AddToTaskCargando(this._Side.OnEjercicioItemButtonClickAsync(this._Item.EjercicioDLO));
        }
    }
}
