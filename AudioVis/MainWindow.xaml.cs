using ArduinoOutput;
using MahApps.Metro.Controls;
using System;

namespace AudioVis
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public FFTDependencyWrapper FftAndColorsSource { get; set; }

        public ArduinoSender ArduinoBridge { get; set; }

        System.Windows.Threading.DispatcherTimer t;
        public MainWindow()
        {
            FftAndColorsSource = new FFTDependencyWrapper();
            ArduinoBridge = ArduinoSender.Instance;
            //this.DataContext = FftAndColorsSource;            
            InitializeComponent();
            LeftSliders_tsl.FftAndColorsSource = FftAndColorsSource;
            t = new System.Windows.Threading.DispatcherTimer();
            t.Interval = new TimeSpan(0,0,0,0, 50);
            t.Tick += new EventHandler(dispatcherTimer_Tick);
            t.Start();
        }

        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            FftAndColorsSource.processFFT();
            Left_v.AddValues(FftAndColorsSource.BlueLight.Values[0], FftAndColorsSource.YellowLight.Values[0]);
            ArduinoBridge.SendValue(new Packet {channelData = (ChannelData.Left | ChannelData.LowFreq), Brightness = (byte)FftAndColorsSource.BlueLight.Values[0] });
            ArduinoBridge.SendValue(new Packet { channelData = (ChannelData.Left | ChannelData.HighFreq), Brightness = (byte)FftAndColorsSource.YellowLight.Values[0] });
            if (FftAndColorsSource.TwoChannels)
            {
                Right_v.AddValues(FftAndColorsSource.BlueLight.Values[1], FftAndColorsSource.YellowLight.Values[1]);
                ArduinoBridge.SendValue(new Packet { channelData = (ChannelData.Right | ChannelData.LowFreq), Brightness = (byte)FftAndColorsSource.BlueLight.Values[1] });
                ArduinoBridge.SendValue(new Packet { channelData = (ChannelData.Right | ChannelData.HighFreq), Brightness = (byte)FftAndColorsSource.YellowLight.Values[1] });
            }
        }

        

        private void MetroWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            t.Stop();
            FftAndColorsSource.Dispose();
            ArduinoBridge.Dispose();
        }

        private void ComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            ArduinoBridge.SetPort(Ports_cb.SelectedItem as String);
        }
    }
}
