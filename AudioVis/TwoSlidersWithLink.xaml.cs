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

namespace AudioVis
{
    /// <summary>
    /// Interaction logic for TwoSlidersWithLink.xaml
    /// </summary>
    public partial class TwoSlidersWithLink : UserControl
    {
        public FFTDependencyWrapper FftAndColorsSource { get; set; }
        public TwoSlidersWithLink()
        {
            FftAndColorsSource = new FFTDependencyWrapper();
            InitializeComponent();
        }

        private void Blue_sl_ValueChanged(object sender, EventArgs e)
        {
            if (!FftAndColorsSource.IsLinked)
            {
                return;
            }
            Yellow_sl.Low = Blue_sl.Low;
            Yellow_sl.High = Blue_sl.High;
        }
    }
}
