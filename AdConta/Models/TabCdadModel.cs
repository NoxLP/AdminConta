using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;

namespace AdConta.Models
{
    public class TabCdadModel : TabCdadDBConnection
    {
        public TabCdadModel(int cod)
        {
            this._OrigTable = new DataTable();
            base.SetDataTableByCommand(string.Format("{0} {1} AND Codigo={2} {3}",
                GlobalSettings.Properties.Settings.Default.SELECTCDADES,
                GlobalSettings.Properties.Settings.Default.WHERECDADES_NOBAJA,
                cod.ToString(),
                GlobalSettings.Properties.Settings.Default.ORDERCDADES),
                ref this._OrigTable);
            this.UpdateMinMaxCods();
            this._NewTable = this._OrigTable.Copy();
        }

        #region fields
        private DataTable _OrigTable;
        private DataTable _NewTable;
        private int _MaxCod;
        private int _MinCod;
        private CuentaBancaria _CuentaBancaria;
        #endregion

        #region properties
        public DataTable DTable
        {
            get { return this._NewTable; }
            set { this._NewTable = value; }
        }
        public int MaxCod
        {
            get { return this._MaxCod; }
            protected set { this._MaxCod = value; }
        }
        public int MinCod
        {
            get { return this._MinCod; }
            protected set { this._MinCod = value; }
        }
        public CuentaBancaria CuentaBancaria
        {
            get { return this._CuentaBancaria; }
            set { this._CuentaBancaria = value; }
        }
        #endregion

        #region database methods
        /// <summary>
        /// Copy _NewTable to _OrigTable and update database with _OrigTable
        /// </summary>
        public void SaveChanges()
        {
            /*DBTest test = new DBTest(_NewTable);
            test.Title = "new table";
            test.Show();*/

            _OrigTable = _NewTable.Copy();
            base.UpdateDatabaseTableFromDataTable(this._OrigTable);
        }
        /// <summary>
        /// Copy _OrigTable to _NewTable so revert all changes.
        /// </summary>
        public void RevertChanges()
        {
            _NewTable = _OrigTable.Copy();
        }
        /// <summary>
        /// Used when Cod(Id) is changed by the user to fill the datatables with the new data
        /// </summary>
        /// <param name="newCod"></param>
        public void ChangeCod(int newCod)
        {
            this._OrigTable.Clear();// = new DataTable();
            base.SetDataTableByCommand(string.Format("SELECT * FROM Cdades {0} AND Codigo={1} {2}",
                GlobalSettings.Properties.Settings.Default.WHERECDADES_NOBAJA,
                newCod.ToString(),
                GlobalSettings.Properties.Settings.Default.ORDERCDADES),
                ref this._OrigTable);

            this._NewTable.Clear();
            this._NewTable = _OrigTable.Copy();
        }
        /// <summary>
        /// Called when a new Cdad record is added.
        /// </summary>
        public void UpdateMinMaxCods()
        {
            this.MaxCod = base.ExecuteScalar<int>("SELECT MAX(Codigo) FROM Cdades");
            this.MinCod = base.ExecuteScalar<int>("SELECT MIN(Codigo) FROM Cdades");
        }
        #endregion
    }
}
