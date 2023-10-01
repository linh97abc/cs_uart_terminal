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
        bool isHexFormatEn = false;

        public bool IsHexFormatEn
        {
            get { return isHexFormatEn; }
            set { isHexFormatEn = value; }
        }

        public static readonly char[] KEY_LEFT = new char[] { (char)ConsoleKey.Escape, '[', 'D' };
        public static readonly char[] KEY_UP = new char[] { (char)ConsoleKey.Escape, '[', 'A' };
        public static readonly char[] KEY_RIGHT = new char[] { (char)ConsoleKey.Escape, '[', 'C' };
        public static readonly char[] KEY_DOWN = new char[] { (char)ConsoleKey.Escape, '[', 'B' };
        public static readonly byte KEY_ETX = 0x03;
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

        public void ProcessInput(string s)
        {
            this.ser.Write(s);
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

        public void ProcessInput(ConsoleKeyInfo key)
        {
            char[] sendData;
            switch (key.Key)
            {
                case ConsoleKey.Enter:
                    sendData = new char[] { '\n' };
                    //if (isEcho)
                    //{
                    //    Console.Write(sendData);
                    //}
                    break;
                case ConsoleKey.LeftArrow:
                    sendData = KEY_LEFT;
                    break;
                case ConsoleKey.UpArrow:
                    sendData = KEY_UP;
                    break;
                case ConsoleKey.RightArrow:
                    sendData = KEY_RIGHT;
                    break;
                case ConsoleKey.DownArrow:
                    sendData = KEY_DOWN;
                    break;
                //case ConsoleKey.Backspace:
                //    sendData = new char[] { key.KeyChar };
                //    break;

                default:
                    sendData = new char[] { key.KeyChar };
                    //if (isEcho)
                    //{
                    //    Console.Write(sendData);
                    //}
                    break;
            }

            this.ProcessInput(sendData);
        }

        private void Ser_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            byte[] data = new byte[this.ser.BytesToRead];
            int recvLen = this.ser.Read(data, 0, data.Length);

            if (this.isHexFormatEn)
            {
                for (int i = 0; i < recvLen; i++)
                {
                    Console.Write($"{data[i]:X2} ");
                }
            }
            else
            {
                var str = System.Text.Encoding.UTF8.GetString(data, 0, recvLen);
                Console.Write(str);
                
            }

        }

        public bool IsSendBufferEmpty()
        {
            return this.ser.BytesToWrite == 0;
        }
    }
}
