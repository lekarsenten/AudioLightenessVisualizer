using ArduinoOutput;
using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.IO;

namespace AudioVis
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    public partial class MainWindow : MetroWindow
    {
        internal class ChannelValues
        {
            internal ushort B;
            internal ushort Y;
        }
        internal class ComputedValues
        {
            internal bool twoChannels;
            internal ChannelValues L;
            internal ChannelValues R;
            internal bool valuesModified;
            internal void ModifyValues(Alarm alarm)
            {
                double BMult = alarm.CurrentMultiplierB;
                double YMult = alarm.CurrentMultiplierY;
                L.B = (ushort)(L.B * BMult);
                L.Y = (ushort)(L.Y * YMult);
                if (twoChannels)
                {
                    R.B = (ushort)(R.B * BMult);
                    R.Y = (ushort)(R.Y * YMult);
                }
                valuesModified = true;
            }
            internal ComputedValues(FFTDependencyWrapper wrapper)
            {
                L = new ChannelValues { B = wrapper.BlueLight.Values[0], Y = wrapper.YellowLight.Values[0] };
                if (wrapper.TwoChannels)
                {
                    twoChannels = true;
                    R = new ChannelValues { B = wrapper.BlueLight.Values[1], Y = wrapper.YellowLight.Values[1] };
                }
            }
            internal LinkedList<Packet> GetPackets()
            {
                LinkedList<Packet> packets = new LinkedList<Packet>();
                packets.AddLast(new Packet { channelData = (ChannelData.Left | ChannelData.LowFreq), Brightness = (byte)L.B });
                packets.AddLast(new Packet { channelData = (ChannelData.Left | ChannelData.HighFreq), Brightness = (byte)L.Y });
                if (twoChannels)
                {
                    packets.AddLast(new Packet { channelData = (ChannelData.Right | ChannelData.LowFreq), Brightness = (byte)R.B });
                    packets.AddLast(new Packet { channelData = (ChannelData.Right | ChannelData.HighFreq), Brightness = (byte)R.Y });
                }
                return packets;
            }
        }
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
            var values = getFinalValues(FftAndColorsSource);
            Left_v.AddValues(values.L.B, values.L.Y);
            if (FftAndColorsSource.TwoChannels)
            {
                Right_v.AddValues(values.R.B, values.R.Y);
            }
            foreach (var packet in values.GetPackets())
            {
                ArduinoBridge.SendValue(packet);
            }
        }

        private ComputedValues getFinalValues(FFTDependencyWrapper wrapper)
        {
            wrapper.processFFT();
            ComputedValues vals = new ComputedValues(wrapper);
            if (Sunset_alm.IsEnabled_cb.IsChecked == true)
            {
                vals.ModifyValues(Sunset_alm);
            }
            return vals;
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
            FftAndColorsSource.BlueLight.NotifyAboutProps();
            FftAndColorsSource.YellowLight.NotifyAboutProps();
        }
    }
}
