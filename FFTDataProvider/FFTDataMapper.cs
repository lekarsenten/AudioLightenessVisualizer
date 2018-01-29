using CSCore.DSP;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFTDataProvider
{
    public class AvgHolder : ICloneable
    {
        float minAvgFFT = 1;
        float maxAvgFFT = 0;
        public float CalcAvg(float[] data)
        {
            if (data.Length == 0)
            {
                return 0;
            }
            Avg = data.Average();//TODO: different avg patterns
            if (Avg < minAvgFFT)
            {
                minAvgFFT = Avg;
            }
            if (Avg > maxAvgFFT)
            {
                maxAvgFFT = Avg;
            }
            if (Avg.Equals(0.0))//reset min/max due to song/audio ending
            {
                minAvgFFT = 1;
                maxAvgFFT = 0;
            }
            return Avg;
        }

        public object Clone()
        {
            var copy = new AvgHolder();
            copy.minAvgFFT = this.minAvgFFT;
            copy.maxAvgFFT = this.maxAvgFFT;
            copy.Avg = this.Avg;
            return copy;
        }

        public float Percent
        {
            get
            {
                return (Avg - minAvgFFT) / (maxAvgFFT - minAvgFFT);
            }
        }

        public float Avg { get; private set; }
    }
    public class ColorTemperatureData
    {
        public ushort High
        {
            get
            {
                return _high;
            }
            set
            {
                _high = value;
                if (_high < _low)
                {
                    Low = _high;
                }
            }
        }
        public ushort Low
        {
            get { return _low; }
            set
            {
                _low = value;
                if (_low > _high)
                {
                    High = _low;
                }
            }
        }
        public float LowHz { get; set; }
        public float HighHz { get; set; }
        protected ushort LowIndex
        {
            get
            {
                return (ushort)Math.Floor(LowHz * fftSize / sampleSize);
            }
        }

        protected ushort HighIndex
        {
            get
            {
                return (ushort)Math.Floor(HighHz * fftSize / sampleSize);
            }
        }

        protected int BucketsSize
        {
            get
            {
                return HighIndex - LowIndex;
            }
        }
        
        public float Avg { get; private set; }
        private float minAvgFFT = 1;
        private float maxAvgFFT = 0;
        private ushort _high;
        private ushort _low;
        private readonly float fftSize;
        private readonly float sampleSize;

        public AvgHolder[] AvgHolders { get; set; }

        public ushort Value
        {
            get
            {
                return Values[0];
            }
        }

        public ushort[] Values
        {
            get
            {
                return AvgHolders.Select(x => (ushort)Math.Ceiling(Low + (High - Low) * x.Percent)).ToArray();
            }
        }
        public ColorTemperatureData(float LowHz, float HighHz, ushort fftSize, float sampleSize)
        {
            this.fftSize = fftSize;
            this.sampleSize = sampleSize;
            this.LowHz = LowHz;
            this.HighHz = HighHz;
            High = 255;
            Low = 0;
            AvgHolders = new AvgHolder[2];
            AvgHolders[0] = new AvgHolder();
            AvgHolders[1] = new AvgHolder();
        }
        
        public float CalcAvg(float[] data, byte index = 0)
        {
            var dataHolder = new float[BucketsSize];
            if (data.Length > BucketsSize)
                Array.Copy(data, LowIndex, dataHolder, 0, BucketsSize);
            else
                Array.Copy(data, dataHolder, BucketsSize);
            //Debug.Write(String.Format("LowHz: {0}; HighHz: {1}; LowIndex: {2}; HighIndex: {3}; Data:", LowHz, HighHz, LowIndex, HighIndex));
            //foreach (var item in dataHolder)
            //{
            //    Debug.Write(item + ",");
            //}
            //Debug.WriteLine("");
            return AvgHolders[index].CalcAvg(dataHolder);
        }
    }
    public class FFTDataMapper : IDisposable
    {
        public const float LowBucketLowValueHz = 50;
        public const float LowBucketHighValueHz = 500;
        public const float HighBucketLowValueHz = 500;
        public const float HighBucketHighValueHz = 5000;
        protected readonly ushort fftSize;
        public ColorTemperatureData BlueLight { get; set; }
        public ColorTemperatureData YellowLight { get; set; }
        FFTDataProvider _instance = null;
        protected FFTDataProvider FFTProvider
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new FFTDataProvider(FftSize.Fft2048);
                }
                return _instance;
            }
        }
        bool fftInitialized = false;

        public FFTDataMapper() : this(FftSize.Fft2048)
        {
        }

        private FFTDataMapper(FftSize fftSize = FftSize.Fft2048)
        {
            this.fftSize = (ushort)fftSize;
            _instance = new FFTDataProvider(fftSize);
            BlueLight = new ColorTemperatureData(LowBucketLowValueHz, LowBucketHighValueHz, (ushort)fftSize, _instance.SamplesRate);
            YellowLight = new ColorTemperatureData(HighBucketLowValueHz, HighBucketHighValueHz, (ushort)fftSize, _instance.SamplesRate);
        }
        public void processFFTData()
        {
            float[] fftData = _instance.GetFftData();
            if (fftData==null)
            {
                return;
            }
            float blueAvg = BlueLight.CalcAvg(fftData);
            float yellowAvg = YellowLight.CalcAvg(fftData);
        }

        public void Dispose()
        {
            ((IDisposable)_instance).Dispose();
        }
    }
}
