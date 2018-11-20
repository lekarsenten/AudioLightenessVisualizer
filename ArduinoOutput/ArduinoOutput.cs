using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OutputBridges
{

    
    public class ArduinoSender : PacketSender, INotifyPropertyChanged, IDisposable
    {
        private static List<string> _comsList;
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

        public List<string> ComsList
        {
            get
            {
                if (_comsList == null)
                {
                    _comsList = SerialPort.GetPortNames().ToList();
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
            SetTarget(portName);
        }

        public override void SetTarget(string portName)
        {
            Dispose();//close previous COM port
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

        public override bool SendValues(ChannelValues packet)
        {
            if (!IsInitialized)
            {
                return false;
            }
            try
            {
                _currentComPort.Write(GetMessage(packet));
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void Dispose()
        {
            if (_currentComPort != null)
            {
                ((IDisposable)_currentComPort).Dispose();
            }
        }

        private string GetMessage(ChannelValues value)
        {
            return string.Format("{0}:{1}|{2}{3}", Convert.ToByte(value.isRightChannel), value.B, value.Y, Environment.NewLine);
        }
    }
}
