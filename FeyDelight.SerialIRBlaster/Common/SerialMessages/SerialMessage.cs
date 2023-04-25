using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeyDelight.SerialIRBlaster.Common.SerialMessages
{
    internal abstract class SerialMessage : ISerialMessage
    {
        public string MessageType { get; }
        public SerialMessage(string MessageType) {
            this.MessageType = MessageType;
        }
        public string GetPayload(string payload)
        {
            return $"{this.MessageType}:{payload};";
        }
        public abstract string GetPayload();
    }
}
