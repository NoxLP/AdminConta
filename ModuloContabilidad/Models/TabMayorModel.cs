using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using AdConta.Models;
using ModuloContabilidad.ObjModels;

namespace ModuloContabilidad.Models
{
    public class TabMayorModel : TabMayorDBConnection
    {
        public TabMayorModel(int cod)
        {
            this._DTable = new DataTable();
            string tableName = string.Format("C{0}Cuen{1}",
                cod.ToString(),
                GlobalSettings.Properties.Settings.Default.CUENTADEFAULT);
            this._CurrentAccount = CuentaMayor.GetCuentaDefault();

            if (!base.ExistsTableInDB(tableName))
                base.CreateTable(tableName, cod.ToString(), this.CurrentAccount);

            string SQLcmd = string.Format("{0} {1} {2}",
                GlobalSettings.Properties.Settings.Default.SELECTMAYOR,
                tableName,
                GlobalSettings.Properties.Settings.Default.ORDERMAYOR);
            base.SetDataTableByCommand(SQLcmd, ref this._DTable);
            this.UpdateMinMaxAccs(cod);
            this.DView = this.DTable.DefaultView;
        }

        #region fields
        private DataTable _DTable;
        private DataView _DView;
        private CuentaMayor _CurrentAccount;
        private int _MaxAcc;
        private int _MinAcc;
        #endregion

        #region properties
        public DataTable DTable
        {
            get { return this._DTable; }
            set { this._DTable = value; }
        }
        public DataView DView
        {
            get { return this._DView; }
            set { this._DView = value; }
        }
        public CuentaMayor CurrentAccount
        {
            get { return this._CurrentAccount; }
            protected set
            {
                if (this._CurrentAccount != value)
                    this._CurrentAccount = value;
            }
        }
        public int MaxAcc
        {
            get { return this._MaxAcc; }
            protected set { this._MaxAcc = value; }
        }
        public int MinAcc
        {
            get { return this._MinAcc; }
            protected set { this._MinAcc = value; }
        }
        #endregion

        #region database methods
        /// <summary>
        /// Change current account. Return false if the new account don't exist, so VM will make a "fake account".
        /// ComCod HAVE to be the same as the previous one.
        /// </summary>
        /// <param name="AccCod"></param>
        /// <param name="ComCod"></param>
        public bool ChangeAcc(int AccCod, int ComCod)
        {
            this._DTable.Clear();
            string tableName = string.Format("C{0}Cuen{1}", ComCod, AccCod);
            this.CurrentAccount.Codigo = tableName;

            if (!base.ExistsTableInDB(tableName))
            {
                this.CurrentAccount.CuentaFalsa = true;
                return false;
            }
            else
            {
                string SQLcmd = string.Format("{0} {1} {2}",
                GlobalSettings.Properties.Settings.Default.SELECTMAYOR,
                tableName,
                GlobalSettings.Properties.Settings.Default.ORDERMAYOR);
                base.SetDataTableByCommand(SQLcmd, ref this._DTable);
                this.DView = this.DTable.DefaultView;
                return true;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="cod"></param>
        public void UpdateMinMaxAccs(int cod)
        {
            this.MaxAcc = base.ExecuteScalar<int>(string.Format("SELECT MAX(CodigoCuenta) FROM C{0}CuentasContables", cod.ToString()));
            this.MinAcc = base.ExecuteScalar<int>(string.Format("SELECT MIN(CodigoCuenta) FROM C{0}CuentasContables", cod.ToString()));
        }
        #endregion
    }
}