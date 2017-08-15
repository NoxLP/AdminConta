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

namespace ModuloGeneral
{
    /// <summary>
    /// Interaction logic for WindowModel_Confirmacion_ConAceptarYCancelar.xaml
    /// </summary>
    public partial class WindowModel_Confirmacion_ConAceptarYCancelar : Window
    {
        public WindowModel_Confirmacion_ConAceptarYCancelar()
        {
            InitializeComponent();
        }



        public string MensajeConfirmacion
        {
            get { return (string)GetValue(MensajeConfirmacionProperty); }
            set { SetValue(MensajeConfirmacionProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MensajeConfirmacion.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MensajeConfirmacionProperty =
            DependencyProperty.Register("MensajeConfirmacion", typeof(string), typeof(WindowModel_Confirmacion_ConAceptarYCancelar), new PropertyMetadata(""));

        private void ButAceptar_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }
    }
}
