using BarRaider.SdTools;
using BarRaider.SdTools.Wrappers;
using BarRaider.SdTools.Communication.SDEvents;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;
using System.IO.Ports;
using System.Management;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using System.Security.Authentication;
using System.Xml.Linq;
using BarRaider.SdTools.Events;
using FeyDelight.SerialIRBlaster.Common;

namespace FeyDelight.SerialIRBlaster.Actions
{
    internal abstract class SerialIRBlasterBase : KeypadBase
    {
        public class GlobalSettings
        {
            [JsonProperty(PropertyName = "serialPortSettings")]
            public Dictionary<string, SerialPortSettings> SerialPortSettings { get; set; }
        }
        protected GlobalSettings globalSettings;

        public SerialIRBlasterBase(SDConnection connection, InitialPayload payload)
            : base(connection, payload)
        {
            Connection.StreamDeckConnection.OnPropertyInspectorDidAppear += StreamDeckConnection_OnPropertyInspectorDidAppear;
            Connection.OnSendToPlugin += Connection_OnSendToPlugin;
            GlobalSettingsManager.Instance.OnReceivedGlobalSettings += Instance_OnReceivedGlobalSettings;
            Connection.GetGlobalSettingsAsync();
        }

        private async void Instance_OnReceivedGlobalSettings(object sender, ReceivedGlobalSettingsPayload payload)
        {
            Logger.Instance.LogMessage(TracingLevel.INFO, $"{this.GetType()}.{nameof(Instance_OnReceivedGlobalSettings)}");
            // existing settings
            if (payload?.Settings != null && payload.Settings.Count > 0)
            {
                Logger.Instance.LogMessage(TracingLevel.INFO, $"previous setting available, retrieving...");
                globalSettings = payload.Settings.ToObject<GlobalSettings>();
                if (globalSettings.SerialPortSettings == null)
                {
                    globalSettings.SerialPortSettings = new Dictionary<string, SerialPortSettings>();
                }
                Logger.Instance.LogMessage(TracingLevel.INFO, $"retrieved.");
            } 
            else
            {
                Logger.Instance.LogMessage(TracingLevel.INFO, $"no settings available, creating one...");
                globalSettings = new GlobalSettings()
                {
                    SerialPortSettings = new Dictionary<string, SerialPortSettings>(),
                };
                await SetGlobalSettings();
                Logger.Instance.LogMessage(TracingLevel.INFO, $"done.");
            }


            Logger.Instance.LogMessage(TracingLevel.INFO, $"creating all sockets");
            if (globalSettings.SerialPortSettings.Count > 0)
            {
                foreach (var setting in globalSettings.SerialPortSettings)
                {
                    Program.SerialPortManager.AddSerialPort(setting.Value);
                }
            }

            Logger.Instance.LogMessage(TracingLevel.INFO, $"done.");

        }

        private async void Connection_OnSendToPlugin(object sender, SDEventReceivedEventArgs<SendToPlugin> e)
        {
            Logger.Instance.LogMessage(TracingLevel.INFO, $"{this.GetType()}.{nameof(Connection_OnSendToPlugin)}");
            var payload = e.Event.Payload;
            Logger.Instance.LogMessage(TracingLevel.INFO, $"payload available...");
            if (payload["property_inspector"] != null)
            {
                Logger.Instance.LogMessage(TracingLevel.INFO, $"getting action...");
                var action = payload["property_inspector"].ToString().ToLower();
                Logger.Instance.LogMessage(TracingLevel.INFO, $"action {action}");
                switch (action)
                {
                    case "socket-add":
                        var settings = SerialPortSettings.FromJObject(payload);
                        Logger.Instance.LogMessage(TracingLevel.INFO, $"adding setting...");
                        if (Program.SerialPortManager.AddSerialPort(settings))
                        {
                            Logger.Instance.LogMessage(TracingLevel.INFO, $"New setting {settings.ComPort}");
                            globalSettings.SerialPortSettings.Add(settings.ComPort, settings);
                            await SetGlobalSettings();
                        } 
                        else
                        {
                            Logger.Instance.LogMessage(TracingLevel.INFO, $"existing setting {settings.ComPort}. closing.");
                        }
                        break;
                    case "socket-remove":
                        var key = payload["key"].ToString();
                        Logger.Instance.LogMessage(TracingLevel.INFO, $"Attempting to remove {key}");
                        if (globalSettings.SerialPortSettings.Remove(key))
                        {
                            Logger.Instance.LogMessage(TracingLevel.INFO, $"successfully removed {key}");
                            Program.SerialPortManager.RemoveSerialPort(key);
                        } 
                        else
                        {
                            Logger.Instance.LogMessage(TracingLevel.INFO, $"{key} did not exist");
                        }
                        await SetGlobalSettings();
                        break;
                }
                SendDataToPropertyInspector();
            }
        }

        private void StreamDeckConnection_OnPropertyInspectorDidAppear(object sender, SDEventReceivedEventArgs<PropertyInspectorDidAppearEvent> e)
        {
            SendDataToPropertyInspector();
        }

        private async void SendDataToPropertyInspector()
        {
            var serials = SerialNameAndId.GetSerialNameAndIds();
            await Connection.SendToPropertyInspectorAsync(JObject.FromObject(new
            {
                serials,
                globalSettings
            }));

        }

        public override void Dispose()
        {
            Connection.StreamDeckConnection.OnPropertyInspectorDidAppear -= StreamDeckConnection_OnPropertyInspectorDidAppear;
            Connection.OnSendToPlugin -= Connection_OnSendToPlugin;
            GlobalSettingsManager.Instance.OnReceivedGlobalSettings -= Instance_OnReceivedGlobalSettings;
        }

        private Task SetGlobalSettings()
        {

            Logger.Instance.LogMessage(TracingLevel.INFO, $"Creating JObject");
            var obj = JObject.FromObject(globalSettings);
            Logger.Instance.LogMessage(TracingLevel.INFO, $"created: {obj}");
            Logger.Instance.LogMessage(TracingLevel.INFO, $"saving...");
            return Connection.SetGlobalSettingsAsync(obj);
        }

        int triedToGetPort = 0;
        protected async void TryToGetPort(SerialPortRequester requester, ReplyDelegate replyDelegate)
        {
            await Task.Delay(500);
            var port = Program.SerialPortManager.GetSerialPort(requester, replyDelegate);
            if (port == null && triedToGetPort < 10)
            {
                ++triedToGetPort;
                TryToGetPort(requester, replyDelegate);
            }
        }
    }
}
