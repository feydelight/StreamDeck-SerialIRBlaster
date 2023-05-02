using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeyDelight.SerialIRBlaster.Common
{
    internal abstract class SerialPortRequester : ISerialPortRequester
    {

        [JsonProperty(PropertyName = "serials")]
        public List<SerialPortSettings> Serials { get; set; }

        [JsonProperty(PropertyName = "key")]
        public string Key { get; set; }

        [JsonProperty(PropertyName = "id")]
        public string id { get; set; }
        public Guid ID
        {
            get
            {
                return Guid.Parse(this.id);
            }
            set
            {
                id = value.ToString();
            }
        }

        public SerialPortRequester()
        {
            ID = Guid.NewGuid();
        }

    }
}
