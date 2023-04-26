using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeyDelight.SerialIRBlaster.Common
{
    internal abstract class SerialPortRequester
    {

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

        public SerialPortRequester(Guid ID)
        {
            this.ID = ID;
        }
    }
}
