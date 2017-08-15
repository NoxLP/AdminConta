using System;
using System.Dynamic;
using System.Windows;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Collections.Specialized;

namespace AdConta.ViewModel
{
    /// <summary>
    /// Base for all Abletabcontrol tabs's viewmodels.
    /// </summary>
    public abstract class aVMTabBase : DataTableHelperVMBase/*<- OJO esto ya no deberia ser necesario*/, IPublicNotify, IVMTabBaseWithUoW, IEquatable<aVMTabBase>
    {
        public aVMTabBase()
        {
            this._TaskCargando = new ObservableCollection<INotifyTask>();
            this._TaskCargando.CollectionChanged += TaskCargando_OnCollectionChanged;

            NotifyTask initializeOwnersTask = NotifyTask.Create(InitializeOwners(), x =>
            {
                this._TaskCargando.Remove(x);
                NotifyPropChanged("TaskCargando");
            });
            this._TaskCargando.Add(initializeOwnersTask);
        }
        
        #region fields
        private int _TabCodigoComunidad = 0;
        protected int _TabCodigoEjercicio = 0;
        private TabType _TabType;
        private string _Header;
        private bool _IsSelected = true;
        private ObservableCollection<INotifyTask> _TaskCargando = new ObservableCollection<INotifyTask>();
        #endregion

        #region properties
        public ReadOnlyObservableCollection<INotifyTask> TaskCargando
        {
            get { return new ReadOnlyObservableCollection<INotifyTask>(this._TaskCargando); }
            //set
            //{
            //    if (this._TaskCargando != value)
            //    {
            //        this._TaskCargando = value;
            //        NotifyPropChanged("TaskCargando");
            //    }
            //}
        }
        public bool AnyTaskCargando
        {
            get { return this.TaskCargando.Any(notifyTask => !notifyTask.IsCompleted); }
        }
        public int TabCodigoComunidad
        {
            get { return this._TabCodigoComunidad; }
            set
            {
                if (value != this._TabCodigoComunidad)
                {
                    this._TabCodigoComunidad = value;
                    OnChangedComunidad(value);
                    this.NotifyPropChanged("TabComCod");
                }
            }
        }
        public int TabCodigoEjercicio
        {
            get { return this._TabCodigoEjercicio; }
            set
            {
                if (this._TabCodigoEjercicio != value)
                {
                    this._TabCodigoEjercicio = value;
                    OnChangedEjercicio(this._TabCodigoEjercicio);
                    this.NotifyPropChanged("TabEjerCod");
                }
            }
        }
        public TabType TabType
        {
            get { return this._TabType; }
            set { this._TabType = value; }
        }
        public string Header
        {
            get { return this._Header; }
            set
            {
                if (value != this._Header)
                {
                    this._Header = value;
                    this.NotifyPropChanged("Header");
                }
            }
        }
        public bool IsSelected
        {
            get { return this._IsSelected; }
            set
            {
                if (value != this._IsSelected)
                {
                    this._IsSelected = value;
                    this.NotifyPropChanged("IsSelected");
                }
            }
        }
        public int TabIndex { get; private set; }

        #region virtual model properties
        public virtual int ComMaxCod
        {
            get { throw new NotImplementedException(); }
        }
        public virtual int ComMinCod
        {
            get { throw new NotImplementedException(); }
        }
        #endregion
        #endregion

        #region PropertyChanged
        public void PublicNotifyPropChanged(string propName)
        {
            this.NotifyPropChanged(propName);
        }
        #endregion
        
        #region common commands virtual methods
        //Methods called by common commands. Virtual, so each tab can define their method.
        /// <summary>
        /// Method called by modify and save commands.
        /// Modify command: Switch the readonly property of all textboxes(except TabTBCod), and controls that accepts editing, in the caller tab, becoming all editable.        
        /// Save Command(ONLY IF saveChanges == true): Save changes made to controls after activating modify command.
        /// </summary>
        /// <param name="saveChanges">HAVE to be true IF called by SaveCommand, so the method save changes too</param>
        public virtual void ModifyRecord(bool saveChanges)
        {
            throw new NotImplementedException();
        }
        public virtual bool CanModifyRecord()
        {
            throw new NotImplementedException();
        }
        public virtual bool CanCopyAccount()
        {
            throw new NotImplementedException();
        }
        public virtual void CopyAccountToClipboard()
        {
            throw new NotImplementedException();
        }
        public virtual void PasteAccountFromClipboard()
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// Order model to get data of new Cod(NOT database Id, just a new record of whatever the tab is handling).
        /// Use with next/prev record commands.
        /// </summary>
        /// <param name="newCodigoComunidad"></param>
        public abstract void OnChangedComunidad(int newCodigoComunidad);
        public abstract void OnChangedEjercicio(int newCodigoEjercicio);
        public virtual bool IsLastAccount()
        {
            throw new NotImplementedException();
        }
        public virtual bool IsFirstAccount()
        {
            throw new NotImplementedException();
        }
        #endregion

        #region other virtual methods
        /// <summary>
        /// Called when a new Cdad record is added.
        /// </summary>
        public virtual void UpdateMinMaxCods()
        {
            throw new NotImplementedException();
        }
        #endregion

        #region helpers
        public IEnumerable<int> GetOwners()
        {
            return new int[] { this._TabCodigoComunidad, this._TabCodigoEjercicio };
        }
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        /// <summary>
        /// Retrieve owners messages
        /// </summary>
        /// <returns></returns>
        protected async Task InitializeOwners()
        {
            int? comunidad = (int?)Messenger.Messenger.SearchMsg("LastComCod");
            int? ejercicio = (int?)Messenger.Messenger.SearchMsg("LastEjerCod");
            int? año = (int?)Messenger.Messenger.SearchMsg("LastAñoEjer");

            if (comunidad == null) throw new ArgumentException("No se puede abrir el mayor de esta comunidad. No ha seleccionado un código, el código es erróneo o la Comunidad no existe");
            else if (ejercicio == null) throw new ArgumentException("No se puede abrir el mayor de este ejercicio. No ha seleccionado un ejercicio o el ejercicio no existe");
            else if (año == null) throw new ArgumentException("No se puede abrir el mayor de este ejercicio. No ha seleccionado un ejercicio o el ejercicio no existe");

            InitializeCodigosOwners((int)comunidad, (int)ejercicio, (int)año);
        }
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        /// <summary>
        /// Only to initialize the class from a child constructor.
        /// </summary>
        /// <param name="CodigoComunidad"></param>
        public void InitializeCodigosOwners(int CodigoComunidad, int CodigoEjercicio, int AñoEjercicio)
        {
            this._TabCodigoComunidad = CodigoComunidad;
            this._TabCodigoEjercicio = CodigoEjercicio;
        }
        /// <summary>
        /// Used only by AbleTabControl. Store tab index for compare tabs.
        /// </summary>
        /// <param name="nuevoTabIndex"></param>
        public void ChangeTabIndex(int nuevoTabIndex)
        {
            this.TabIndex = nuevoTabIndex;
        }
        #endregion

        #region IEquatable
        public bool Equals(aVMTabBase other)
        {
            return this.TabIndex == other.TabIndex && this.TabCodigoComunidad == other.TabCodigoComunidad && this.TabType == other.TabType;
        }
        public override int GetHashCode()
        {
            int hash = base.GetHashCode();
            hash = (hash * 7) + this.TabCodigoComunidad.GetHashCode();
            hash = (hash * 7) + this.TabType.GetHashCode();
            hash = (hash * 7) + this.TabIndex.GetHashCode();
            return hash;
        }
        #endregion

        #region task cargando collection changed
        private void NotifyAllTaskCargandoProperties()
        {
            NotifyPropChanged("AnyTaskCargando");
            NotifyPropChanged("TaskCargando");
        }
        public bool AddToTaskCargando(Task task, bool removeFromCollectionWhenCompleted = true)
        {
            Action<INotifyTask> removeWhenCompleted = null;
            if (removeFromCollectionWhenCompleted)
                removeWhenCompleted = nTask => this._TaskCargando.Remove(nTask);

            var notifyTask = NotifyTask.Create(task, removeWhenCompleted);
            if (this._TaskCargando.Contains(notifyTask))
                return false;

            this._TaskCargando.Add(notifyTask);
            NotifyAllTaskCargandoProperties();
            return true;
        }
        public bool AddToTaskCargando(INotifyTask notifyTask)
        {
            if (this._TaskCargando.Contains(notifyTask))
                return false;

            this._TaskCargando.Add(notifyTask);
            NotifyAllTaskCargandoProperties();
            return true;
        }
        public void RemoveFromTaskCargando(INotifyTask notifyTask)
        {
            if (!this._TaskCargando.Contains(notifyTask))
                return;

            this._TaskCargando.Remove(notifyTask);
        }

        private void TaskCargando_OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            ObservableCollection<INotifyTask> taskCargando = (ObservableCollection<INotifyTask>)sender;

            if(e.OldItems != null)
            {
                NotifyAllTaskCargandoProperties();
            }
        }
        #endregion

        #region UoW
        public abstract Task CleanUnitOfWorkAsync();
        public abstract void InitializeUoW();
        #endregion
    }
}