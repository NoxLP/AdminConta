using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using AdConta.Models;
using ModuloContabilidad.ObjModels;

namespace ModuloContabilidad.Models
{
    public class TabMayorDBConnection : aMayor_DBConnectionBase
    {
        public TabMayorDBConnection()
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
        #endregion

        #region helpers
        //TODO cuando se crea un record en la tabla de cuentas contables hay que terminar de rellenar datos (importante: nombres de las cuentas)
        public override void CreateTable(string tableName, string cod, CuentaMayor acc)
        {
            //Create account table
            string command = this.CreateCmd;
            command = command.Remove(this._TypeNameIndex, this._TypeNameLength);
            command = command.Insert(this._TypeNameIndex, tableName);

            this.ExecuteNonQuery(command);

            //Update cdad's list(table) of accounts with the new table, create it if no exist
            TableCreateCmds Tcc = new TableCreateCmds();
            string accountsTable = "C" + cod + Tcc.TableTypeNames[(int)TableType.CuentasC];

            if (!base.ExistsTableInDB(accountsTable))
            {
                command = Tcc[TableType.CuentasC];
                command = command.Remove(this._TypeNameIndex, this._TypeNameLength);
                command = command.Insert(this._TypeNameIndex, accountsTable);

                this.ExecuteNonQuery(command);
            }

            //TODO faltan nombres de las cuentas
            this.ExecuteNonQuery(string.Format("INSERT INTO {0} (CodigoCuenta, Grupo, Subgrupo, Sufijo, Nombre) VALUES ({1},{2},{3},{4},'{5}')",
                accountsTable,
                acc.Id,
                acc.Grupo,
                acc.Subgrupo,
                acc.Sufijo,
                acc.Nombre));
        }
        #endregion
    }
}
