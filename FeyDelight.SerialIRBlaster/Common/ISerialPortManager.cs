﻿using System.IO.Ports;
using System;

namespace FeyDelight.SerialIRBlaster.Common
{
    public delegate void ReplyDelegate(SerialPort Sender, string Line);
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
        /// <param name="replyDelegate">Event that you can subscribe to </param>
        /// <returns>Returns null if the port failed to open</returns>
        SerialPort GetSerialPort(ISerialPortRequester requester, ReplyDelegate replyDelegate);

        /// <summary>
        /// Closes a serial port. Continues to keep record of the settings for future use
        /// </summary>
        /// <param name="Requester"></param>
        void CloseSerialPort(ISerialPortRequester Requester);
        
        /// <summary>
        /// Adds serial port settings to the list of available serialports
        /// </summary>
        /// <param name="Settings"></param>
        /// <returns>true if a serial was added, false otherwise</returns>
        bool AddSerialPort(ISerialPortSettings Settings);

        /// <summary>
        /// Removes serial port and its connection information
        /// </summary>
        /// <param name="Key"></param>
        void RemoveSerialPort(string Key);
    }
}