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
    [PluginActionId("com.feydelight.serialirblaster.disconnectall")]
    class DisconnectAll : SerialIRBlasterBase<DisconnectAllPluginSettings>
    {
        protected override DisconnectAllPluginSettings Settings { get; set; }

        public DisconnectAll(SDConnection connection, InitialPayload payload)
            : base(connection, payload)
        {
            SaveSettings();            
        }


        public override void KeyPressed(KeyPayload payload)
        {
            Logger.Instance.LogMessage(TracingLevel.INFO, $"{this.GetType()} {nameof(KeyPressed)}");
            Program.SerialPortManager.CloseAllSerialPorts();
        }

        public override void KeyReleased(KeyPayload payload)
        {
        }

        public override void OnTick()
        {
        }

         
    }
}
