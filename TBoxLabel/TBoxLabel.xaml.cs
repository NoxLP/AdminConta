using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;
using System.Data;

namespace AdConta.UserControls
{
    /// <summary>
    /// Simple user control with textbox + label. Interaction logic for TBoxLabel.xaml
    /// </summary>
    public partial class TBoxLabel : UserControl, INotifyPropertyChanged
    {
        public TBoxLabel()
        {
            InitializeComponent();

            if (this.BaseTBox.IsReadOnly)
                this.BaseTBox.Background = (Brush)Application.Current.Resources["BackgroundAppColor"];
            else
                this.BaseTBox.Background = Brushes.White;
        }

        #region PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        protected void NotifyPropChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public void PublicNotifyPropChanges(string propertyName)
        {
            this.NotifyPropChanged(propertyName);
        }
        #endregion        

        #region dependency properties
        #region static DependencyProperty
        public static readonly DependencyProperty TBWidthProperty =
            DependencyProperty.Register("TBWidth", typeof(double), typeof(TBoxLabel), new PropertyMetadata(new Double(), OnTBWidthChange));

        public static readonly DependencyProperty TBHeightProperty =
            DependencyProperty.Register("TBHeight", typeof(double), typeof(TBoxLabel), new PropertyMetadata(double.NaN));

        public static readonly DependencyProperty LabelProperty =
            DependencyProperty.Register("Label", typeof(string), typeof(TBoxLabel), new PropertyMetadata("Label"));

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(TBoxLabel), new PropertyMetadata(""));

        public static readonly DependencyProperty TBFontSizeProperty =
            DependencyProperty.Register("TBFontSize", typeof(double), typeof(TBoxLabel), new PropertyMetadata(GlobalSettings.Properties.Settings.Default.USERFONTSIZE));

        public static readonly DependencyProperty TBFontStyleProperty =
            DependencyProperty.Register("TBFontStyle", typeof(FontStyle), typeof(TBoxLabel), new PropertyMetadata());

        public static readonly DependencyProperty TBFontWeightProperty =
            DependencyProperty.Register("TBFontWeight", typeof(FontWeight), typeof(TBoxLabel), new PropertyMetadata());

        public static readonly DependencyProperty TextWrapProperty =
            DependencyProperty.Register("TextWrap", typeof(TextWrapping), typeof(TBoxLabel), new PropertyMetadata(TextWrapping.NoWrap));

        public static readonly DependencyProperty AcceptsReturnProperty =
            DependencyProperty.Register("AcceptsRet", typeof(bool), typeof(TBoxLabel), new PropertyMetadata(false));

        public static readonly DependencyProperty IsReadOnlyProperty =
            DependencyProperty.Register("TBReadOnly", typeof(bool), typeof(TBoxLabel), new PropertyMetadata(true, OnReadOnlyChange));

        public static readonly DependencyProperty TBTextAlignmentProperty =
            DependencyProperty.Register("TBTextAlignment", typeof(TextAlignment), typeof(TBoxLabel), new PropertyMetadata(TextAlignment.Left));

        public static readonly DependencyProperty EnterKeyBindingProperty =
            DependencyProperty.Register("EnterKeyBinding", typeof(ICommand), typeof(TBoxLabel), new PropertyMetadata(OnEnterKeyBindingChange));
        #endregion

        #region public and OnChangeHandlers
        public double TBWidth
        {
            get { return (double)GetValue(MinWidthProperty); }
            set { SetValue(MinWidthProperty, value); }
        }
        private static void OnTBWidthChange(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            TBoxLabel control = d as TBoxLabel;

            double n = (double)e.NewValue;
            if (n != double.NaN)
                control.BaseTBox.Width = (n < control.BaseLabel.Width ? control.BaseLabel.Width : n);
        }

        public double TBHeight
        {
            get { return (double)GetValue(TBHeightProperty); }
            set { SetValue(TBHeightProperty, value); }
        }

        public string Label
        {
            get { return (string)GetValue(LabelProperty); }
            set { SetValue(LabelProperty, value); }
        }

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public double TBFontSize
        {
            get { return (double)GetValue(TBFontSizeProperty); }
            set { SetValue(TBFontSizeProperty, value); }
        }

        public FontStyle TBFontStyle
        {
            get { return (FontStyle)GetValue(TBFontStyleProperty); }
            set { SetValue(TBFontStyleProperty, value); }
        }

        public FontWeight TBFontWeight
        {
            get { return (FontWeight)GetValue(TBFontWeightProperty); }
            set { SetValue(TBFontWeightProperty, value); }
        }

        public TextWrapping TextWrap
        {
            get { return (TextWrapping)GetValue(TextWrapProperty); }
            set { SetValue(TextWrapProperty, value); }
        }

        public bool AcceptsRet
        {
            get { return (bool)GetValue(AcceptsReturnProperty); }
            set { SetValue(AcceptsReturnProperty, value); }
        }

        public bool TBReadOnly
        {
            get { return (bool)GetValue(IsReadOnlyProperty); }
            set { SetValue(IsReadOnlyProperty, value); }
        }
        private static void OnReadOnlyChange(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            TBoxLabel control = d as TBoxLabel;

            if ((bool)e.NewValue == true)
                control.BaseTBox.Background = (Brush)Application.Current.Resources["BackgroundAppColor"];
            else if ((bool)e.NewValue == false)
                control.BaseTBox.Background = Brushes.White;
        }

        public TextAlignment TBTextAlignment
        {
            get { return (TextAlignment)GetValue(TBTextAlignmentProperty); }
            set { SetValue(TBTextAlignmentProperty, value); }
        }

        public ICommand EnterKeyBinding
        {
            get { return (ICommand)GetValue(EnterKeyBindingProperty); }
            set { SetValue(EnterKeyBindingProperty, value); }
        }
        private static void OnEnterKeyBindingChange(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            TBoxLabel control = d as TBoxLabel;

            if (e.NewValue == null)
            {
                control.EnterKeyBinding = new Command_EnterKeyDefault(control);
            }
            else if (e.NewValue != control.EnterKeyBinding)
            {
                control.EnterKeyBinding = (ICommand)e.NewValue;
            }
        }
        #endregion
        #endregion
    }

    public class Command_EnterKeyDefault : ICommand
    {
        private TBoxLabel _TBL;

        public Command_EnterKeyDefault(TBoxLabel tbl)
        {
            this._TBL = tbl;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public void Execute(object parameter)
        {
            this._TBL.PublicNotifyPropChanges(this._TBL.Text);
        }
    }
}
