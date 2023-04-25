using BarRaider.SdTools;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;

namespace FeyDelight.SerialIRBlaster.Common
{
    public class SerialNameAndId
    {
        [JsonProperty(PropertyName = "desc")]
        public string Description { get; set; }

        [JsonProperty(PropertyName = "id")]
        public string DeviceID { get; set; }

        public SerialNameAndId(string description, string deviceID)
        {
            Description = description;
            DeviceID = deviceID;
        }

        public static List<SerialNameAndId> GetSerialNameAndIds()
        {
            ManagementScope connectionScope = new ManagementScope();
            SelectQuery serialQuery = new SelectQuery("SELECT * FROM Win32_SerialPort");
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(connectionScope, serialQuery);

            List<SerialNameAndId> serials = new List<SerialNameAndId>();
            try
            {
                foreach (ManagementObject item in searcher.Get().Cast<ManagementObject>())
                {
                    string desc = item["Description"].ToString();
                    string deviceId = item["DeviceID"].ToString();
                    serials.Add(new SerialNameAndId(desc, deviceId));
                }
            }
            catch (ManagementException)
            {
            }

            if (serials.Count == 0)
            {
                string[] ports = SerialPort.GetPortNames();
                foreach (string port in ports)
                {
                    serials.Add(new SerialNameAndId(port, port));
                }
            }

            serials.Sort(new SerialNameAndIdComparer());

            Logger.Instance.LogMessage(TracingLevel.INFO, "Found the following Serials: ");
            Logger.Instance.LogMessage(TracingLevel.INFO, JsonConvert.SerializeObject(serials));

            return serials;
        }

        public override string ToString()
        {
            return $"Description: {Description}, DeviceId: {DeviceID}";
        }
    }
}
