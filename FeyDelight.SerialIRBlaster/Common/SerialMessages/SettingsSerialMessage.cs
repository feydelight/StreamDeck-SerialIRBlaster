using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeyDelight.SerialIRBlaster.Common.SerialMessages
{
    internal class SettingsSerialMessage: SerialMessage
    {
        public bool DecodeSignals { get; }
        public bool DebugMode { get; }
        public bool Reply { get; }

        public SettingsSerialMessage(bool DecodeSignals, bool DebugMode, bool Reply)
            : base("Settings")
        {
            this.DecodeSignals = DecodeSignals;
            this.DebugMode = DebugMode;
            this.Reply = Reply;  
        }

        public override string GetPayload()
        {
            return base.GetPayload($"{DecodeSignals},{DebugMode},{Reply}");
        }

        public override string ToString()
        {
            return $"{nameof(DecodeSignals)}:{DecodeSignals}, {nameof(DebugMode)}: {DebugMode}, {nameof(Reply)}: {Reply}";
        }
    }
}
