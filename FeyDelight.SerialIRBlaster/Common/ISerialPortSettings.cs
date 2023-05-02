using Newtonsoft.Json.Linq;
using System.IO.Ports;

namespace FeyDelight.SerialIRBlaster.Common
{
    internal interface ISerialPortSettings
    {
        int BaudRate { get; set; }
        string ComPort { get; set; }
        int DataBits { get; set; }
        string DisplayName { get; set; }
        Parity Parity { get; set; }
        StopBits StopBit { get; set; }

        bool Equals(object obj);
        int GetHashCode();
        SerialPort GetSerialPort();
        bool IsEqualTo(JObject os);
        bool IsSettingChanging(ISerialPortSettings other);
    }
}