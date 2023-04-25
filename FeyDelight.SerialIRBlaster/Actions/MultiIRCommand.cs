﻿using BarRaider.SdTools;
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
    [PluginActionId("com.feydelight.serialirblaster.multiircommand")]
    internal class MultiIRCommand : KeypadBase
    {
        private class PluginSettings : SerialPortSettings
        {
            public PluginSettings(Guid ID)
                : base(ID)
            {

            }
            public static PluginSettings CreateDefaultSettings(Guid ID)
            {
                return new PluginSettings(ID)
                {
                    MultiCommand = "",
                };
            }

            [JsonProperty(PropertyName = "multiCommand")]
            public string MultiCommand { get; set; }

        }

        private readonly PluginSettings settings;
        private Guid ID { get; } = Guid.NewGuid();

        public MultiIRCommand(SDConnection connection, InitialPayload payload)
            : base(connection, payload)
        {
            Connection.StreamDeckConnection.OnPropertyInspectorDidAppear += StreamDeckConnection_OnPropertyInspectorDidAppear;
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

            Program.SerialPortManager.GetSerialPort(this.settings, SerialPort_DataReceived);
        }

        private async void StreamDeckConnection_OnPropertyInspectorDidAppear(object sender, SDEventReceivedEventArgs<PropertyInspectorDidAppearEvent> e)
        {
            var serials = SerialNameAndId.GetSerialNameAndIds();
            await Connection.SendToPropertyInspectorAsync(JObject.FromObject(new
            {
                serials
            }));
        }

        public override void KeyPressed(KeyPayload payload)
        {
            Logger.Instance.LogMessage(TracingLevel.INFO, $"{this.GetType()} {nameof(KeyPressed)}");
            var serialPort = Program.SerialPortManager.GetSerialPort(settings, SerialPort_DataReceived);
            if (serialPort == null)
            {
                return;
            }

            List<IRBlastSerialMessage> messages = new List<IRBlastSerialMessage>();

            string[] splitCommands = this.settings.MultiCommand?.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            if (splitCommands.Length == 0)
            {
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
            Logger.Instance.LogMessage(TracingLevel.INFO, $"{this.GetType()} {nameof(KeyReleased)}");
        }

        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort sp = (SerialPort)sender;
            try
            {
                string indata = sp.ReadLine().Trim();
                Logger.Instance.LogMessage(TracingLevel.INFO, $"Serial replied: {indata}");
            }
            catch (Exception)
            {
                // either timed out, or the port got closed. either way, no biggy.
                return;
            }
        }

        public override void OnTick()
        {

        }

        public override void Dispose()
        {
            Logger.Instance.LogMessage(TracingLevel.INFO, "Destructor called");
            Connection.StreamDeckConnection.OnPropertyInspectorDidAppear -= StreamDeckConnection_OnPropertyInspectorDidAppear;
            Program.SerialPortManager.CloseSerialPort(settings, SerialPort_DataReceived);
        }

        public override async void ReceivedSettings(ReceivedSettingsPayload payload)
        {
            Logger.Instance.LogMessage(TracingLevel.INFO, $"{this.GetType()} {nameof(ReceivedSettings)}");
            bool reOpen = false;
            if (settings.IsEqualTo(payload.Settings) == false)
            {
                Logger.Instance.LogMessage(TracingLevel.INFO, $"ComPort Changed. re-opening");
                reOpen = true;
                Program.SerialPortManager.CloseSerialPort(settings, SerialPort_DataReceived);
            }
            Tools.AutoPopulateSettings(settings, payload.Settings);
            if (reOpen)
            {
                Program.SerialPortManager.GetSerialPort(settings, SerialPort_DataReceived);
            }
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
