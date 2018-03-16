using System;
using System.ComponentModel;
using System.Globalization;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;

namespace AudioVis
{
    [ValueConversion(typeof(bool?), typeof(bool?))]
    public class InvertBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value==null) return null;
            return !((bool)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    /// <summary>
    /// Interaction logic for Alarms.xaml
    /// </summary>
    public partial class Alarm : UserControl, INotifyPropertyChanged
    {
        private string _caption;
        private bool _isSunSet;
        private double _currentMultiplierB = 1;
        private double _currentMultiplierY = 1;

        public bool IsSunSet
        {
            get => _isSunSet; set
            {
                _isSunSet = value;
                OnPropertyChanged("IsSunSet");
            }
        }

        public string Caption
        {
            get => _caption; set
            {
                _caption = value;
                OnPropertyChanged("Caption");
            }
        }

        DispatcherTimer start_t;
        DispatcherTimer modifyTimer;
        public double CurrentMultiplierB
        {
            get => _currentMultiplierB; set
            {
                _currentMultiplierB = value;
                OnPropertyChanged("CurrentMultiplierB");
            }
        }
        public double CurrentMultiplierY
        {
            get => _currentMultiplierY; set
            {
                _currentMultiplierY = value;
                OnPropertyChanged("CurrentMultiplierY");
            }
        }
        public Alarm()
        {
            InitializeComponent();
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void IsEnabled_cb_Checked(object sender, RoutedEventArgs e)
        {
            if (interval_tp.SelectedTime == null || interval_tp.SelectedTime.Equals(TimeSpan.Zero) || end_tp.SelectedTime == null)
                return;
            TimeSpan currTime = DateTime.Now.TimeOfDay;
            TimeSpan? startTime = end_tp.SelectedTime - interval_tp.SelectedTime;
            TimeSpan? delta = startTime - currTime;
            if (delta.Value<TimeSpan.Zero)
            {
                MessageBox.Show("Error in setting alarm time!");
                return;
            }
            CurrentMultiplierB = CurrentMultiplierY = Convert.ToDouble(IsSunSet);
            start_t = new DispatcherTimer();
            start_t.Interval = delta.Value;
            start_t.Tick += Start_t_Tick;
            start_t.Start();
        }

        private void Start_t_Tick(object sender, EventArgs e)
        {
            (sender as DispatcherTimer).Stop();
            ushort millsPerPercent = (ushort)Math.Ceiling(interval_tp.SelectedTime.Value.TotalMilliseconds / 200);//100 for Blue and 100 for Yellow
            modifyTimer = new DispatcherTimer();
            modifyTimer.Interval = new TimeSpan(millsPerPercent*TimeSpan.TicksPerMillisecond);
            modifyTimer.Tick += ModifyTimer_Tick;
            modifyTimer.Start();
        }

        private void ModifyTimer_Tick(object sender, EventArgs e)
        {
            //bool changeB = !(CurrentMultiplierY < 1) || (IsSunSet && CurrentMultiplierB > 0);
            bool changeB = IsSunSet && (CurrentMultiplierB > 0) || !IsSunSet && !(CurrentMultiplierY < 1);
            double amount = (-1) * Convert.ToInt16(IsSunSet) * 0.01;
            CurrentMultiplierB = Math.Round(CurrentMultiplierB + Convert.ToInt16(changeB) * amount, 2);
            CurrentMultiplierY = Math.Round(CurrentMultiplierY + Convert.ToInt16(!changeB) * amount, 2);
            if (((CurrentMultiplierB + CurrentMultiplierY)<=0 && IsSunSet) || (CurrentMultiplierB + CurrentMultiplierY)>=2 && !IsSunSet)
            {
                (sender as DispatcherTimer).Stop();
            }
        }

        private void IsEnabled_cb_Unchecked(object sender, RoutedEventArgs e)
        {
            CurrentMultiplierB = CurrentMultiplierY = 1;
            start_t?.Stop();
            modifyTimer?.Stop();
        }
    }
}
