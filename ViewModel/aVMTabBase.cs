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
    interface IVMTabBaseWithUoW
    {
        Task CleanUnitOfWork();
        Task InitUoWAsync();
    }
    /// <summary>
    /// Base for all Abletabcontrol tabs's viewmodels.
    /// </summary>
    public abstract class aVMTabBase : DataTableHelperVMBase/*<- OJO esto ya no deberia ser necesario*/, IPublicNotify, IVMTabBaseWithUoW, IEquatable<aVMTabBase>
    {
        #region fields
        private int _TabComCod = 0;
        private TabType _Type;
        private string _Header;
        private bool _IsSelected = true;
        #endregion

        #region properties
        public int TabComCod
        {
            get { return this._TabComCod; }
            set
            {
                if (value != this._TabComCod)
                {
                    this._TabComCod = value;
                    this._Header = this.GetHeader();
                    OnChangedCod(value);
                    this.NotifyPropChanged("TabComCod");
                }
            }
        }
        public TabType Type
        {
            get { return this._Type; }
            set { this._Type = value; }
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

        //Methods called by common commands. Virtual, so each tab can define their method.
        #region common commands virtual methods
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
        /// <param name="newCod"></param>
        public virtual void OnChangedCod(int newCod)
        {
            throw new NotImplementedException();
        }
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
        /// <summary>
        /// Gets tab header with tabbase property TabCodCom and Type
        /// </summary>
        /// <returns></returns>
        private string GetHeader()
        {
            TabHeader TabHeaders = new TabHeader();
            return string.Format("{0} - {1}", this.TabComCod.ToString(), TabHeaders[this.Type]);
        }
        /// <summary>
        /// Only to initialize the class from a child constructor.
        /// </summary>
        /// <param name="cod"></param>
        public void InitializeComcod(int cod)
        {
            this._TabComCod = cod;
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
            return this.TabIndex == other.TabIndex && this.TabComCod == other.TabComCod && this.Type == other.Type;
        }
        public override int GetHashCode()
        {
            int hash = base.GetHashCode();
            hash = (hash * 7) + this.TabComCod.GetHashCode();
            hash = (hash * 7) + this.Type.GetHashCode();
            hash = (hash * 7) + this.TabIndex.GetHashCode();
            return hash;
        }
        #endregion

        #region UoW
        public abstract Task CleanUnitOfWork();
        public abstract Task InitUoWAsync();
        #endregion
    }
}