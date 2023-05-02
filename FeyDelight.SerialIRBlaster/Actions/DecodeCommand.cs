using BarRaider.SdTools;
using BarRaider.SdTools.Wrappers;
using BarRaider.SdTools.Communication.SDEvents;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;
using System.IO.Ports;
using System.Management;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using FeyDelight.SerialIRBlaster.Common;
using System.Security.Authentication;
using FeyDelight.SerialIRBlaster.Common.SerialMessages;
using System.Xml.Linq;
using System.Text.RegularExpressions;
using System.Drawing;
using FeyDelight.SerialIRBlaster.PluginSettings;

namespace FeyDelight.SerialIRBlaster.Actions
{

    [PluginActionId("com.feydelight.serialirblaster.decodecommand")]
    class DecodeCommand : SerialIRBlasterBase<DecodePluginSettings>
    {
        protected override DecodePluginSettings Settings { get; set; }        

        private int decodeTime;

        public DecodeCommand(SDConnection connection, InitialPayload payload)
            : base(connection, payload)
        {
            if (int.TryParse(Settings.DecodeTime, out decodeTime) == false)
            {
                decodeTime = 60;
                Settings.DecodeTime = decodeTime.ToString();
            }
            SaveSettings();
            base.TryToGetPort();
        }

        DateTimeOffset endOfChecking { get; set; }
        bool removeTrigger = false;
        public async override void KeyPressed(KeyPayload payload)
        {
            Logger.Instance.LogMessage(TracingLevel.INFO, $"{this.GetType()} {nameof(KeyPressed)}");
            var serialPort = Program.SerialPortManager.GetSerialPort(Settings);
            if (serialPort == null)
            {
                Logger.Instance.LogMessage(TracingLevel.INFO, $"Failed to get the serial port");
                await Connection.ShowAlert();
                return;
            }


            SettingsSerialMessage message = new SettingsSerialMessage(true, false, true);
            Logger.Instance.LogMessage(TracingLevel.INFO, $"Sending: {message}");
            serialPort.DataReceived += SerialPort_DataReceived;
            serialPort.Write(message.GetPayload());
            endOfChecking = DateTimeOffset.Now.AddSeconds(decodeTime);
            removeTrigger = true;
            Logger.Instance.LogMessage(TracingLevel.INFO, $"Checking for decoded messages till {endOfChecking}. Currently {DateTimeOffset.Now}");
            await Connection.ShowOk();
        }

        public override void KeyReleased(KeyPayload payload)
        {
        }

        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort sp = (SerialPort)sender;
            try
            {
                string indata = sp.ReadLine().Trim();
                Logger.Instance.LogMessage(TracingLevel.INFO, $"[Decoder] Serial replied: {indata}");
                if (DateTimeOffset.Now < endOfChecking)
                {
                    TryParseReturnData(indata);
                }
            }
            catch (Exception)
            {
                // either timed out, or the port got closed. either way, no biggy.
                return;
            }
        }
        private void TryParseReturnData(string line)
        {
            /*
1DE2FF02
Protocol=NEC Address=0xFF02 Command=0xE2 Raw-Data=0x1DE2FF02 32 bits LSB first
Send with: IrSender.sendNEC(0xFF02, 0xE2, <numberOfRepeats>);
            */
            Regex reg = new Regex(@"Protocol=(.*) Address=(.*) Command=(.*) Raw-Data=", RegexOptions.IgnoreCase);
            Match match = reg.Match(line);
            if (match.Success && match.Groups.Count >= 4)
            {
                string protocol = match.Groups[1].Value;
                string address = match.Groups[2].Value;
                string command = match.Groups[3].Value;
                Logger.Instance.LogMessage(TracingLevel.INFO, $"Found match. Protocol: '{protocol}'; Address: '{address}'; Command: '{command}'");
                DrawText(protocol, address, command);
            }
            else
            {
                Logger.Instance.LogMessage(TracingLevel.INFO, $"did not find match in: {line}");
                Logger.Instance.LogMessage(TracingLevel.INFO, $"success {match.Success}; Groups count: {match.Groups.Count}");
            }
        }

        private async void DrawText(string protocol, string address, string command)
        {
            const int STARTING_TEXT_Y = 3;
            const int BUFFER_Y = 16;
            address = $"Addr: {address}";
            command = $"Cmd: {command}";
            try
            {
                using (Bitmap bmp = Tools.GenerateGenericKeyImage(out Graphics graphics))
                {
                    int height = bmp.Height;
                    int width = bmp.Width;

                    var fontDefault = new Font("Verdana", 28, FontStyle.Bold, GraphicsUnit.Pixel);

                    var bgBrush = new SolidBrush(Color.Black);
                    graphics.FillRectangle(bgBrush, new Rectangle(0, 0, width, height));

                    var fgBrush = new SolidBrush(Color.White);

                    float stringHeight = STARTING_TEXT_Y;
                    float stringWidth = graphics.GetTextCenter(protocol, width, fontDefault);
                    stringHeight = graphics.DrawAndMeasureString(protocol, fontDefault, fgBrush,
                        new PointF(stringWidth, stringHeight)) + BUFFER_Y;
                    float fontSize = graphics.GetFontSizeWhereTextFitsImage(address, width, fontDefault, 8);
                    var fontOther = new Font(fontDefault.Name, fontSize, fontDefault.Style, GraphicsUnit.Pixel);
                    stringWidth = graphics.GetTextCenter(address, width, fontOther);

                    stringHeight = graphics.DrawAndMeasureString(address, fontOther, fgBrush, new PointF(stringWidth, stringHeight));

                    stringWidth = graphics.GetTextCenter(command, width, fontOther);
                    graphics.DrawAndMeasureString(command, fontOther, fgBrush, new PointF(stringWidth, stringHeight));
                    await Connection.SetImageAsync(bmp);
                    graphics.Dispose();
                    fontDefault.Dispose();
                    fontOther.Dispose();
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.LogMessage(TracingLevel.ERROR, $"{nameof(DrawText)} Error drawing decoded data {ex}");
            }
        }

        public override void OnTick()
        {
            if (removeTrigger && DateTimeOffset.Now > endOfChecking)
            {
                removeTrigger = false;
                var serialPort = Program.SerialPortManager.GetSerialPort(Settings);
                if (serialPort != null)
                {
                    serialPort.DataReceived -= SerialPort_DataReceived;
                }
            }
        }

        public override void Dispose()
        {
            base.Dispose();
            Logger.Instance.LogMessage(TracingLevel.INFO, "Destructor called");
            var serialPort = Program.SerialPortManager.GetSerialPort(Settings);
            if (serialPort != null)
            {
                serialPort.DataReceived -= SerialPort_DataReceived;
            }
            Program.SerialPortManager.CloseSerialPort(Settings);
        }

        public override async void ReceivedSettings(ReceivedSettingsPayload payload)
        {
            base.ReceivedSettings(payload);
            if (int.TryParse(Settings.DecodeTime, out decodeTime) == false)
            {
                decodeTime = 60;
            }
            await SaveSettings();
        }

    }
}
