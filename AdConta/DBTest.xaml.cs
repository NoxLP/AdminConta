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
using System.Windows.Shapes;
using System.Data;
using AdConta.Models;

namespace AdConta.Models
{
    /// <summary>
    /// Database test window. Interaction logic for DBTest.xaml
    /// </summary>
    public partial class DBTest : Window
    {
        public DBTest()
        {
            InitializeComponent();

            DataTable dt = new DataTable();
            DatabaseConnection dbcon = new DatabaseConnection();
            dbcon.SetDataTableByCommand("SELECT * FROM" + " C0Cuen5720001 " + "ORDER BY NAsiento, Fecha, FechaValor",
                ref dt, TableType.Mayor);
            /*dbcon.SetDataTableByCommand(Properties.Settings.Default.SELECTCDADES + " " + Properties.Settings.Default.ORDERCDADES, ref dt, 
                ContaWPF2.Models.TableType.Cdades);*/
            this.dgrid.ItemsSource = dt.DefaultView;
        }

        public DBTest(DataTable dt)
        {
            InitializeComponent();

            DatabaseConnection dbcon = new DatabaseConnection();
            dbcon.SetDataTableByCommand(GlobalSettings.Properties.Settings.Default.SELECTCDADES + GlobalSettings.Properties.Settings.Default.WHERECDADES_NOBAJA, ref dt,
                TableType.Cdades);
            this.dgrid.ItemsSource = dt.DefaultView;
        }
    }
}
