using FeyDelight.SerialIRBlaster.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeyDelight.SerialIRBlaster.PluginSettings
{
    internal class MultiIRPluginSettings : SerialPortRequester
    {

        [JsonProperty(PropertyName = "multiCommand")]
        public string MultiCommand { get; set; }

        public MultiIRPluginSettings()
            : base()
        {
            MultiCommand = "";
        }
    }
}
