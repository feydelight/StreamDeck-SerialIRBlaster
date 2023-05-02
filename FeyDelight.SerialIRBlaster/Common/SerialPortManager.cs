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
            public Dictionary<Guid, ReplyDelegate> ConnectedActions { get; set; }
            public ISerialPortSettings SerialPortSettings { get; set; }
        }

        readonly object dictLock = new object();

        Dictionary<string, SerialPortCache> PortCaches { get; } = new Dictionary<string, SerialPortCache>();

        public bool AddSerialPort(ISerialPortSettings settings)
        {
            var key = settings.ComPort;
            if (string.IsNullOrEmpty(key))
            {
                return false;
            }

            lock (dictLock)
            {
                if (PortCaches.ContainsKey(key))
                {
                    var currSettings = PortCaches[key].SerialPortSettings;
                    if (currSettings.IsSettingChanging(settings) == false)
                    {
                        return false;
                    }
                    var serialPort = PortCaches[key].SerialPort;
                    if (serialPort?.IsOpen ?? false)
                    {
                        serialPort.Close();
                    }
                    PortCaches[key].SerialPort = null;
                    PortCaches[key].SerialPortSettings = settings;
                    // connectedActions can stay connected;

                }
                else
                {
                    PortCaches.Add(key, new SerialPortCache
                    {
                        ConnectedActions = new Dictionary<Guid, ReplyDelegate>(),
                        SerialPort = null,
                        SerialPortSettings = settings
                    });
                }
            }
            return true;
        }

        public void RemoveSerialPort(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return;
            }
            lock (dictLock)
            {
                if (PortCaches.TryGetValue(key, out var serialPortCache) == false)
                {
                    return;
                }
                if (serialPortCache.SerialPort?.IsOpen ?? false)
                {
                    serialPortCache.SerialPort.Close();
                }
                PortCaches.Remove(key);
            }
        }

        public SerialPort GetSerialPort(ISerialPortRequester requester, ReplyDelegate replyDelegate)
        {
            var key = requester.Key;
            if (string.IsNullOrEmpty(key))
            {
                return null;
            }
            if (requester.ID == Guid.Empty)
            {
                Logger.Instance.LogMessage(TracingLevel.INFO, $"Port request denied because of an invalid ID {requester.ID}");
                return null;
            }
            lock (dictLock)
            {
                Logger.Instance.LogMessage(TracingLevel.INFO, $"ID {requester.ID} requesting port: {key}");
                if (PortCaches.TryGetValue(key, out var portCache) == false)
                {
                    Logger.Instance.LogMessage(TracingLevel.INFO, $"Requesting port that doesn't exist or has been deleted {requester.ID}");
                    return null;
                }

                if (portCache.SerialPort == null)
                {
                    try
                    {
                        Logger.Instance.LogMessage(TracingLevel.INFO, $"Setting port info...");
                        portCache.SerialPort = portCache.SerialPortSettings.GetSerialPort();
                        Logger.Instance.LogMessage(TracingLevel.INFO, $"Port info set.");
                    }
                    catch (Exception e)
                    {
                        Logger.Instance.LogMessage(TracingLevel.ERROR, $"Failed to set port info. This may be resolved later after all the settings are corrected: {e}");
                        return null;
                    }
                }
                portCache.ConnectedActions[requester.ID] = replyDelegate;

                if (portCache.SerialPort.IsOpen == false)
                {
                    try
                    {
                        Logger.Instance.LogMessage(TracingLevel.INFO, $"Adding DataReceived hook to {portCache.SerialPort.PortName}...");
                        portCache.SerialPort.DataReceived += SerialPort_DataReceived;
                        Logger.Instance.LogMessage(TracingLevel.INFO, $"Successfully added hook to {portCache.SerialPort.PortName}.");
                        Logger.Instance.LogMessage(TracingLevel.INFO, $"Opening port {portCache.SerialPort.PortName}...");
                        portCache.SerialPort.Open();
                        Logger.Instance.LogMessage(TracingLevel.INFO, $"Port {portCache.SerialPort.PortName} opened.");                        
                    }
                    catch (Exception e)
                    {
                        Logger.Instance.LogMessage(TracingLevel.ERROR, $"Failed to open Serial Port: {e}");
                        return null;
                    }
                }
                return portCache.SerialPort;
            }
        }
        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort sp = (SerialPort)sender;
            try
            {
                string indata = sp.ReadLine().Trim();
                if (string.IsNullOrEmpty(indata))
                {
                    return;
                }
                lock(dictLock)
                {
                    string key = sp.PortName;
                    if (PortCaches.TryGetValue(key, out var portCache) == false)
                    {
                        Logger.Instance.LogMessage(TracingLevel.INFO, $"Port {key} sent messages after it was asked to be closed.");
                        return;
                    }
                    //Logger.Instance.LogMessage(TracingLevel.INFO, $"Serial replied: '{indata}'");
                    foreach(var action in portCache.ConnectedActions)
                    {
                        if (action.Value == null)
                        {
                            continue;
                        }
                        //Logger.Instance.LogMessage(TracingLevel.INFO, $"Sending it to: {action.Key}");
                        action.Value.Invoke(sp, indata);
                    }
                }
            }
            catch (Exception)
            {
                // either timed out, or the port got closed. either way, no biggy.
                return;
            }
        }


        public void CloseSerialPort(ISerialPortRequester requester)
        {
            var key = requester.Key;
            if (string.IsNullOrEmpty(key))
            {
                return;
            }
            if (requester.ID == Guid.Empty)
            {
                Logger.Instance.LogMessage(TracingLevel.INFO, $"Port close request denied because of an invalid ID {requester.ID}");
                return;
            }
            lock (dictLock)
            {
                if (PortCaches.TryGetValue(key, out var port))
                {
                    Logger.Instance.LogMessage(TracingLevel.INFO, $"Removing {requester.ID} from Port {key}...");
                    port.ConnectedActions.Remove(requester.ID);
                    Logger.Instance.LogMessage(TracingLevel.INFO, $"Removed {requester.ID} from Port {key}.");

                    if (port.ConnectedActions.Count > 0)
                    {
                        return;
                    }
                    Logger.Instance.LogMessage(TracingLevel.INFO, $"No other Actions connected. Closing {key}...");
                    port.SerialPort?.Close();
                    port.SerialPort = null;
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
                    port.SerialPort?.Close();
                    port.SerialPort = null;
                    Logger.Instance.LogMessage(TracingLevel.INFO, $"Closed {key}");
                }
            }
            Logger.Instance.LogMessage(TracingLevel.INFO, $"All ports closed.");
        }
    }
}
