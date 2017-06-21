using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Data;
using System.Reflection;
using AdConta.Models;
using AdConta.ViewModel;
using TabbedExpanderCustomControl;
using Repository;
using Extensions;

namespace AdConta
{
    /// <summary>
    /// Viewmodel for tabs of type Cdades.
    /// </summary>
    public class VMTabCdad : aTabsWithTabExpVM//, IDataErrorInfo
    {
        public VMTabCdad() 
        {
            this.Type = TabType.Cdad;
            InitUoWAsync().Forget().ConfigureAwait(false);

            base.InitializeComcod((App.Current.MainWindow.DataContext as VMMain).LastComCod);
            this._model = new TabCdadModel(this.TabComCod);
            this._model.CuentaBancaria = new CuentaBancaria();
            this._model.CuentaBancaria.AccountNumber = GetValueFromTable<string>("Cuenta");
            this._TabPanelWidth = 10;
            
            this._ModifyCommand = new Command_ModifyButtonClick(this);
            this._SaveCommand = new Command_SaveButtonClick(this);
            this._CopyAccount = new Command_CopyAccount(this);
            this._PasteAccount = new Command_PasteAccount(this);
            this._NextRecord = new Command_NextRecord_Cdad(this);
            this._PrevRecord = new Command_PrevRecord_Cdad(this);
        }

        #region fields
        private TabCdadModel _model;
        //private DataTable _ComTable;
        private bool _ReadOnlyAll = true;
        private double _TabPanelWidth;

        #region tabbed expander
        private int _TopTabbedExpanderSelectedIndex;
        private int _BottomTabbedExpanderSelectedIndex;
        #endregion
        #endregion

        #region properties
        public UnitOfWork UOW { get; private set; }
        public ComunidadRepository ComunidadRepo { get; private set; }
        public bool ReadOnlyALL
        {
            get { return this._ReadOnlyAll; }
            set
            {
                if (value != this.ReadOnlyALL)
                {
                    this._ReadOnlyAll = value;
                    NotifyPropChanged("ReadOnlyAll");
                    NotifyPropChanged("ModifyDataAllowed");
                }
            }
        }
        public bool ModifyDataAllowed
        {
            get { return !this.ReadOnlyALL; }
        }

        public string CIF
        {
            get { return this.GetValueFromTable<string>("CIF"); }
            set
            {
                if (value != this.GetValueFromTable<string>("CIF"))
                {
                    this.SetValueToTable("CIF", value);
                    this.NotifyPropChanged("CIF");
                }
            }
        }
        public string Nombre
        {
            get { return this.GetValueFromTable<string>("Nombre"); }
            set
            {
                if (value != this.GetValueFromTable<string>("Nombre"))
                {
                    this.SetValueToTable("Nombre", value);
                    this.NotifyPropChanged("Nombre");
                }
            }
        }
        public string TipoCalle
        {
            get { return this.GetValueFromTable<string>("TipoCalle"); }
            set
            {
                if (value != this.GetValueFromTable<string>("TipoCalle"))
                {
                    this.SetValueToTable("TipoCalle", value);
                    this.NotifyPropChanged("TipoCalle");
                }
            }
        }
        public string Direccion
        {
            get { return this.GetValueFromTable<string>("Direccion"); }
            set
            {
                if (value != this.GetValueFromTable<string>("Direccion"))
                {
                    this.SetValueToTable("Direccion", value);
                    this.NotifyPropChanged("Direccion");
                }
            }
        }
        public int CP
        {
            get { return this.GetValueFromTable<int>("CP"); }
            set
            {
                if (value != this.GetValueFromTable<int>("CP"))
                {
                    this.SetValueToTable("CP", value);
                    this.NotifyPropChanged("CP");
                }
            }
        }
        public string Localidad
        {
            get { return this.GetValueFromTable<string>("Localidad"); }
            set
            {
                if (value != this.GetValueFromTable<string>("Localidad"))
                {
                    this.SetValueToTable("Localidad", value);
                    this.NotifyPropChanged("Localidad");
                }
            }
        }
        public string Poblacion
        {
            get { return this.GetValueFromTable<string>("Poblacion"); }
            set
            {
                if (value != this.GetValueFromTable<string>("Poblacion"))
                {
                    this.SetValueToTable("Poblacion", value);
                    this.NotifyPropChanged("Poblacion");
                }
            }
        }

        #region bank account
        public string Cuenta
        {
            get { return this._model.CuentaBancaria.AccountNumber; }
            set
            {
                if (this._model.CuentaBancaria.AccountNumber == value)
                    return;

                try
                {
                    this._model.CuentaBancaria.AccountNumber = value;
                    this.NotifyBankAccountChanged();
                }
                catch (Exception err)
                {
                    MessageBox.Show(err.ToString());
                }
            }
            /*get { return this.GetValueFromTable<string>("Cuenta"); }
            set
            {
                if (value != this.GetValueFromTable<string>("Cuenta"))
                {
                    this.SetValueToTable("Cuenta", value);                    
                    this.NotifyPropChanged("Cuenta");
                    this.DivideAccountNumber();
                    //Raise event CanExecuteChanged for CopyAccount command
                    CommandManager.InvalidateRequerySuggested();
                }
            }*/
        }
        public string Cuenta_IBAN
        {
            get { return this._model.CuentaBancaria.GetIBAN(); }
            set
            {
                if(this._model.CuentaBancaria.GetIBAN() != value)
                {
                    if (this._model.CuentaBancaria.SetNewAccountIBAN(value))
                        this.NotifyBankAccountChanged();
                    else
                        throw new Exception("La cuenta de banco es incorrecta.");
                }
            }
            /*get { return this.GetValueFromTable<string>("Cuenta_IBAN"); }
            set
            {
                if (value != this.GetValueFromTable<string>("Cuenta_IBAN"))
                {
                    this.SetValueToTable("Cuenta_IBAN", value);
                    this.SetPartOfAccount(AccountPart.IBAN, (string)value);
                    this.NotifyPropChanged("Cuenta_IBAN");
                }
            }*/
        }
        public int Cuenta_Banco
        {
            get { return this._model.CuentaBancaria.GetAccountPart(AccountPart.Bank); }
            set
            {
                if (this._model.CuentaBancaria.GetAccountPart(AccountPart.Bank) != value)
                {
                    if (this._model.CuentaBancaria.SetNewAccountByPart(AccountPart.Bank, value))
                        this.NotifyBankAccountChanged();
                    else
                        throw new Exception("La cuenta de banco es incorrecta.");
                }
            }
            /*get { return this.GetValueFromTable<int>("Cuenta_Banco"); }
            set
            {
                if (value != this.GetValueFromTable<int>("Cuenta_Banco"))
                {
                    this.SetValueToTable("Cuenta_Banco", value);
                    this.SetPartOfAccount(AccountPart.Bank, (int)value);
                    this.NotifyPropChanged("Cuenta_Banco");
                }
            }*/
        }
        public int Cuenta_Ofic
        {
            get { return this._model.CuentaBancaria.GetAccountPart(AccountPart.Office); }
            set
            {
                if (this._model.CuentaBancaria.GetAccountPart(AccountPart.Office) != value)
                {
                    if (this._model.CuentaBancaria.SetNewAccountByPart(AccountPart.Office, value))
                        this.NotifyBankAccountChanged();
                    else
                        throw new Exception("La cuenta de banco es incorrecta.");
                }
            }
        }
        public int Cuenta_DC
        {
            get { return this._model.CuentaBancaria.GetAccountPart(AccountPart.DC); }
            set
            {
                if (this._model.CuentaBancaria.GetAccountPart(AccountPart.DC) != value)
                {
                    if (this._model.CuentaBancaria.SetNewAccountByPart(AccountPart.DC, value))
                        this.NotifyBankAccountChanged();
                    else
                        throw new Exception("La cuenta de banco es incorrecta.");
                }
            }
        }
        public int Cuenta_Cuenta
        {
            get { return this._model.CuentaBancaria.GetAccountPart(AccountPart.Account); }
            set
            {
                if (this._model.CuentaBancaria.GetAccountPart(AccountPart.Account) != value)
                {
                    if (this._model.CuentaBancaria.SetNewAccountByPart(AccountPart.Account, value))
                        this.NotifyBankAccountChanged();
                    else
                        throw new Exception("La cuenta de banco es incorrecta.");
                }
            }
        }
        #endregion

        public DateTime FechaPunteo
        {
            get { return this.GetValueFromTable<DateTime>("FechaPunteo"); }
            set
            {
                if (value != this.GetValueFromTable<DateTime>("FechaPunteo"))
                {
                    this.SetValueToTable("FechaPunteo", value);
                    this.NotifyPropChanged("FechaPunteo");
                }
            }
        }
        public string Presidente
        {
            get { return this.GetValueFromTable<string>("Presidente"); }
            set
            {
                if (value != this.GetValueFromTable<string>("Presidente"))
                {
                    this.SetValueToTable("Presidente", value);
                    this.NotifyPropChanged("Presidente");
                }
            }
        }
        public string Secretario
        {
            get { return this.GetValueFromTable<string>("Secretario"); }
            set
            {
                if (value != this.GetValueFromTable<string>("Secretario"))
                {
                    this.SetValueToTable("Secretario", value);
                    this.NotifyPropChanged("Secretario");
                }
            }
        }
        public string Tesorero
        {
            get { return this.GetValueFromTable<string>("Tesorero"); }
            set
            {
                if (value != this.GetValueFromTable<string>("Tesorero"))
                {
                    this.SetValueToTable("Tesorero", value);
                    this.NotifyPropChanged("Tesorero");
                }
            }
        }
        public string Vocales
        {
            get { return this.GetValueFromTable<string>("Vocales"); }
            set
            {
                if (value != this.GetValueFromTable<string>("Vocales"))
                {
                    this.SetValueToTable("Vocales", value);
                    this.NotifyPropChanged("Vocales");
                }
            }
        }
        public string Notas
        {
            get { return this.GetValueFromTable<string>("Notas"); }
            set
            {
                if (value != this.GetValueFromTable<string>("Notas"))
                {
                    this.SetValueToTable("Notas", value);
                    this.NotifyPropChanged("Notas");
                }
            }
        }

        #region model properties
        public override int ComMaxCod
        {
            get { return this._model.MaxCod; }
        }
        public override int ComMinCod
        {
            get { return this._model.MinCod; }
        }
        #endregion

        public double TabPanelWidth
        {
            get { return this._TabPanelWidth; }
            set
            {
                if(this._TabPanelWidth != value)
                {
                    this._TabPanelWidth = value;
                    this.NotifyPropChanged("TabPanelWidth");
                }
            }
        }

        #region tabbed expander
        public override ObservableCollection<TabExpTabItemBaseVM> TopTabbedExpanderItemsSource { get; set; }
        public override ObservableCollection<TabExpTabItemBaseVM> BottomTabbedExpanderItemsSource { get; set; }
        public override int TopTabbedExpanderSelectedIndex
        {
            get { return this._TopTabbedExpanderSelectedIndex; }
            set
            {
                if (this._TopTabbedExpanderSelectedIndex != value)
                {
                    this._TopTabbedExpanderSelectedIndex = value;
                    this.NotifyPropChanged("TopTabbedExpanderSelectedIndex");
                }
            }
        }
        public override int BottomTabbedExpanderSelectedIndex
        {
            get { return this._BottomTabbedExpanderSelectedIndex; }
            set
            {
                if (this._BottomTabbedExpanderSelectedIndex != value)
                {
                    this._BottomTabbedExpanderSelectedIndex = value;
                    this.NotifyPropChanged("BottomTabbedExpanderSelectedIndex");
                }
            }
        }
        #endregion
        #endregion

        #region commands
        private Command_ModifyButtonClick _ModifyCommand;
        private Command_SaveButtonClick _SaveCommand;
        private Command_CopyAccount _CopyAccount;
        private Command_PasteAccount _PasteAccount;
        private Command_NextRecord_Cdad _NextRecord;
        private Command_PrevRecord_Cdad _PrevRecord;
        #endregion

        #region commands props
        public ICommand ModifyCommand { get { return this._ModifyCommand; } }
        public ICommand SaveCommand { get { return this._SaveCommand; } }
        public ICommand CopyAccount { get { return this._CopyAccount; } }
        public ICommand PasteAccount { get { return this._PasteAccount; } }
        public ICommand NextRecord { get { return this._NextRecord; } }
        public ICommand PrevRecord { get { return this._PrevRecord; } }
        #endregion

        #region validate TODO
        /*public string Error { get { return null; } }
        public string this[string columnName]
        {
            get
            {
                /*switch (columnName)
                {
                    case "CP":
                        if (this.Age < 10 || this.Age > 100)
                            return "The age must be between 10 and 100";
                        break;
                }

                return string.Empty;
            }
        }*/
        #endregion

        #region helpers
        /// <summary>
        /// Save all changes to database and set all to read only.
        /// </summary>
        public void SaveChanges()
        {
            this._model.SaveChanges();
            this.ReadOnlyALL = true;
        }
        /// <summary>
        /// Launch NotifyPropChanged for all properties of this tab.
        /// Launch CommandManager.InvalidateRequerySuggested.
        /// </summary>
        private void NotifyAllChanges()
        {
            /*PropertyInfo[] properties = typeof(VMTabCdad).GetProperties();
            foreach(PropertyInfo pInfo in properties)
            {
                if (pInfo.Name != "ReadOnlyALL" &&
                    pInfo.Name != "ModifyDataAllowed")
                    NotifyPropChanged(pInfo.Name);
            }*/
            this.NotifyPropChanged("");
            CommandManager.InvalidateRequerySuggested();
        }
        /// <summary>
        /// Launch NotifyPropChanged for all bank account properties of this tab, and stores "Cuenta" property in database.
        /// Launch CommandManager.InvalidateRequerySuggested.
        /// </summary>
        private void NotifyBankAccountChanged()
        {
            this.NotifyPropChanged("Cuenta");
            this.NotifyPropChanged("Cuenta_IBAN");
            this.NotifyPropChanged("Cuenta_Banco");
            this.NotifyPropChanged("Cuenta_Ofic");
            this.NotifyPropChanged("Cuenta_DC");
            this.NotifyPropChanged("Cuenta_Cuenta");

            this.SetValueToTable("Cuenta", this.Cuenta);
            //Fuerza CanExecute de los comandos
            CommandManager.InvalidateRequerySuggested();
        }
        /// <summary>
        /// Called when a new Cdad record is added.
        /// </summary>
        public override void UpdateMinMaxCods()
        {
            this._model.UpdateMinMaxCods();
        }
        /// <summary>
        /// Order model to get data of new Cod(new row in database).
        /// </summary>
        /// <param name="newCod"></param>
        public override void OnChangedCod(int newCod)
        {
            if (!this.ReadOnlyALL)
                return;

            this._model.ChangeCod(newCod);
            this.TabComCod = newCod;
            NotifyAllChanges();
        }
        #endregion
        /*#region account number Helpers
        /// <summary>
        /// Given a complete account number in string format, set all bank account parts in int format.
        /// </summary>
        /// <param name="account"></param>
        private void DivideAccountNumber()
        {
            this.Cuenta_IBAN = this._model.Account.GetIBAN();
            this.Cuenta_Banco = this._model.Account.GetAccountPart(AccountPart.Bank);
            this.Cuenta_Ofic = this._model.Account.GetAccountPart(AccountPart.Office);
            this.Cuenta_DC = this._model.Account.GetAccountPart(AccountPart.DC);
            this.Cuenta_Cuenta = this._model.Account.GetAccountPart(AccountPart.Account);
            /*string account = this.Cuenta;

            this.Cuenta_IBAN = account.Substring(0, 4);

            string banco = account.Substring(4, 4);
            string ofic = account.Substring(8, 4);
            string dc = account.Substring(12, 2);
            string cuenta = account.Substring(14, 10);

            this.Cuenta_Banco = int.Parse(banco);
            this.Cuenta_Ofic = int.Parse(ofic);
            this.Cuenta_DC = int.Parse(dc);
            this.Cuenta_Cuenta = int.Parse(cuenta);
        }
        /*
        /// <summary>
        /// Set a bank account part by enum.
        /// </summary>
        /// <param name="part"></param>
        /// <param name="value">Int => NOT for IBAN</param>
        private void SetPartOfAccount(AccountPart part, int value)
        {
            if (part == AccountPart.IBAN) throw new Exception("Can not set IBAN as an integer value. Wrong value type or account part.");

            int i = 0;
            int n = 0;



            string newStr = this.Cuenta;
            newStr.Remove(i, n);
            newStr.Insert(i, value.ToString());
            this.Cuenta = newStr;
        }
        /// <summary>
        /// Set a bank account part by enum.
        /// </summary>
        /// <param name="part"></param>
        /// <param name="value">String => ONLY for IBAN</param>
        private void SetPartOfAccount(AccountPart part, string value)
        {
            if (part != AccountPart.IBAN) throw new Exception("IBAN can only be setted as a string value. Wrong value type or account part.");

            string newStr = this.Cuenta;
            newStr.Remove(0, 4);
            newStr.Insert(0, value);
            this.Cuenta = newStr;
        }
        /// <summary>
        /// Get if string bankAccount is a correct IBAN bank account, and set cleanAccount to a trimmed(and clean of typical separators
        /// '/' and '-') version of bankAccount.
        /// </summary>
        /// <param name="bankAccount"></param>
        /// <param name="cleanAccount"></param>
        /// <returns></returns>
        private bool IsAnAccount(string bankAccount, out string cleanAccount) //http://www.codeproject.com/Tips/775696/IBAN-Validator modified
        {
            bankAccount = bankAccount.ToUpper(); //IN ORDER TO COPE WITH THE REGEX BELOW
            if (String.IsNullOrEmpty(bankAccount))
            {
                cleanAccount = string.Empty;
                return false;
            }
            else if (System.Text.RegularExpressions.Regex.IsMatch(bankAccount, "^[A-Z0-9]"))
            {
                bankAccount = bankAccount.Replace(" ", String.Empty).Replace("-", String.Empty).Replace("/", String.Empty);
                cleanAccount = bankAccount;

                string bank = bankAccount.Substring(4, bankAccount.Length - 4) + bankAccount.Substring(0, 4);
                int asciiShift = 55;
                StringBuilder sb = new StringBuilder();
                foreach (char c in bank)
                {
                    int v;
                    if (Char.IsLetter(c)) v = c - asciiShift;
                    else v = int.Parse(c.ToString()); //tryparse
                    sb.Append(v);
                }
                string checkSumString = sb.ToString();
                int checksum = int.Parse(checkSumString.Substring(0, 1));
                for (int i = 1; i < checkSumString.Length; i++)
                {
                    int v = int.Parse(checkSumString.Substring(i, 1));
                    checksum *= 10;
                    checksum += v;
                    checksum %= 97;
                }
                return checksum == 1;
            }
            else
            {
                cleanAccount = string.Empty;
                return false;
            }
        }
        #endregion*/
        
        #region datatablehelpers overridden methods
        /// <summary>
        /// Gets value of type T from datatable column using ConvertFromDBVal. Doesn't check if value is of type T. Only one row supposed(f.i. tab cdades).
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="column"></param>
        /// <returns></returns>
        public override T GetValueFromTable<T>(string column)
        {
            return ConvertFromDBVal<T>(this._model.DTable.Rows[0][column]);
        }
        /// <summary>
        /// Set value of datatable column. Only one row supposed(f.i. tab cdades).
        /// </summary>
        /// <param name="column"></param>
        /// <param name="value"></param>
        public override void SetValueToTable(string column, object value)
        {
            this._model.DTable.Rows[0][column] = value;
        }
        #endregion

        #region common commands overridden methods
        /// <summary>
        /// Method called by modify and save commands.
        /// Modify command: Switch the readonly property of all textboxes(except TabTBCod), and controls that accepts editing, in the caller tab, becoming all editable.        
        /// Save Command(ONLY IF saveChanges == true): Save changes made to controls after activating modify command.
        /// </summary>
        /// <param name="saveChanges">HAVE to be true IF called by SaveCommand, so the method save changes too</param>
        public override void ModifyRecord(bool saveChanges)
        {
            if (saveChanges)
            {
                this.SaveChanges();
                CommandManager.InvalidateRequerySuggested();
                return;
            }

            if (this.ModifyDataAllowed == true)
            {
                this._model.RevertChanges();
                //TODO
                //¿REALMENTE ES NECESARIO ESTO?:
                this.NotifyAllChanges();
                // => COMPROBAR
            }

            this.ReadOnlyALL = !this.ReadOnlyALL;
            CommandManager.InvalidateRequerySuggested();
        }
        public override bool CanModifyRecord()
        {
            return this.ModifyDataAllowed;
        }
        public override bool CanCopyAccount()
        {
            return !string.IsNullOrEmpty(this.Cuenta);
        }
        public override void CopyAccountToClipboard()
        {
            Clipboard.SetText(this.Cuenta);
        }
        public override void PasteAccountFromClipboard()
        {
            if (!this.ModifyDataAllowed) return;

            this.Cuenta = Clipboard.GetText();
            /*string cleanAcc = "";
            if (this._model.Account.IsAnAccount(Clipboard.GetText(), out cleanAcc))
                this.Cuenta = cleanAcc;
            else
                MessageBox.Show("El texto que intenta pegar no es una cuenta bancaria:" + "\n\r" + Clipboard.GetText());*/
        }
        #endregion

        #region UoW
        /// <summary>
        /// Llamado por AbleTabControl cuando se cierra la pestaña
        /// </summary>
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public override async Task CleanUnitOfWork()
        {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            this.UOW.RemoveVMTabReferencesFromRepos().Forget().ConfigureAwait(false);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        }

        public override async Task InitUoWAsync()
        {
            iAppRepositories appRepos = (iAppRepositories)Application.Current;
            HashSet<IRepository> repos = new HashSet<IRepository>();

            repos.Add(appRepos.ComunidadRepo);
            this.UOW = new UnitOfWork(repos, this);

            this.ComunidadRepo = appRepos.ComunidadRepo;
        }
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        #endregion
    }
}
