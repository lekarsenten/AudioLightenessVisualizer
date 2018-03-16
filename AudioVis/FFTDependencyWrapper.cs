using FFTDataProvider;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace AudioVis
{
    [Serializable]
    public class ColorTemperatureDataDependencyWrapper : ColorTemperatureData, INotifyPropertyChanged
    {
        public ColorTemperatureDataDependencyWrapper(float LowHz, float HighHz, ushort fftSize, float sampleSize) : base(LowHz, HighHz, fftSize, sampleSize)
        {
        }

        public new ushort Low
        {
            get
            {
                return base.Low;
            }
            set
            {
                base.Low = value;
                OnPropertyChanged("Low");
            }
        }

        public new ushort High
        {
            get
            {
                return base.High;
            }
            set
            {
                base.High = value;
                OnPropertyChanged("High");
            }
        }

        public new float LowHz
        {
            get
            {
                return base.LowHz;
            }
            set
            {
                base.LowHz = value;
                OnPropertyChanged("LowHz");
            }
        }

        public new float HighHz
        {
            get
            {
                return base.HighHz;
            }
            set
            {
                base.HighHz = value;
                OnPropertyChanged("HighHz");
            }
        }

        public ColorTemperatureDataDependencyWrapper() : base()
        {

        }

        public void NotifyAboutProps()
        {
            OnPropertyChanged("Low");
            OnPropertyChanged("High");
            OnPropertyChanged("LowHz");
            OnPropertyChanged("HighHz");
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    [Serializable]
    public class FFTDependencyWrapper : FFTDataProvider.FFTDataMapper, INotifyPropertyChanged
    {
        private bool _isLinked;
        private ColorTemperatureDataDependencyWrapper _YellowLight;
        private ColorTemperatureDataDependencyWrapper _BlueLight;

        public bool TwoChannels
        {
            get
            {
                return FFTProvider.SeparateChannels;
            }
            set
            {
                FFTProvider.SeparateChannels = value;
                if (value)
                {
                    updateAverage();
                }
                OnPropertyChanged("TwoChannels");
            }
        }

        private void updateAverage()
        {
            _BlueLight.AvgHolders[1] = (AvgHolder)_BlueLight.AvgHolders[0].Clone();
            _YellowLight.AvgHolders[1] = (AvgHolder)_YellowLight.AvgHolders[0].Clone();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public new ColorTemperatureDataDependencyWrapper BlueLight
        {
            get
            {
                return _BlueLight;
            }

            set
            {
                _BlueLight = value;
                OnPropertyChanged("BlueLight");
                if (IsLinked)
                {
                    UpdateLinked(_BlueLight, YellowLight);
                }
            }
        }
        public new ColorTemperatureDataDependencyWrapper YellowLight
        {
            get
            {
                return _YellowLight;
            }

            set
            {
                _YellowLight = value;
                OnPropertyChanged("YellowLight");
                if (IsLinked)
                {
                    UpdateLinked(_YellowLight, BlueLight);
                }
            }
        }

        public bool IsLinked
        {
            get { return _isLinked; }
            set
            {
                _isLinked = value;
                OnPropertyChanged("IsLinked");
                UpdateLinked(_BlueLight, _YellowLight);
                OnPropertyChanged("BlueLight");
                OnPropertyChanged("YellowLight");
                OnPropertyChanged("NotIsLinked");
            }
        }
        public bool NotIsLinked
        {
            get
            {
                return !IsLinked;
            }
        }

        public FFTDependencyWrapper():base()
        {
            BlueLight = new ColorTemperatureDataDependencyWrapper(LowBucketLowValueHz, LowBucketHighValueHz, fftSize, base.FFTProvider.SamplesRate);
            YellowLight = new ColorTemperatureDataDependencyWrapper(HighBucketLowValueHz, HighBucketHighValueHz, fftSize, base.FFTProvider.SamplesRate);
        }

        public void UpdateLinked(ColorTemperatureDataDependencyWrapper source, ColorTemperatureDataDependencyWrapper target)
        {
            if (!IsLinked)
                return;
            target.Low = source.Low;
            target.High = source.High;
        }
        public void processFFT()
        {
            float[] fftData = FFTProvider.GetFftData();
            if (fftData != null)
            {
                BlueLight.CalcAvg(fftData);
                YellowLight.CalcAvg(fftData);
            }
            
            if (TwoChannels)
            {
                fftData = FFTProvider.GetFftData(true);
                if (fftData != null)
                {
                    BlueLight.CalcAvg(fftData, 1);
                    YellowLight.CalcAvg(fftData, 1);
                }
               
            }
        }
    }
}
