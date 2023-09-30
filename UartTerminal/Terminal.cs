using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;

namespace UartTerminal
{
    public class Terminal
    {
        SerialPort ser;

        public Terminal(string portName, int baudrate)
        {
            this.ser = new SerialPort(portName, baudrate);
            this.ser.DataReceived += Ser_DataReceived;

            this.ser.Open();
            this.ser.DiscardInBuffer();
        }

        public void ProcessInput(char[] c)
        {
            this.ser.Write(c, 0, c.Length);
        }

        public void ProcessInput(byte[] c)
        {
            this.ser.Write(c, 0, c.Length);
        }

        public void ProcessInput(char c)
        {
            var b = new char[] { c };
            this.ser.Write(b, 0, 1);
        }

        public void ProcessInput(byte c)
        {
            var b = new byte[] { c };
            this.ser.Write(b, 0, 1);
        }

        private void Ser_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            byte[] data = new byte[this.ser.BytesToRead];
            int recvLen = this.ser.Read(data, 0, data.Length);

            var str = System.Text.Encoding.UTF8.GetString(data, 0, recvLen);
            Console.Write(str);
        }

    }
}
