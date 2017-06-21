using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using AdConta.Models;

namespace ModuloContabilidad.Models
{
    public class AsientosDBConnection : aDBConnectionBase
    {
        public AsientosDBConnection()
        {
            TableCreateCmds commands = new TableCreateCmds();
            this._CreateCmd = commands[TableType.Mayor];
            this._TypeNameIndex = commands.TypeNameIndex;
            this._TypeNameLength = commands.TypeNameLength;
        }

        #region fields
        private readonly string _CreateCmd;
        private readonly int _TypeNameIndex;
        private readonly int _TypeNameLength;
        #endregion

        #region properties
        public override string CreateCmd
        {
            get { return this._CreateCmd; }
        }
        #endregion

        #region database connection methods
        /// <summary>
        /// Using settings's connection string, set DataTable by provided SQL command and re-initialize class, 
        /// setting data adapter and command builder.
        /// </summary>
        /// <param name="SQLCommand"></param>
        /// <returns></returns>
        public override void SetDataTableByCommand(string SQLCommand, ref DataTable dTable)
        {
            using (SqlConnection con = new SqlConnection(base._strCon))
            {
                con.Open();
                base.cmdBuilder = new SqlCommandBuilder(base.da);
                base.da = new SqlDataAdapter(SQLCommand, con);
                base.da.Fill(dTable);
                con.Close();
            }
        }

        public override void UpdateDatabaseTableFromDataTable(DataTable dTable)
        {
            using (SqlConnection con = new SqlConnection(this._strCon))
            {
                //Update journal
                DataSet dSet = new DataSet();
                dSet.Tables.Add(dTable);

                //this.con = new SqlConnection(this.ConnectionString);
                con.Open();
                base.cmdBuilder = new SqlCommandBuilder(this.da);
                base.da.Update(dTable);

                //Update ledge accounts
                DataTable Accounts = new DataTable();
                //SqlCommand ledgeCmd;
                //SqlDataAdapter ledgeDa;



                con.Close();
            }
        }
        #endregion
    }
}
