using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OutputBridges
{
    /// <summary>
    /// Left with Low is 0
    /// Left with High is 2
    /// Right with Low is 1
    /// Right with High is 3
    /// </summary>
    public struct ChannelValues
    {
        public ushort B;
        public ushort Y;
        public bool isRightChannel;
        public static implicit operator Tuple<ushort, ushort>(ChannelValues source)
        {
            return new Tuple<ushort, ushort>(source.B, source.Y);
        }
    }
    public abstract class PacketSender
    {
        public abstract bool SendValues(ChannelValues values);
        public abstract void SetTarget(string targetName);
        public virtual bool SendValues(Tuple<ChannelValues, ChannelValues> values)
        {
            return SendValues(values.Item1) && SendValues(values.Item2);
        }
    }
    public class BridgeRouter : PacketSender, IDisposable
    {
        public Dictionary<string, PacketSender> _targets;
        public string[] TargetsList
        {
            get
            {
                return _targets.Keys.ToArray();
            }
        }
        ArduinoSender arduinoBridge;
        HueSender hueBridge;
        private static BridgeRouter _instance;
        private static string currentTarget;

        private BridgeRouter()
        {
            arduinoBridge = ArduinoSender.Instance;
            hueBridge = new HueSender();
            _targets = new Dictionary<string, PacketSender>();
            _targets.Add("", null);
            _targets.Add("hue", hueBridge);
            foreach (string comPort in arduinoBridge.ComsList)
            {
                _targets.Add(comPort, arduinoBridge);
            }
            currentTarget = _targets.Keys.First();
        }

        public static BridgeRouter Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new BridgeRouter();
                }
                return _instance;
            }
        }

        public void Dispose()
        {
            if (arduinoBridge != null)
            {
                arduinoBridge.Dispose();
            }
        }

        public override void SetTarget(string targetName)
        {
            currentTarget = targetName;
            if (_targets[currentTarget] != null)
            {
                _targets[currentTarget].SetTarget(targetName);
            }
        }

        public override bool SendValues(ChannelValues values)
        {
            if (_targets[currentTarget] != null)
            {
                return _targets[currentTarget].SendValues(values);
            }
            else return false;
        }
    }
}
