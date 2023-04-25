using System.IO.Ports;
using System;

namespace FeyDelight.SerialIRBlaster.Common
{
    internal interface ISerialPortManager
    {
        void CloseAllSerialPorts();
        SerialPort GetSerialPort(SerialPortSettings Settings, SerialDataReceivedEventHandler serialPort_DataReceived);
        void CloseSerialPort(SerialPortSettings Settings, SerialDataReceivedEventHandler serialPort_DataReceived);
    }
}