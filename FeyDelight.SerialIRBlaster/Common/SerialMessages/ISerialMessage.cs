using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeyDelight.SerialIRBlaster.Common.SerialMessages
{
    internal interface ISerialMessage
    {
        string MessageType { get; }
        string GetPayload();
    }
}
