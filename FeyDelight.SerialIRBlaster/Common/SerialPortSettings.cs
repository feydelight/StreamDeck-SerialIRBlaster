using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;

namespace FeyDelight.SerialIRBlaster.Common
{
    internal abstract class SerialPortSettings
    {

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

        public Guid ID {
            get
            {
                return Guid.Parse(id);
            } 
            set
            {
                id = value.ToString();
            }
        }

        [JsonProperty(PropertyName = "id")]
        public string id { get; set; }

        public SerialPortSettings(Guid ID)
        {
            ComPort = null;
            BaudRate = 9600;
            DataBits = 8;
            Parity = Parity.None;
            StopBit = StopBits.One;
            this.ID = ID;
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
            string other = $"{os["comPort"]}{os["baudRate"]}{os["dataBits"]}{os["parity"]}{os["stopBit"]}";
            string this1 = $"{ComPort}{BaudRate}{DataBits}{Parity}{StopBit}";
            string this2 = $"{ComPort}{BaudRate}{DataBits}{(int)Parity}{(int)StopBit}";
            return (this1 == other || this2 == other);
        }
    }
}
