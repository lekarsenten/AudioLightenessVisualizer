using ArduinoOutput;
using MahApps.Metro.Controls;
using System;
using System.IO;

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
            FftAndColorsSource = SettingsSerializationManager.DeSerialize("default.xml", typeof(FFTDependencyWrapper)) as FFTDependencyWrapper;
            ArduinoBridge = ArduinoSender.Instance;
            InitializeComponent();
            initializeBindings();
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
            SettingsSerializationManager.Serialize("default.xml", FftAndColorsSource);
            FftAndColorsSource.Dispose();
            ArduinoBridge.Dispose();
        }

        private void ComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            ArduinoBridge.SetPort(Ports_cb.SelectedItem as String);
        }

        public void ProfileControl_CheckBoxClicked(object sender, Tuple<ProfileAction, byte> e)
        {
            switch (e.Item1)
            {
                case ProfileAction.Create:
                    SettingsSerializationManager.Serialize(string.Format("{0}.xml", e.Item2), FftAndColorsSource);
                    break;
                case ProfileAction.Delete:
                    File.Delete(string.Format("{0}.xml", e.Item2));
                    break;
                case ProfileAction.Apply:
                    FftAndColorsSource = SettingsSerializationManager.DeSerialize(string.Format("{0}.xml", e.Item2), typeof(FFTDependencyWrapper)) as FFTDependencyWrapper;
                    initializeBindings();
                    break;
                default:
                    break;
            }
        }

        private void initializeBindings()
        {
            LeftSliders_tsl.FftAndColorsSource = FftAndColorsSource;
        }
    }
}
