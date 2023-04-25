using System;

namespace FeyDelight.SerialIRBlaster.Common.SerialMessages
{
    internal class IRBlastSerialMessage: SerialMessage
    {
        public ProtocolEnum Protocol { get; }

        public int Address { get; }

        public int Command { get; }

        public int Delay { get; }

        public IRBlastSerialMessage(ProtocolEnum Protocol, int Address, int Command, int Delay)
            : base("IRCommand")
        {
            this.Protocol = Protocol;
            this.Address = Address;
            this.Command = Command;
            this.Delay = Delay;
        }

        public static IRBlastSerialMessage FromString(string Protocol, string AddressHex, string CommandHex, string Delay)
        {
            if (Enum.TryParse(Protocol, out ProtocolEnum protocol) == false) {
                throw new ArgumentException($"Unabled to parse into {nameof(ProtocolEnum)}: {Protocol}", nameof(Protocol));
            }
            char[] _trim_hex = new char[] { '0', 'x' };
            if (int.TryParse(AddressHex?.TrimStart(_trim_hex),
                System.Globalization.NumberStyles.HexNumber, null, out int address) == false)
            {
                throw new ArgumentException($"Unabled to parse into int: {AddressHex}", nameof(AddressHex));
            }
            if (int.TryParse(CommandHex?.TrimStart(_trim_hex),
                System.Globalization.NumberStyles.HexNumber, null,  out int command) == false)
            {
                throw new ArgumentException($"Unabled to parse into int: {CommandHex}", nameof(CommandHex));
            }
            if (int.TryParse(Delay, out int delay) == false)
            {
                throw new ArgumentException($"Unabled to parse into int: {Delay}", nameof(Delay));
            }
            var SerialMessage = new IRBlastSerialMessage(protocol, address, command, delay);
            return SerialMessage;
        }

        public override string GetPayload()
        {
            return base.GetPayload($"{Delay},{Protocol},{Address},{Command}");
        }

        public override string ToString()
        {
            return $"{nameof(Delay)}: {Delay}, {nameof(Protocol)}: {Protocol}, {nameof(Address)}: {Address}, {nameof(Command)}: {Command}";
        }
    }
}
