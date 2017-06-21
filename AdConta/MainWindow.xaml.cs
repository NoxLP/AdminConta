using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using AdConta.AbleTabControl;

namespace AdConta
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            DEBUGDELETEME_FillDatabase();

            InitializeComponent();

            this.WindowState = WindowState.Maximized;
            /*this.Dispatcher.BeginInvoke(
                System.Windows.Threading.DispatcherPriority.Loaded, 
                (Action)(() => this.AbleTabControl.DataContext = this.DataContext));*/
        }        

        #region helpers
        /// <summary>
        /// Bring to front given user control. Used by SideTool because Abletabcontrol tends to go to front by itself.
        /// </summary>
        /// <param name="pParent"></param>
        /// <param name="pToMove"></param>
        static public void BringToFront(Grid pParent, UserControl pToMove)
        {
            try
            {
                int currentIndex = Grid.GetZIndex(pToMove);
                int zIndex = 0;
                int maxZ = 0;
                UserControl child;
                for (int i = 0; i < pParent.Children.Count; i++)
                {
                    if (pParent.Children[i] is UserControl &&
                        pParent.Children[i] != pToMove)
                    {
                        child = pParent.Children[i] as UserControl;
                        zIndex = Grid.GetZIndex(child);
                        maxZ = Math.Max(maxZ, zIndex);
                        if (zIndex > currentIndex)
                        {
                            Grid.SetZIndex(child, zIndex - 1);
                        }
                    }
                }
                Grid.SetZIndex(pToMove, maxZ);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
        #endregion

        #region DEBUG BORRAME
        private void prueba_Click(object sender, RoutedEventArgs e)
        {
            /*if (this.AbleTabControl.TabItems[0].prueba != null)
                MessageBox.Show("OK");*/
            //this.SideTool.ExIsExpanded = !this.SideTool.ExIsExpanded;
            //MessageBox.Show("ok");
            AdConta.Models.DBTest test = new AdConta.Models.DBTest();
            test.Show();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void DEBUGDELETEME_FillDatabase()
        {
            /*AdConta.Models.DatabaseConnection db = new AdConta.Models.DatabaseConnection();

            db.ExecuteQuery(string.Format("INSERT INTO Cdades(Codigo,Baja,CIF,Nombre,Cuenta,Direccion,Presidente,FechaPunteo) VALUES ({0})",
                "0,0,'H54486654','C.P. EDIF. LUJANA, 2','ES0201820761230201625910','CALLE LUJANA, 2','PEPITO LORENZO','10/03/2016'"));
            db.ExecuteQuery(string.Format("INSERT INTO Cdades(Codigo,Baja,CIF,Nombre,Cuenta,Direccion,Presidente,FechaPunteo) VALUES ({0})",
                "1,0,'h78987987','C.P. EDIF. MAMPORRO I','ES0201820761230201625910','CALLE CAMORRA, 5','JUAN SANTANA','10/03/2016'"));
            db.ExecuteQuery(string.Format("INSERT INTO Props(Vvda,Coeficiente,Nombre,Direccion,CP,Cuenta,Remesa) VALUES ({0})",
                "'1ºC',0.0043,'PEPITO LORENZO','CALLE LUJANA, 2-1ºC',35005,0,0"));

            AdConta.Models.TabMayorDBConnection mdb = new Models.TabMayorDBConnection();

            mdb.CreateTable("C0Cuen5720001", "0", new Models.LedgeAccount("5720001"));

            using (System.Data.SqlClient.SqlConnection con = new System.Data.SqlClient.SqlConnection(db.ConnectionString))
            {
                con.Open();
                DateTime d = new DateTime(2016, 3, 26);
                System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand(
"INSERT INTO C0Cuen5720001(NAsiento,Fecha,FechaValor,Concepto,Debe,Haber,Saldo_1) VALUES (1,@date,@time,'ESTO ES UNA PRUEBA',1000,0,0)",
                    con);
                cmd.Parameters.AddWithValue("@date", d);
                cmd.Parameters.AddWithValue("@time", DateTime.Today);
                cmd.ExecuteNonQuery();
                con.Close();
            }*/

            /*db.ExecuteQuery(string.Format(
                "INSERT INTO C0Cuen5720001(NAsiento,Fecha,FechaValor,Concepto,Debe,Haber,Saldo_1) VALUES (
1,26/03/2016,@time,'ESTO ES UNA PRUEBA',1000,0,0)", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")));*/

            /*    [ID]       INT            IDENTITY (1, 1) NOT NULL,
[NAsiento] INT            DEFAULT ((0)) NOT NULL,
[Fecha]	   DATE			  NOT NULL,
[FechaValor]   DATETIME   NOT NULL,
[Concepto] NVARCHAR (MAX) NOT NULL,
[Debe]     INT            DEFAULT ((0)) NOT NULL,
[Haber]    INT            DEFAULT ((0)) NOT NULL,
[Saldo-1]  INT            DEFAULT ((0)) NOT NULL,
[Saldo]    AS             (([Debe]-[Haber])+[Saldo-1]),
[Factura]  INT            NULL,
[Recibo]   INT            NULL,
PRIMARY KEY CLUSTERED ([ID] ASC)
)*/
        }
        #endregion
    }
}