using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using FeyDelight.SerialIRBlaster;
using FeyDelight.SerialIRBlaster.Common;
using System.IO.Ports;

namespace FeyDelight.SerialIRBlaster.UnitTests
{
    [TestClass]
    public class SerialNameAndID_unitTest
    {
        [TestMethod]
        public void TestGetSerialNameAndIds()
        {
            var ports = SerialPort.GetPortNames();
            var serials = SerialNameAndId.GetSerialNameAndIds();
            Assert.AreEqual(ports.Length, serials.Count, $"Expected the same number of serial ports to be discovered.");
        }
    }
}
