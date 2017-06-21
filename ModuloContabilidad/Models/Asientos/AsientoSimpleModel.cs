using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using Extensions;
using AdConta;
using AdConta.Models;
using ModuloContabilidad.ObjModels;

namespace ModuloContabilidad.Models
{
//    public class AsientoSimpleModel : AsientosDBConnection
//    {
//        public AsientoSimpleModel(int cod, bool asientoIsNew)
//        {
//            this._Asiento = new ObjModels.AsientoSimple(GetNAsiento(asientoIsNew));
//        }

//        #region fields
//        private int _ComCod;
//        private aAsiento _Asiento;
//        #endregion

//        #region properties
//        public int ComCod { get { return this._ComCod; } }
//        public aAsiento Asiento { get { return this._Asiento; } }
//        #endregion

//        #region database methods
//        public void SaveNewAsientoToDB()
//        {
//            TableCreateCmds tables = new TableCreateCmds();
//            string tablename = tables.TableTypeNames[(int)TableType.Diario];
//            /*SqlCommand cmd = new SqlCommand(string.Format("{0} C{1}{2} {3}",
//                GlobalSettings.GlobalSettings.Properties.Settings.Default.SELECTDIARIO,
//                this.ComCod.ToString(),
//                tablename,
//                GlobalSettings.GlobalSettings.Properties.Settings.Default.ORDERDIARIO));*/
//            DataTable DTable = new DataTable();
//            base.SetDataTableByCommand(string.Format("{0} C{1}{2} {3}",
//                GlobalSettings.Properties.Settings.Default.SELECTDIARIO,
//                this.ComCod.ToString(),
//                tablename,
//                GlobalSettings.Properties.Settings.Default.ORDERDIARIO),
//                ref DTable);
//            List<string> LedgeList;

//            FillDataTableFromAsiento(this._Asiento, ref DTable, out LedgeList);

//            base.UpdateDatabaseTableFromDataTable(DTable);
//        }
//        #endregion

//        #region helpers
//        public void FillDataTableFromAsiento(aAsiento asiento, ref DataTable DTable)
//        {
//            int i = 0;
//            foreach (Apunte ap in asiento.Apuntes)
//            {
//                DTable.NewRow();
//                i++;
//                DTable.Rows[i].SetField<int>("Cuenta", ap.Account.Id);
//                DTable.Rows[i].SetField<decimal>("Importe", ap.Importe);
//                DTable.Rows[i].SetField<bool>("Debe/Haber", ap.DebeHaber.GetAttribute<DebitCreditAtttribute>().Description);
//                DTable.Rows[i].SetField<string>("Concepto", ap.Concepto);
//                DTable.Rows[i].SetField<bool>("Punteo", ap.Punteado);
//                DTable.Rows[i].SetField<string>("Recibo", ap.Recibo);
//                DTable.Rows[i].SetField<string>("Factura", ap.Factura);
//                DTable.Rows[i].SetField<DateTime>("Fecha", this._Asiento.Fecha);
//                /*
//                [NApunte]    INT            IDENTITY (1, 1) NOT NULL,
//[NAsiento]   INT            NOT NULL,
//[Fecha]      DATE           NOT NULL,
//[Concepto]   NVARCHAR (MAX) NOT NULL,
//[Debe/Haber] BIT            DEFAULT ((0)) NOT NULL,
//[Importe]    MONEY          DEFAULT ((0)) NOT NULL,
//[Cuenta]     INT            NOT NULL,
//[Punteo]     BIT            DEFAULT ((0)) NULL,
//[Factura]    NVARCHAR (MAX) NULL,
//[Recibo]     NVARCHAR (MAX) NULL,
//                */
//            }
//        }
//        public void FillDataTableFromAsiento(aAsiento asiento, ref DataTable DTable, out List<string> ledgeList)
//        {
//            ledgeList = new List<string>();
//            int i = 0;
//            foreach (Apunte ap in asiento.Apuntes)
//            {
//                DTable.NewRow();
//                i++;
//                DTable.Rows[i].SetField<int>("Cuenta", ap.Account.Id);
//                DTable.Rows[i].SetField<decimal>("Importe", ap.Importe);
//                DTable.Rows[i].SetField<bool>("Debe/Haber", ap.DebeHaber.GetAttribute<DebitCreditAtttribute>().Description);
//                DTable.Rows[i].SetField<string>("Concepto", ap.Concepto);
//                DTable.Rows[i].SetField<bool>("Punteo", ap.Punteado);
//                DTable.Rows[i].SetField<string>("Recibo", ap.Recibo);
//                DTable.Rows[i].SetField<string>("Factura", ap.Factura);
//                DTable.Rows[i].SetField<DateTime>("Fecha", this._Asiento.Fecha);
//                /*
//                [NApunte]    INT            IDENTITY (1, 1) NOT NULL,
//[NAsiento]   INT            NOT NULL,
//[Fecha]      DATE           NOT NULL,
//[Concepto]   NVARCHAR (MAX) NOT NULL,
//[Debe/Haber] BIT            DEFAULT ((0)) NOT NULL,
//[Importe]    MONEY          DEFAULT ((0)) NOT NULL,
//[Cuenta]     INT            NOT NULL,
//[Punteo]     BIT            DEFAULT ((0)) NULL,
//[Factura]    NVARCHAR (MAX) NULL,
//[Recibo]     NVARCHAR (MAX) NULL,
//                */

//                ledgeList.Add(ap.Account.Codigo);
//            }
//        }
//        //TODO
//        private int GetNAsiento(bool asientoIsNew)
//        {
//            //TODO
//            return 0;
//        }
//        #endregion
//    }
}
