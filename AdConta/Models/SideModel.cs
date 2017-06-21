using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;

namespace AdConta.Models
{
    /// <summary>
    /// Model class for sidetool: get all data in table "Cdades"
    /// </summary>
    public class SideModel : DatabaseConnection
    {
        public SideModel()
        {
            this._Table = new DataTable();
            base.SetDataTableByCommand(GlobalSettings.Properties.Settings.Default.SELECTCDADES + " " +
                GlobalSettings.Properties.Settings.Default.WHERECDADES_NOBAJA, ref this._Table, TableType.Cdades);
        }
        //DatabaseConnection dbCon = new DatabaseConnection();
        private DataTable _Table;

        public DataTable Table
        {
            get { return this._Table; }
            set { this._Table = value; }
        }
    }
}
