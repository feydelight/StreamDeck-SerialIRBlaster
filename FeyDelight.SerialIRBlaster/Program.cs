using BarRaider.SdTools;
using FeyDelight.SerialIRBlaster.Common;
using System;

namespace FeyDelight.SerialIRBlaster
{
    internal class Program
    {
        public static ISerialPortManager SerialPortManager { get; protected set; }
        static void Main(string[] args)
        {
            if (SerialPortManager == null)
            {
                SerialPortManager = new SerialPortManager();
            }
            SDWrapper.Run(args);
        }
    }
}
