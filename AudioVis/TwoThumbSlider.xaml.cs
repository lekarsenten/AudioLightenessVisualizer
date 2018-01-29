using FFTDataProvider;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace AudioVis
{
    /// <summary>
    /// Interaction logic for TwoThumbSlider.xaml
    /// </summary>
    public partial class TwoThumbSlider : UserControl
    {
        public event EventHandler ValueChanged;
        public TwoThumbSlider()
        {
            InitializeComponent();
        }



        public uint MinRangeWidth
        {
            get { return (uint)GetValue(MinRangeWidthProperty); }
            set { SetValue(MinRangeWidthProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MinRangeWidth.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MinRangeWidthProperty =
            DependencyProperty.Register("MinRangeWidth", typeof(uint), typeof(TwoThumbSlider), new PropertyMetadata((uint)0));



        public double Min
        {
            get { return (double)GetValue(MinProperty); }
            set { SetValue(MinProperty, value); }
        }

        public double Max
        {
            get { return (double)GetValue(MaxProperty); }
            set { SetValue(MaxProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Max.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MaxProperty =
            DependencyProperty.Register("Max", typeof(double), typeof(TwoThumbSlider));



        // Using a DependencyProperty as the backing store for Min.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MinProperty =
            DependencyProperty.Register("Min", typeof(double), typeof(TwoThumbSlider));



        public ushort Low
        {
            get { return (ushort)GetValue(LowProperty); }
            set { SetValue(LowProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Low.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LowProperty =
            DependencyProperty.Register("Low", typeof(ushort), typeof(TwoThumbSlider));



        public ushort High
        {
            get { return (ushort)GetValue(HighProperty); }
            set { SetValue(HighProperty, value); }
        }

        // Using a DependencyProperty as the backing store for High.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HighProperty =
            DependencyProperty.Register("High", typeof(ushort), typeof(TwoThumbSlider));

        private void Value_sl_UpperValueChanged(object sender, MahApps.Metro.Controls.RangeParameterChangedEventArgs e)
        {
            if (e.NewValue - Low < MinRangeWidth)
            {
                High = (ushort)e.OldValue;
                return;
            }
            ValueChanged?.Invoke(this, null);
        }

        private void Value_sl_LowerValueChanged(object sender, MahApps.Metro.Controls.RangeParameterChangedEventArgs e)
        {
            if (High - e.NewValue < MinRangeWidth)
            {
                Low = (ushort)e.OldValue;
                return;
            }
            ValueChanged?.Invoke(this, null);
        }
    }
}
