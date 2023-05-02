using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;

namespace FeyDelight.SerialIRBlaster.Common
{
    internal class SerialPortSettings : ISerialPortSettings
    {
        [JsonProperty(PropertyName = "displayName")]
        public string DisplayName { get; set; }

        [JsonProperty(PropertyName = "comPort")]
        public string ComPort { get; set; }

        [JsonProperty(PropertyName = "baudRate")]
        public int BaudRate { get; set; }

        [JsonProperty(PropertyName = "dataBits")]
        public int DataBits { get; set; }

        [JsonProperty(PropertyName = "parity")]
        public Parity Parity { get; set; }

        [JsonProperty(PropertyName = "stopBit")]
        public StopBits StopBit { get; set; }

        public SerialPortSettings()
        {
            DisplayName = null;
            ComPort = null;
            BaudRate = 9600;
            DataBits = 8;
            Parity = Parity.None;
            StopBit = StopBits.One;
        }

        public SerialPort GetSerialPort()
        {
            var sp = new SerialPort(
                        portName: this.ComPort,
                        baudRate: this.BaudRate,
                        parity: this.Parity,
                        dataBits: this.DataBits,
                        stopBits: this.StopBit
                        )
            {
                DtrEnable = true
            };
            return sp;
        }

        public bool IsEqualTo(JObject os)
        {
            var other = FromJObject(os);
            return this.ComPort == other.ComPort &&
                this.BaudRate == other.BaudRate &&
                this.DataBits == other.DataBits &&
                this.Parity == other.Parity &&
                this.StopBit == other.StopBit;
        }

        internal static SerialPortSettings FromJObject(JObject payload)
        {

            string comPort = (string)payload["comPort"];
            if (string.IsNullOrEmpty(comPort))
            {
                throw new ArgumentException(nameof(comPort));
            }
            string displayName = (string)payload["displayName"];
            if (string.IsNullOrEmpty(displayName))
            {
                displayName = comPort;
            }
            int baudRate = (int)payload["baudRate"];
            int dataBits = (int)payload["dataBits"];
            if (!Enum.TryParse(payload["parity"].ToString(), true, out Parity parity))
            {
                throw new ArgumentException(nameof(parity));
            }
            if (!Enum.TryParse(payload["stopBit"].ToString(), true, out StopBits stopBit))
            {
                throw new ArgumentException(nameof(stopBit));
            }

            return new SerialPortSettings
            {
                DisplayName = displayName,
                ComPort = comPort,
                BaudRate = baudRate,
                DataBits = dataBits,
                Parity = parity,
                StopBit = stopBit,
            };
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            if (ReferenceEquals(this, obj))
            {
                return true;
            }
            var other = obj as ISerialPortSettings;
            return
                this.DisplayName == other.DisplayName &&
                this.ComPort.Equals(other.ComPort) &&
                this.BaudRate.Equals(other.BaudRate) &&
                this.DataBits.Equals(other.DataBits) &&
                this.Parity.Equals(other.Parity) &&
                this.StopBit.Equals(other.StopBit);

        }

        public override int GetHashCode()
        {
            return
                DisplayName.GetHashCode() ^
                ComPort.GetHashCode() ^
                BaudRate.GetHashCode() ^
                DataBits.GetHashCode() ^
                Parity.GetHashCode() ^
                StopBit.GetHashCode();
        }

        public bool IsSettingChanging(ISerialPortSettings other)
        {
            return !
                (this.ComPort.Equals(other.ComPort) &&
                this.BaudRate.Equals(other.BaudRate) &&
                this.DataBits.Equals(other.DataBits) &&
                this.Parity.Equals(other.Parity) &&
                this.StopBit.Equals(other.StopBit));
        }
    }
}
