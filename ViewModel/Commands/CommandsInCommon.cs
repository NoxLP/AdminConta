using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using AdConta.ViewModel;

namespace AdConta.ViewModel
{
    #region modify/save commands
    /// <summary>
    /// Modify button command. Switch the readonly property of all textboxes, and controls that accepts editing, in the caller tab, becoming all editable.
    /// Can be called from Abletabcontrol's tabs.
    /// </summary>
    public class Command_ModifyButtonClick : ICommand
    {
        private aVMTabBase _tab;

        public Command_ModifyButtonClick(aVMTabBase tab)
        {
            this._tab = tab;
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
            this._tab.ModifyRecord(false);
        }
    }
    /// <summary>
    /// Save button command. Save changes made to controls after activating modify command.
    /// Can be called from Abletabcontrol's tabs.
    /// </summary>
    public class Command_SaveButtonClick : ICommand
    {
        private aVMTabBase _tab;

        public Command_SaveButtonClick(aVMTabBase tab)
        {
            this._tab = tab;
        }

        public bool CanExecute(object parameter)
        {
            return this._tab.CanModifyRecord();
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public void Execute(object parameter)
        {
            this._tab.ModifyRecord(true);
        }
    }
    #endregion

    #region bank account commands
    /// <summary>
    /// Command for copy bank account number TO clipboard.
    /// Can be called from Abletabcontrol's tabs.
    /// </summary>
    public class Command_CopyAccount : ICommand
    {
        private aVMTabBase _tab;

        public Command_CopyAccount(aVMTabBase tab)
        {
            this._tab = tab;
        }

        public bool CanExecute(object parameter)
        {
            return this._tab.CanCopyAccount();
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public void Execute(object parameter)
        {
            this._tab.CopyAccountToClipboard();
        }
    }
    /// <summary>
    /// Command for paste bank account number FROM clipboard.
    /// Can be called from Abletabcontrol's tabs.
    /// </summary>
    public class Command_PasteAccount : ICommand
    {
        private aVMTabBase _tab;

        public Command_PasteAccount(aVMTabBase tab)
        {
            this._tab = tab;
        }

        public bool CanExecute(object parameter)
        {
            return (this._tab.CanModifyRecord() && !string.IsNullOrEmpty(Clipboard.GetText()));
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public void Execute(object parameter)
        {
            this._tab.PasteAccountFromClipboard();
        }
    }
    #endregion

    #region next/previous record
    /// <summary>
    /// Get next comunidad record.
    /// </summary>
    public class Command_NextRecord_Cdad : ICommand
    {
        private aVMTabBase _tab;

        public Command_NextRecord_Cdad(aVMTabBase tab)
        {
            this._tab = tab;
        }

        public bool CanExecute(object parameter)
        {
            return this._tab.ComMaxCod > this._tab.TabComCod;
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public void Execute(object parameter)
        {
            this._tab.OnChangedCod(this._tab.TabComCod + 1);
        }
    }
    /// <summary>
    /// Get previous comunidad record.
    /// </summary>
    public class Command_PrevRecord_Cdad : ICommand
    {
        private aVMTabBase _tab;

        public Command_PrevRecord_Cdad(aVMTabBase tab)
        {
            this._tab = tab;
        }

        public bool CanExecute(object parameter)
        {
            return this._tab.ComMinCod < this._tab.TabComCod;
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public void Execute(object parameter)
        {
            this._tab.OnChangedCod(this._tab.TabComCod - 1);
        }
    }
    /// <summary>
    /// Get next mayor record.
    /// </summary>
    public class Command_NextRecord_Mayor : ICommand
    {
        private aVMTabBase _tab;

        public Command_NextRecord_Mayor(aVMTabBase tab)
        {
            this._tab = tab;
        }

        public bool CanExecute(object parameter)
        {

            return !this._tab.IsLastAccount();
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public void Execute(object parameter)
        {

        }
    }
    /// <summary>
    /// Get previous mayor record.
    /// </summary>
    public class Command_PrevRecord_Mayor : ICommand
    {
        private aVMTabBase _tab;

        public Command_PrevRecord_Mayor(aVMTabBase tab)
        {
            this._tab = tab;
        }

        public bool CanExecute(object parameter)
        {
            return !string.IsNullOrEmpty(Clipboard.GetText());
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public void Execute(object parameter)
        {
            this._tab.PasteAccountFromClipboard();
        }
    }
    #endregion
}
