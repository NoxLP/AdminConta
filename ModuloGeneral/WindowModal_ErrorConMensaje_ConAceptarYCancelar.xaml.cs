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
    /// Interaction logic for WindowModal_ErrorConMensaje_ConAceptarYCancelar.xaml
    /// </summary>
    public partial class WindowModal_ErrorConMensaje_ConAceptarYCancelar : Window
    {
        public WindowModal_ErrorConMensaje_ConAceptarYCancelar()
        {
            InitializeComponent();
        }

        #region properties
        public string Titulo
        {
            get { return (string)GetValue(TituloProperty); }
            set { SetValue(TituloProperty, value); }
        }
        public string MensajeError
        {
            get { return (string)GetValue(MensajeErrorProperty); }
            set { SetValue(MensajeErrorProperty, value); }
        }
        public string TextoBotonAceptar
        {
            get { return (string)GetValue(TextoBotonAceptarProperty); }
            set { SetValue(TextoBotonAceptarProperty, value); }
        }
        public string TextoBotonCancelar
        {
            get { return (string)GetValue(TextoBotonCancelarProperty); }
            set { SetValue(TextoBotonCancelarProperty, value); }
        }
        #endregion

        #region static dependency properteis
        // Using a DependencyProperty as the backing store for Titulo.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TituloProperty =
            DependencyProperty.Register("Titulo", typeof(string), typeof(WindowModal_ErrorConMensaje_ConAceptarYCancelar), new FrameworkPropertyMetadata(""));
        
        // Using a DependencyProperty as the backing store for MensajeError.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MensajeErrorProperty =
            DependencyProperty.Register("MensajeError", typeof(string), typeof(WindowModal_ErrorConMensaje_ConAceptarYCancelar), new FrameworkPropertyMetadata(""));
        
        // Using a DependencyProperty as the backing store for TextoBotonAceptar.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TextoBotonAceptarProperty =
            DependencyProperty.Register("TextoBotonAceptar", typeof(string), typeof(WindowModal_ErrorConMensaje_ConAceptarYCancelar), new FrameworkPropertyMetadata("Aceptar"));
        
        // Using a DependencyProperty as the backing store for TextoBotonCancelar.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TextoBotonCancelarProperty =
            DependencyProperty.Register("TextoBotonCancelar", typeof(string), typeof(WindowModal_ErrorConMensaje_ConAceptarYCancelar), new FrameworkPropertyMetadata("Cancelar"));
        #endregion

        private void ButAceptar_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }
    }
}
