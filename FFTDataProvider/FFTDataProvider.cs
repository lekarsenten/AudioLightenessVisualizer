using CSCore;
using CSCore.DSP;
using CSCore.SoundIn;
using CSCore.Streams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFTDataProvider
{
    public class FFTDataProvider : IDisposable
    {
        public readonly FftSize fftSize;
        private static bool _readStarted = false;
        private WasapiLoopbackCapture _soundIn;
        private FftProvider fftProvider;
        private FftProvider fftProvider2;
        public bool SeparateChannels { get; set; }
        public int SamplesRate{ get; private set; }

        public FFTDataProvider(FftSize fftSize)
        {
            this.fftSize = fftSize;
            _soundIn = new WasapiLoopbackCapture();
            _soundIn.Initialize();
            var soundInSource = new SoundInSource(_soundIn);
            ISampleSource source = soundInSource.ToSampleSource();
            fftProvider = new FftProvider(source.WaveFormat.Channels, fftSize);
            fftProvider2 = new FftProvider(source.WaveFormat.Channels, fftSize);
            var notificationSource = new SingleBlockNotificationStream(source);
            SamplesRate = source.WaveFormat.SampleRate;
            //pass the intercepted samples as input data to the spectrumprovider (which will calculate a fft based on them)
            notificationSource.SingleBlockRead += addToFFTs;
            var _source = notificationSource.ToWaveSource(16);
            byte[] buffer = new byte[_source.WaveFormat.BytesPerSecond / 8];// 1/8 seconds
            soundInSource.DataAvailable += (s, aEvent) =>
            {
                int read;
                while ((read = _source.Read(buffer, 0, buffer.Length)) > 0) ;
            };
            _readStarted = false;
        }

        private void addToFFTs(object sender, SingleBlockReadEventArgs e)
        {
            if (!SeparateChannels)
            {
                fftProvider.Add(e.Left, e.Right);
                return;
            }
            else
            {
                fftProvider.Add(e.Left, e.Left);
                fftProvider2.Add(e.Right, e.Right);
            }
        }

        public float[] GetFftData()
        {
            if (!_readStarted)
            {
                start();
                return null;
            }
            var fftBuffer = new float[(int)fftSize];
            if (fftProvider.GetFftData(fftBuffer))
            {
                return fftBuffer;
            }
            return null;
        }

        public float[] GetFftData(bool takeRightChannel = false)
        {
            if (!SeparateChannels || !takeRightChannel)
            {
                return GetFftData();
            }
            else
            {
                if (!_readStarted)
                {
                    start();
                    return null;
                }
                var fftBuffer = new float[(int)fftSize];
                if (fftProvider2.GetFftData(fftBuffer))
                {
                    return fftBuffer;
                }
                return null;
            }
            
        }

        private void start()
        {
            //play the audio
            _soundIn.Start();
            _readStarted = true;
        }

        public void Dispose()
        {
            _soundIn.Stop();
            _soundIn.Dispose();
        }
    }
}
