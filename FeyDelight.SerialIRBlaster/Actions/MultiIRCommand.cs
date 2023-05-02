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
    [PluginActionId("com.feydelight.serialirblaster.multiircommand")]
    internal class MultiIRCommand : SerialIRBlasterBase<MultiIRPluginSettings>
    {

        protected override MultiIRPluginSettings Settings { get; set; }


        public MultiIRCommand(SDConnection connection, InitialPayload payload)
            : base(connection, payload)
        {
            SaveSettings();
            base.TryToGetPort(SerialPort_DataReceived);
        }

        public override async void KeyPressed(KeyPayload payload)
        {
            Logger.Instance.LogMessage(TracingLevel.INFO, $"{this.GetType()} {nameof(KeyPressed)}");
            var serialPort = Program.SerialPortManager.GetSerialPort(Settings, SerialPort_DataReceived);
            if (serialPort == null)
            {
                await Connection.ShowAlert();
                return;
            }

            List<IRBlastSerialMessage> messages = new List<IRBlastSerialMessage>();

            string[] splitCommands = this.Settings.MultiCommand?.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            if (splitCommands.Length == 0)
            {
                await Connection.ShowAlert();
                return;
            }
            foreach (string command in splitCommands)
            {
                try
                {
                    string[] splitOp = command.Trim().Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    if (splitOp.Length != 4)
                    {
                        throw new InvalidOperationException("Invalid command.");
                    }
                    string Delay = splitOp[0].Trim();
                    string Protocol = splitOp[1].Trim();
                    string Address = splitOp[2].Trim();
                    string Command = splitOp[3].Trim();
                    IRBlastSerialMessage message = IRBlastSerialMessage.FromString(Protocol, Address, Command, Delay) ??
                        throw new InvalidOperationException("Invalid command.");
                    Logger.Instance.LogMessage(TracingLevel.INFO, $"Sending: {message}");
                    serialPort.Write(message.GetPayload());
                }
                catch (Exception ex)
                {
                    Logger.Instance.LogMessage(TracingLevel.INFO, $"Invalid command: '{command}'. Expected '[delay],[Protocol],[Address],[Command]'");
                    Logger.Instance.LogMessage(TracingLevel.ERROR, $"{ex}");
                    continue;
                }
            }
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
