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
using FeyDelight.SerialIRBlaster.PluginSettings;

namespace FeyDelight.SerialIRBlaster.Actions
{
    [PluginActionId("com.feydelight.serialirblaster.singleircommand")]
    class SingleIRCommand : SerialIRBlasterBase<SingleIRPluginSettings>
    {

        protected override SingleIRPluginSettings Settings { get; set; }
        
        public SingleIRCommand(SDConnection connection, InitialPayload payload)
            : base(connection, payload)
        {
            SaveSettings();
            base.TryToGetPort(SerialPort_DataReceived);
        }

        public async override void KeyPressed(KeyPayload payload)
        {
            Logger.Instance.LogMessage(TracingLevel.INFO, $"{this.GetType()} {nameof(KeyPressed)}");
            var serialPort = Program.SerialPortManager.GetSerialPort(Settings, SerialPort_DataReceived);
            if (serialPort == null)
            {
                await Connection.ShowAlert();
                return;
            }


            IRBlastSerialMessage message;
            try
            {
                message = IRBlastSerialMessage.FromString(Settings.Protocol, Settings.Address, Settings.Command, "0");
                if (message == null)
                {
                    throw new Exception();
                }
            }
            catch (Exception e)
            {
                Logger.Instance.LogMessage(TracingLevel.ERROR, $"Failed to get message out of settings");
                Logger.Instance.LogMessage(TracingLevel.ERROR, $"{e}");
                Logger.Instance.LogMessage(TracingLevel.ERROR, JsonConvert.SerializeObject(Settings));
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
            Program.SerialPortManager.CloseSerialPort(Settings);
        }

        public override async void ReceivedSettings(ReceivedSettingsPayload payload)
        {
            base.ReceivedSettings(payload);
            await SaveSettings();
        }


    }
}
