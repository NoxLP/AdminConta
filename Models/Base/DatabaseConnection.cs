using System;
using System.Windows;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;

namespace AdConta.Models
{
    /// <summary>
    /// Enum for tables.
    /// </summary>
    public enum TableType : int { Cdades, Props, Mayor, Diario, CuentasC }

    //TODO Add year to tables names????
    public class TableCreateCmds
    {
        public int TypeNameLength = 4;
        public int TypeNameIndex = 20;

        private readonly string[] _TableTypeNames = new string[]
        {
            #region Tables names per table type
		    "Cdad",
            "Prop",
            "Cuen",
            "Diar",
            "CuentasContables"
	        #endregion
        };

        private readonly string[] CreateCmds = new string[]
        {
            #region CREATE TABLE commands strings
            @"CREATE TABLE [dbo].[Cdad] (
    [Id]            INT            IDENTITY (1, 1) NOT NULL,
    [Baja]          BIT            DEFAULT ((0)) NOT NULL,
    [Codigo]        INT            NULL,
    [CIF]           NVARCHAR (9)   NULL,
    [Nombre]        NVARCHAR (50)  NULL,
    [Cuenta_IBAN]   NVARCHAR (4)   NULL,
    [Cuenta_Banco]  INT            NULL,
    [Cuenta_Ofic]   INT            NULL,
    [Cuenta_DC]     INT            NULL,
    [Cuenta_Cuenta] INT            NULL,
    [Cuenta]        NVARCHAR (24)  NOT NULL,
    [FechaPunteo]   DATETIME       NOT NULL,
    [TipoCalle]     NVARCHAR (5)   NULL,
    [Calle]         NVARCHAR (MAX) NULL,
    [Portal]        NVARCHAR (20)  NULL,
    [Numero]        INT            NULL,
    [CP]            INT            NULL,
    [Localidad]     NVARCHAR (50)  NULL,
    [Poblacion]     NVARCHAR (50)  NULL,
    [Direccion]     NVARCHAR (MAX) NULL,
    [Notas]         TEXT           NULL,
    [Presidente]    NVARCHAR (MAX) NULL,
    [CodPresidente] INT            NULL,
    [Secretario]    NVARCHAR (MAX) NULL,
    [CodSecretario] INT            NULL,
    [Tesorero]      NVARCHAR (MAX) NULL,
    [CodTesorero]   INT            NULL,
    [Vocales]       TEXT           NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC)
)",

            @"CREATE TABLE [dbo].[Prop] (
    [Id]          INT            IDENTITY (1, 1) NOT NULL,
    [Vvda]        NVARCHAR (50)  NULL,
    [Coeficiente] FLOAT (53)     NULL,
    [Nombre]      NVARCHAR (MAX) NULL,
    [Dni]         NVARCHAR (9)   NULL,
    [PortalG]     INT            NULL,
    [TipoCalle]   NVARCHAR (5)   NULL,
    [Calle]       NVARCHAR (MAX) NULL,
    [Portal]      NVARCHAR (20)  NULL,
    [Puerta]      NVARCHAR (20)  NULL,
    [Numero]      INT            NULL,
    [CP]          INT            NULL,
    [Localidad]   NVARCHAR (50)  NULL,
    [Poblacion]   NVARCHAR (50)  NULL,
    [Direccion]   NVARCHAR (MAX) NULL,
    [Telefono1]   NVARCHAR (20)  NULL,
    [Telefono2]   NVARCHAR (20)  NULL,
    [Telefono3]   NVARCHAR (20)  NULL,
    [Fax]         NVARCHAR (20)  NULL,
    [email]       NVARCHAR (MAX) NULL,
    [Cuenta]      INT            DEFAULT ((430)) NOT NULL,
    [Remesa]      BIT            DEFAULT ((0)) NOT NULL,
    [Asociada]    INT            NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC)
)",

            @"CREATE TABLE [dbo].[Cuen] (
    [ID]       INT            IDENTITY (1, 1) NOT NULL,
    [NAsiento] INT            DEFAULT ((0)) NOT NULL,
    [Fecha]	   DATE			  NOT NULL,
	[FechaValor]   DATETIME   NOT NULL,
    [Concepto] NVARCHAR (MAX) NOT NULL,
    [Debe]     INT            DEFAULT ((0)) NOT NULL,
    [Haber]    INT            DEFAULT ((0)) NOT NULL,
    [Saldo_1]  INT            DEFAULT ((0)) NOT NULL,
    [Saldo]    AS             (([Debe]-[Haber])+[Saldo_1]),
    [Punteo]     BIT            DEFAULT ((0)) NULL,
    [Factura]  INT            NULL,
    [Recibo]   INT            NULL,
    PRIMARY KEY CLUSTERED ([ID] ASC)
)",

            @"CREATE TABLE [dbo].[Diar] (
    [NApunte]    INT            IDENTITY (1, 1) NOT NULL,
    [NAsiento]   INT            NOT NULL,
    [Fecha]      DATE           NOT NULL,
    [Concepto]   NVARCHAR (MAX) NOT NULL,
    [Debe/Haber] BIT            DEFAULT ((0)) NOT NULL,
    [Importe]    MONEY          DEFAULT ((0)) NOT NULL,
    [Cuenta]     INT            NOT NULL,
    [Punteo]     BIT            DEFAULT ((0)) NOT NULL,
    [Factura]    NVARCHAR (50)  NULL,
    [Recibo]     NVARCHAR (50)  NULL,
    PRIMARY KEY CLUSTERED ([NApunte] ASC)
)",
            @"CREATE TABLE [dbo].[CuCo] (
    [Id]            INT            NOT NULL IDENTITY (1, 1),
    [CodigoCuenta]  INT            DEFAULT ((0)) NOT NULL,
    [Grupo]         INT            DEFAULT ((0)) NOT NULL,
    [Subgrupo]      INT            DEFAULT ((0)) NOT NULL,
    [Sufijo]        INT            DEFAULT ((0)) NOT NULL,
    [Nombre]        NVARCHAR (MAX) NULL,
    [EsProveedor]   BIT            DEFAULT ((0)) NOT NULL,
    [EsPropietario] BIT            DEFAULT ((0)) NOT NULL,
    [EsDeudora]     BIT            DEFAULT ((0)) NOT NULL,
    [EsAcreedora]   BIT            DEFAULT ((0)) NOT NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC)
)"
            #endregion
        };

        public string[] TableTypeNames
        {
            get { return this._TableTypeNames; }
        }

        public string this[TableType index]
        {
            get { return this.CreateCmds[(int)index]; }
        }
    }

    //http://stackoverflow.com/questions/5495416/using-sqlcommand-how-to-add-multiple-parameters-to-its-object-insertion-via
    public static class DbCommandExtensions
    {
        public static void AddInputParameters<T>(this IDbCommand cmd,
            T parameters) where T : class
        {
            foreach (var prop in parameters.GetType().GetProperties())
            {
                object val = prop.GetValue(parameters, null);
                var p = cmd.CreateParameter();
                p.ParameterName = prop.Name;
                p.Value = val ?? DBNull.Value;
                cmd.Parameters.Add(p);
            }
        }
    }

    /// <summary>
    /// DEPRECATED. OLD Class for database operations. OLD Base class for models.
    /// </summary>
    public class DatabaseConnection
    {
        public DatabaseConnection()
        {
            //this.con = new SqlConnection(this.ConnectionString);
        }

        #region fields
        private string _strCon = GlobalSettings.Properties.Settings.Default.conta1ConnectionString;
        private SqlDataAdapter da;
        //private SqlConnection con;
        private SqlCommandBuilder cmdBuilder;
        private TableCreateCmds CreateCmd = new TableCreateCmds();
        #endregion

        #region properties
        public string ConnectionString
        {
            get { return this._strCon; }
            set { this._strCon = value; }
        }
        #endregion

        #region database connection methods
        /// <summary>
        /// Using settings's connection string, set DataTable by provided command and re-initialize class, setting data adapter and command builder.
        /// </summary>
        /// <param name="SQLCommand"></param>
        /// <returns></returns>
        public void SetDataTableByCommand(string SQLCommand, ref DataTable dTable, TableType type)
        {
            using (SqlConnection con = new SqlConnection(this._strCon))
            {
                con.Open();
                this.cmdBuilder = new SqlCommandBuilder(this.da);
                this.da = new SqlDataAdapter(SQLCommand, con);
                this.da.Fill(dTable);
                con.Close();
            }
        }

        /*public DataSet GetTableDataSet(Table table)
        {
            string SQL = "";

            switch (table)
            {
                case Table.Cdades:
                    SQL = AdConta.GlobalSettings.Properties.Settings.Default.SELECTCDADES + AdConta.GlobalSettings.Properties.Settings.Default.ORDERCDADES;
                    break;
                case Table.Props:
                    SQL = AdConta.GlobalSettings.Properties.Settings.Default.SELECTPROPS + AdConta.GlobalSettings.Properties.Settings.Default.ORDERPROPS;
                    break;
            }

            return GetDataSetByCommand(SQL);
        }*/

        public void UpdateDatabaseTableFromDataTable(DataTable dTable)
        {
            using (SqlConnection con = new SqlConnection(this._strCon))
            {
                DataSet dSet = new DataSet();
                dSet.Tables.Add(dTable);

                //this.con = new SqlConnection(this.ConnectionString);
                con.Open();
                this.cmdBuilder = new SqlCommandBuilder(this.da);
                this.da.Update(dTable);
                con.Close();
            }
        }

        public void ExecuteQuery(string SQLCommand)
        {
            using (SqlConnection con = new SqlConnection(this._strCon))
            {
                SqlCommand cmd = new SqlCommand(SQLCommand, con);
                con.Open();
                cmd.ExecuteNonQuery();
                con.Close();
            }
        }

        public void ExecuteQuery(SqlCommand SQLCommand)
        {
            using (SqlConnection con = new SqlConnection(this._strCon))
            {
                con.Open();
                SQLCommand.ExecuteNonQuery();
                con.Close();
            }
        }

        public T ExecuteScalar<T>(string SQLCommand)
        {
            using (SqlConnection con = new SqlConnection(this._strCon))
            {
                SqlCommand cmd = new SqlCommand(SQLCommand, con);
                T ret;
                con.Open();
                ret = (T)cmd.ExecuteScalar();
                con.Close();
                return ret;
            }
        }

        private void CreateTable(TableType table, string cod, string secCod = "")
        {
            //Make CREATE TABLE command
            string tableTypeName = this.CreateCmd.TableTypeNames[(int)table];
            string tableName = string.Format("{0}{1}{2}", cod, tableTypeName, secCod);
            string sCmd = this.CreateCmd[table];
            sCmd.Remove(this.CreateCmd.TypeNameIndex, this.CreateCmd.TypeNameLength);
            sCmd.Insert(this.CreateCmd.TypeNameIndex, tableName);
            //---

            this.ExecuteQuery(sCmd);
        }
        #endregion

        #region helpers
        #endregion
    }

    //---------------------------------------------
    //NEW DATABASECONNECTION CLASSES
    //---------------------------------------------

    public abstract class aDBConnectionBase
    {
        #region fields
        protected readonly string _strCon = GlobalSettings.Properties.Settings.Default.conta1ConnectionString;
        protected SqlDataAdapter da;
        protected SqlCommandBuilder cmdBuilder;
        #endregion

        #region properties
        public abstract string CreateCmd { get; }
        #endregion
        
        #region database connection methods
        /// <summary>
        /// Using settings's connection string, set DataTable by provided SQL command and re-initialize class, 
        /// setting data adapter and command builder.
        /// </summary>
        /// <param name="SQLCommand"></param>
        /// <returns></returns>
        public abstract void SetDataTableByCommand(string SQLCommand, ref DataTable dTable);
        public virtual void UpdateDatabaseTableFromDataTable(DataTable dTable)
        {
            using (SqlConnection con = new SqlConnection(this._strCon))
            {
                DataSet dSet = new DataSet();
                dSet.Tables.Add(dTable);

                //this.con = new SqlConnection(this.ConnectionString);
                con.Open();
                this.cmdBuilder = new SqlCommandBuilder(this.da);
                this.da.Update(dTable);
                con.Close();
            }
        }
        public void ExecuteNonQuery(string SQLCommand)
        {
            using (SqlConnection con = new SqlConnection(this._strCon))
            {
                SqlCommand cmd = new SqlCommand(SQLCommand, con);
                con.Open();
                try
                {
                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show(ex.ToString());
                }
                finally
                {
                    con.Close();
                }
            }
        }
        public T ExecuteScalar<T>(string SQLCommand)
        {
            using (SqlConnection con = new SqlConnection(this._strCon))
            {
                SqlCommand cmd = new SqlCommand(SQLCommand, con);
                T ret;
                con.Open();
                object o = cmd.ExecuteScalar();

                if (o == DBNull.Value) ret = default(T);
                else ret = (T)o;

                con.Close();
                return ret;
            }
        }
        #endregion

        #region helpers
        //TODO buscar forma de comprobar que existe una tabla dada preguntando directamente a la DB
        protected bool ExistsTableInDB(string tableName)
        {
            using (SqlConnection con = new SqlConnection(this._strCon))
            {
                con.Open();
                SqlCommand cmd = new SqlCommand(string.Format("SELECT * FROM {0}",
                    tableName), con);
                /*SqlCommand cmd = new SqlCommand(string.Format("SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME={0}",
                    tableName), con);*/
                bool ret = true;

                try
                {
                    cmd.ExecuteScalar();
                }
                catch (Exception)
                {
                    ret = false;
                }
                finally
                {
                    con.Close();
                }
                return ret;
            }
        }
        public virtual void CreateTable(TableType table, string cod, string secCod = "")
        {
            throw new NotImplementedException();
        }
        public virtual void CreateTable(string cod, string secCod)
        {
            throw new NotImplementedException();
        }
        /*public virtual void CreateTable(string cod)
        {
            throw new NotImplementedException();
        }*/
        public virtual void CreateTable(string tableName)
        {
            throw new NotImplementedException();
        }
        #endregion
    }

    public class TabCdadDBConnection : aDBConnectionBase
    {
        public TabCdadDBConnection()
        {
            TableCreateCmds commands = new TableCreateCmds();
            this._CreateCmd = commands[TableType.Cdades];
        }

        #region fields
        private readonly string _CreateCmd;
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
    }

    public class TabPropsDBConnection : aDBConnectionBase
    {
        public TabPropsDBConnection()
        {
            TableCreateCmds commands = new TableCreateCmds();
            this._CreateCmd = commands[TableType.Props];
        }

        #region fields
        private readonly string _CreateCmd;
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

        }
        #endregion
    }
    
    public class TabDiarioDBConnection : aDBConnectionBase
    {
        public TabDiarioDBConnection()
        {
            TableCreateCmds commands = new TableCreateCmds();
            this._CreateCmd = commands[TableType.Props];
        }

        #region fields
        private readonly string _CreateCmd;
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

        }
        #endregion
    }
}