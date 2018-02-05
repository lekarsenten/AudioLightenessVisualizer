using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArduinoOutput
{
    /// <summary>
    /// Left with Low is 0
    /// Left with High is 2
    /// Right with Low is 1
    /// Right with High is 3
    /// </summary>
    [Flags]
    public enum ChannelData
    {
        Left = 0x0,
        Right = 0x1,
        LowFreq = 0x0,
        HighFreq = 0x2
    }
    public struct Packet
    {
        public ChannelData channelData;
        public byte Brightness;
        public string GetMessage()
        {
            return string.Format("{0}:{1}{2}", (byte)channelData, Brightness, Environment.NewLine);
        }
    }
    public class ArduinoSender : INotifyPropertyChanged, IDisposable
    {
        private static string[] _comsList;
        private SerialPort _currentComPort;
        private static ArduinoSender _instance;

        public bool IsInitialized
        {
            get
            {
                return _currentComPort != null;
            }
        }

        public static ArduinoSender Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new ArduinoSender();
                }
                return _instance;
            }
        }

        public string[] ComsList
        {
            get
            {
                if (_comsList == null)
                {
                    _comsList = SerialPort.GetPortNames();
                }
                return _comsList;
            }
        }

        public SerialPort CurrentComPort { get => _currentComPort; }

        private ArduinoSender()
        {

        }
        
        public ArduinoSender(string portName)
        {
            SetPort(portName);
        }

        public void SetPort(string portName)
        {
            _currentComPort = new SerialPort(portName);
            _currentComPort.BaudRate = 19200;
            _currentComPort.WriteTimeout = 100;
            _currentComPort.ReadTimeout = 100;
            try
            {
                _currentComPort.Open();
            }
            catch
            {
                _currentComPort = null;
            }
        }

        public bool SendValue(Packet packet)
        {
            if (!IsInitialized)
            {
                return false;
            }
            try
            {
                _currentComPort.Write(packet.GetMessage());
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        public bool SendValue(ChannelData channelData, byte brightness)
        {
            return SendValue(new Packet { channelData = channelData, Brightness = brightness });
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void Dispose()
        {
            ((IDisposable)_currentComPort).Dispose();
        }
    }
}
