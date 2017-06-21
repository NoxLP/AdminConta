using System;
using System.ComponentModel;
using System.Windows;

namespace AdConta.ViewModel
{
    public class ViewModelBase : INotifyPropertyChanged
    {
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

    public interface IPublicNotify
    {
        void PublicNotifyPropChanged(string propName);
    }
}
