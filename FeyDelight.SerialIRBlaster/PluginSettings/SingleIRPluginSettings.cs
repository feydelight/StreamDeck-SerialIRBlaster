using FeyDelight.SerialIRBlaster.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeyDelight.SerialIRBlaster.PluginSettings
{
    internal class SingleIRPluginSettings : SerialPortRequester
    {
        [JsonProperty(PropertyName = "protocol")]
        public string Protocol { get; set; }

        [JsonProperty(PropertyName = "address")]
        public string Address { get; set; }

        [JsonProperty(PropertyName = "command")]
        public string Command { get; set; }
        public SingleIRPluginSettings()
            : base()
        {

            Protocol = ProtocolEnum.NEC.ToString();
            Address = null;
            Command = null;
        }

    }
}
