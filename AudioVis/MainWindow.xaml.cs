using OutputBridges;
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
        internal const double pF = 0.01;
        internal class ComputedValues
        {
            internal bool twoChannels;
            internal ChannelValues L;
            internal ChannelValues R;
            internal bool valuesModified;
            internal void ModifyValues_Alarm(Alarm alarm)
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
            internal void ModifyValues_toLog()
            {
                L.B = valueToLog(L.B);
                L.Y = valueToLog(L.Y);
                R.B = valueToLog(R.B);
                R.Y = valueToLog(R.Y);
            }
            private ushort valueToLog(double linear)
            {
                double percent = linear / 256;
                double logPercent = Math.Pow(pF, 1 - percent) - pF;
                //double logPercent = 1 - (Math.Log10(percent + pF)/Math.Log10(pF));
                return Convert.ToUInt16(255 * logPercent);
            }
            internal ComputedValues(FFTDependencyWrapper wrapper)
            {
                L = new ChannelValues { B = wrapper.BlueLight.Values[0], Y = wrapper.YellowLight.Values[0], isRightChannel = false };
                if (wrapper.TwoChannels)
                {
                    twoChannels = true;
                    R = new ChannelValues { B = wrapper.BlueLight.Values[1], Y = wrapper.YellowLight.Values[1], isRightChannel = true };
                }
            }
        }
        public FFTDependencyWrapper FftAndColorsSource { get; set; }

        public BridgeRouter Sender { get; set; }

        System.Windows.Threading.DispatcherTimer t;
        public MainWindow()
        {
            FftAndColorsSource = SettingsSerializationManager.DeSerialize("default.xml", typeof(FFTDependencyWrapper)) as FFTDependencyWrapper;
            Sender = BridgeRouter.Instance;
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
            Sender.SendValues(values.L);
            if (!FftAndColorsSource.TwoChannels)
            {
                values.R = values.L;
                values.R.isRightChannel = true;
            }
            Right_v.AddValues(values.R.B, values.R.Y);
            Sender.SendValues(values.R);
        }

        private ComputedValues getFinalValues(FFTDependencyWrapper wrapper)
        {
            wrapper.processFFT();
            ComputedValues vals = new ComputedValues(wrapper);
            if (Sunset_alm.IsEnabled_cb.IsChecked == true)
            {
                vals.ModifyValues_Alarm(Sunset_alm);
            }
            if (Log_cb.IsChecked == true)
            {
                vals.ModifyValues_toLog();
            }
            return vals;
        }

        private void MetroWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            t.Stop();
            SettingsSerializationManager.Serialize("default.xml", FftAndColorsSource);
            FftAndColorsSource.Dispose();
            Sender.Dispose();
        }

        private void ComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            Sender.SetTarget(Ports_cb.SelectedItem as String);
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
