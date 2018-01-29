using FFTDataProvider;
using LiveCharts;
using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
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
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public FFTDependencyWrapper FftAndColorsSource { get; set; }

        System.Windows.Threading.DispatcherTimer t;
        public MainWindow()
        {
            FftAndColorsSource = new FFTDependencyWrapper();
            //this.DataContext = FftAndColorsSource;            
            InitializeComponent();
            LeftSliders_tsl.FftAndColorsSource = FftAndColorsSource;
            t = new System.Windows.Threading.DispatcherTimer();
            t.Interval = new TimeSpan(0,0,0,0, 25);
            t.Tick += new EventHandler(dispatcherTimer_Tick);
            t.Start();
        }

        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            FftAndColorsSource.processFFT();
            Left_v.AddValues(FftAndColorsSource.BlueLight.Values[0], FftAndColorsSource.YellowLight.Values[0]);
            if (Right_ex.IsExpanded && FftAndColorsSource.TwoChannels)
            {
                Right_v.AddValues(FftAndColorsSource.BlueLight.Values[1], FftAndColorsSource.YellowLight.Values[1]);
            }
        }

        

        private void MetroWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            t.Stop();
            FftAndColorsSource.Dispose();
        }
    }
}
