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

namespace AdConta
{
    /// <summary>
    /// Interaction logic for FalloAlCargarUC.xaml
    /// </summary>
    public partial class FalloAlCargarUC : UserControl
    {
        public FalloAlCargarUC()
        {
            InitializeComponent();
        }



        public string MensajeFallo
        {
            get { return (string)GetValue(MensajeFalloProperty); }
            set { SetValue(MensajeFalloProperty, value); }
        }

        // Using a DependencyProperty as the backing store for FalloalCargar.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MensajeFalloProperty =
            DependencyProperty.Register("FalloalCargar", typeof(string), typeof(FalloAlCargarUC), new PropertyMetadata(""));


    }
}
