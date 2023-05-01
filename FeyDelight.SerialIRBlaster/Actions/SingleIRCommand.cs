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
using FeyDelight.SerialIRBlaster.Common;
using System.Security.Authentication;
using FeyDelight.SerialIRBlaster.Common.SerialMessages;
using System.Xml.Linq;

namespace FeyDelight.SerialIRBlaster.Actions
{
    [PluginActionId("com.feydelight.serialirblaster.singleircommand")]
    class SingleIRCommand : SerialIRBlasterBase
    {
        private class PluginSettings : SerialPortRequester
        {
            public PluginSettings(Guid ID)
                : base(ID)
            {

            }
            public static PluginSettings CreateDefaultSettings(Guid ID)
            {
                return new PluginSettings(ID)
                {
                    Protocol = ProtocolEnum.NEC.ToString(),
                    Address = null,
                    Command = null,
                };
            }

            [JsonProperty(PropertyName = "protocol")]
            public string Protocol { get; set; }

            [JsonProperty(PropertyName = "address")]
            public string Address { get; set; }

            [JsonProperty(PropertyName = "command")]
            public string Command { get; set; }

        }

        private readonly PluginSettings settings;
        private Guid ID { get; } = Guid.NewGuid();

        public SingleIRCommand(SDConnection connection, InitialPayload payload)
            : base(connection, payload)
        {
            if (payload.Settings == null || payload.Settings.Count == 0)
            {
                this.settings = PluginSettings.CreateDefaultSettings(ID);
                Connection.SetSettingsAsync(JObject.FromObject(settings));
            }
            else
            {
                this.settings = payload.Settings.ToObject<PluginSettings>();
                if (this.settings.ID == Guid.Empty)
                {
                    this.settings.ID = ID;
                    SaveSettings();
                }
                else
                {
                    this.ID = this.settings.ID;
                }
            }
            base.TryToGetPort(this.settings, SerialPort_DataReceived);
        }

        public async override void KeyPressed(KeyPayload payload)
        {
            Logger.Instance.LogMessage(TracingLevel.INFO, $"{this.GetType()} {nameof(KeyPressed)}");
            var serialPort = Program.SerialPortManager.GetSerialPort(settings, SerialPort_DataReceived);
            if (serialPort == null)
            {
                await Connection.ShowAlert();
                return;
            }


            IRBlastSerialMessage message;
            try
            {
                message = IRBlastSerialMessage.FromString(settings.Protocol, settings.Address, settings.Command, "0");
                if (message == null)
                {
                    throw new Exception();
                }
            }
            catch (Exception e)
            {
                Logger.Instance.LogMessage(TracingLevel.ERROR, $"Failed to get message out of settings");
                Logger.Instance.LogMessage(TracingLevel.ERROR, $"{e}");
                Logger.Instance.LogMessage(TracingLevel.ERROR, JsonConvert.SerializeObject(settings));
                await Connection.ShowAlert();
                return;
            }
            Logger.Instance.LogMessage(TracingLevel.INFO, $"Sending: {message}");
            serialPort.Write(message.GetPayload());
            await Connection.ShowOk();
        }

        public override void KeyReleased(KeyPayload payload)
        {
        }

        private void SerialPort_DataReceived(SerialPort sender, string line)
        {
            if (string.IsNullOrEmpty(line))
            {
                return;
            }
            // do a sanity check to see if its finished
        }

        public override void OnTick()
        {

        }

        public override void Dispose()
        {
            base.Dispose();
            Logger.Instance.LogMessage(TracingLevel.INFO, "Destructor called");
            Program.SerialPortManager.CloseSerialPort(settings);
        }

        public override async void ReceivedSettings(ReceivedSettingsPayload payload)
        {
            Logger.Instance.LogMessage(TracingLevel.INFO, $"{this.GetType()} {nameof(ReceivedSettings)}");
            Tools.AutoPopulateSettings(settings, payload.Settings);
            await SaveSettings();
        }

        public override void ReceivedGlobalSettings(ReceivedGlobalSettingsPayload payload)
        {
        }

        private Task SaveSettings()
        {
            return Connection.SetSettingsAsync(JObject.FromObject(settings));
        }
    }
}
