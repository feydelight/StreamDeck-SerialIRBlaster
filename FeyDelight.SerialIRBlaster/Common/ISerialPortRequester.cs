using System;
using System.Collections.Generic;

namespace FeyDelight.SerialIRBlaster.Common
{
    internal interface ISerialPortRequester
    {
        string id { get; set; }
        Guid ID { get; set; }
        string Key { get; set; }
        List<SerialPortSettings> Serials { get; set; }
    }
}