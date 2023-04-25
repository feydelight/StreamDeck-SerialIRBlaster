using BarRaider.SdTools;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;

namespace FeyDelight.SerialIRBlaster.Common
{
    internal class SerialPortManager : ISerialPortManager
    {
        private class SerialPortCache
        {
            public SerialPort SerialPort { get; set; }
            public HashSet<Guid> ConnectedActions { get; set; }
        }

        readonly object dictLock = new object();

        Dictionary<string, SerialPortCache> PortCaches { get; } = new Dictionary<string, SerialPortCache>();

        public SerialPort GetSerialPort(SerialPortSettings Settings, SerialDataReceivedEventHandler serialPort_DataReceived)
        {
            var key = Settings.ComPort;
            if (string.IsNullOrEmpty(key))
            {
                return null;
            }
            if (Settings.ID == Guid.Empty)
            {
                Logger.Instance.LogMessage(TracingLevel.INFO, $"Port request denied because of an invalid ID {Settings.ID}");
                return null;
            }
            lock (dictLock)
            {
                if (!PortCaches.TryGetValue(key, out var portCache))
                {
                    SerialPort serialPort;
                    try
                    {
                        Logger.Instance.LogMessage(TracingLevel.INFO, $"Setting port info...");
                        serialPort = Settings.GetSerialPort();
                        Logger.Instance.LogMessage(TracingLevel.INFO, $"Port info set.");
                    }
                    catch (Exception e)
                    {
                        Logger.Instance.LogMessage(TracingLevel.ERROR, $"Failed to set port info. This may be resolved later after all the settings are corrected: {e}");
                        return null;
                    }

                    portCache = new SerialPortCache
                    {
                        SerialPort = serialPort,
                        ConnectedActions = new HashSet<Guid>() {
                            Settings.ID,
                        },
                    };
                    PortCaches.Add(key, portCache);
                }
                else
                {
                    portCache.ConnectedActions.Add(Settings.ID);
                }

                portCache.SerialPort.DataReceived -= serialPort_DataReceived;
                portCache.SerialPort.DataReceived += serialPort_DataReceived;
                if (portCache.SerialPort.IsOpen == false)
                {
                    try
                    {
                        Logger.Instance.LogMessage(TracingLevel.INFO, $"Opening port {portCache.SerialPort.PortName} for {Settings.ID}...");
                        portCache.SerialPort.Open();
                        Logger.Instance.LogMessage(TracingLevel.INFO, $"Port {portCache.SerialPort.PortName} opened.");
                    }
                    catch (Exception e)
                    {
                        Logger.Instance.LogMessage(TracingLevel.ERROR, $"Failed to open Serial Port: {e}");
                        return null;
                    }
                }
                else
                {
                    Logger.Instance.LogMessage(TracingLevel.INFO, $"Port {portCache.SerialPort.PortName} already open. Passing to {Settings.ID}...");
                }
                return portCache.SerialPort;
            }
        }


        public void CloseSerialPort(SerialPortSettings Settings, SerialDataReceivedEventHandler serialPort_DataReceived)
        {
            var key = Settings.ComPort;
            if (string.IsNullOrEmpty(key))
            {
                return;
            }
            if (Settings.ID == Guid.Empty)
            {
                Logger.Instance.LogMessage(TracingLevel.INFO, $"Port close request denied because of an invalid ID {Settings.ID}");
                return;
            }
            lock (dictLock)
            {
                if (PortCaches.TryGetValue(key, out var port))
                {
                    Logger.Instance.LogMessage(TracingLevel.INFO, $"Removing {Settings.ID} from Port {key}...");
                    port.ConnectedActions.Remove(Settings.ID);
                    Logger.Instance.LogMessage(TracingLevel.INFO, $"Removed {Settings.ID} from Port {key}.");

                    if (port.ConnectedActions.Count > 0)
                    {
                        port.SerialPort.DataReceived -= serialPort_DataReceived;
                        return;
                    }
                    Logger.Instance.LogMessage(TracingLevel.INFO, $"No other Actions connected. Closing {key}...");
                    port.SerialPort.Close();
                    PortCaches.Remove(key);
                    Logger.Instance.LogMessage(TracingLevel.INFO, $"Closed {key}");
                }
            }
        }

        public void CloseAllSerialPorts()
        {
            Logger.Instance.LogMessage(TracingLevel.INFO, $"Closing all Serial ports...");
            lock (dictLock)
            {
                foreach (var key in PortCaches.Keys)
                {
                    Logger.Instance.LogMessage(TracingLevel.INFO, $"Closing {key}...");
                    var port = PortCaches[key];
                    port.SerialPort.Close();
                    Logger.Instance.LogMessage(TracingLevel.INFO, $"Closed {key}");
                }
                PortCaches.Clear();
            }
            Logger.Instance.LogMessage(TracingLevel.INFO, $"All ports closed.");
        }
    }
}
