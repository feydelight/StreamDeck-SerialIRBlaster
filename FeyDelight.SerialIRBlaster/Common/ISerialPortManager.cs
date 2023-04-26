using System.IO.Ports;
using System;

namespace FeyDelight.SerialIRBlaster.Common
{
    internal interface ISerialPortManager
    {
        /// <summary>
        /// Closes all serial ports. Continues to keep record of the settings for future use
        /// </summary>
        void CloseAllSerialPorts();

        /// <summary>
        /// Retrieves a serial port. If the port is not yet opened, it will attempt to open it.
        /// </summary>
        /// <param name="Requester"></param>
        /// <param name="serialPort_DataReceived"></param>
        /// <returns>Returns null if the port failed to open</returns>
        SerialPort GetSerialPort(SerialPortRequester Requester, SerialDataReceivedEventHandler serialPort_DataReceived);
        
        /// <summary>
        /// Closes a serial port. Continues to keep record of the settings for future use
        /// </summary>
        /// <param name="Requester"></param>
        /// <param name="serialPort_DataReceived"></param>
        void CloseSerialPort(SerialPortRequester Requester, SerialDataReceivedEventHandler serialPort_DataReceived);
        
        /// <summary>
        /// Adds serial port settings to the list of available serialports
        /// </summary>
        /// <param name="Settings"></param>
        /// <returns>true if a serial was added, false otherwise</returns>
        bool AddSerialPort(SerialPortSettings Settings);

        /// <summary>
        /// Removes serial port and its connection information
        /// </summary>
        /// <param name="Key"></param>
        void RemoveSerialPort(string Key);
    }
}