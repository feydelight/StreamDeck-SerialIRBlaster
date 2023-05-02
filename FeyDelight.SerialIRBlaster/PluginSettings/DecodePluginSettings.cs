using FeyDelight.SerialIRBlaster.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeyDelight.SerialIRBlaster.PluginSettings
{
    internal class DecodePluginSettings : SerialPortRequester
    {

        [JsonProperty(PropertyName = "decodeTime")]
        public string DecodeTime { get; set; }

        public DecodePluginSettings() :
            base()
        {
            DecodeTime = "60";
        }
    }
}
