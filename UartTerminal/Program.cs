using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using System.Diagnostics.SymbolStore;
using System.Security.Cryptography.X509Certificates;
using System.Windows.Forms;
using System.IO;

namespace UartTerminal
{
    internal class Program
    {
        const int PORT_NAME_MAX_LEN = 18;
        const byte KEY_ETX = 0x03;
        static void PrintHeader(string[] ports)
        {
            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.WriteLine("***********************UART TERMINAL**************************");
            //Console.ForegroundColor= ConsoleColor.DarkCyan;
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("List Port:");
            //Console.ForegroundColor = ConsoleColor.White;
            Console.ForegroundColor = ConsoleColor.DarkCyan;



            int lineSize = 0;
            foreach (var item in ports)
            {
                lineSize += PORT_NAME_MAX_LEN;

                if (lineSize + PORT_NAME_MAX_LEN > Console.WindowWidth)
                {
                    Console.WriteLine();
                    lineSize = 0;
                }

                Console.Write(item.PadRight(PORT_NAME_MAX_LEN));
            }

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.White;
        }

        static void PrintFooter()
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("**************************************************************");
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.White;
        }

        static void PrintComTitle()
        {
            //Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.Write("$ COM     : ");
            //Console.ForegroundColor = ConsoleColor.White;
        }

        static void PrintBaudrateTitle()
        {
            //Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.Write("$ Baudrate: ");
            //Console.ForegroundColor = ConsoleColor.White;
        }

        static void ParserInput(string[] args, string[] ports, out string portname, out int baudrate)
        {
            portname = string.Empty;
            baudrate = 0;

            //args = new string[] { "COM1", "1"};

            //foreach (var item in args)
            //{
            //    Console.WriteLine(item);
            //}

            bool hasBaudrate = false;

            if (args.Length > 0)
            {
                portname = args[0];

                if (args.Length > 1)
                {
                    hasBaudrate = int.TryParse(args[1], out baudrate);

                    if (hasBaudrate)
                    {
                        if (baudrate <= 0)
                        {
                            hasBaudrate = false;
                        }
                    }
                }
            }

            //Console.Write($"portname={portname} baud={baudrate}");

            if (!ports.Contains(portname))
            {
                portname = string.Empty;
            }

            while (portname == string.Empty)
            {
                PrintComTitle();
                var portSelected = Console.ReadLine();

                if (ports.Contains(portSelected))
                {
                    portname = portSelected;
                }
                else
                {
                    Console.WriteLine($"Error: Port <{portSelected}> is not existed");
                }
            }

            while (!hasBaudrate)
            {
                PrintBaudrateTitle();
                var tmp = Console.ReadLine();

                hasBaudrate = int.TryParse(tmp, out baudrate);

                if (hasBaudrate && (baudrate <= 0))
                {
                    hasBaudrate = false;
                }

                if (!hasBaudrate)
                {
                    Console.WriteLine($"Error: Read input baudrate");
                }
            }
        }

        static void ConsoleBackSpace()
        {
            Console.Write("\b \b");
        }

        static void ProcessInput(Terminal terminal, ConsoleKeyInfo key)
        {
            switch (key.Key)
            {
                case ConsoleKey.Enter:
                    terminal.ProcessInput('\n');
                    break;
                case ConsoleKey.LeftArrow:
                    terminal.ProcessInput(new char[] { (char)ConsoleKey.Escape, '[', 'D' });
                    break;
                case ConsoleKey.UpArrow:
                    terminal.ProcessInput(new char[] { (char)ConsoleKey.Escape, '[', 'A' });
                    break;
                case ConsoleKey.RightArrow:
                    terminal.ProcessInput(new char[] { (char)ConsoleKey.Escape, '[', 'C' });
                    break;
                case ConsoleKey.DownArrow:
                    terminal.ProcessInput(new char[] { (char)ConsoleKey.Escape, '[', 'B' });
                    break;
                case ConsoleKey.Backspace:
                    terminal.ProcessInput(key.KeyChar);
                    Console.Write("\u001b[C");
                    break;

                default:
                    terminal.ProcessInput(key.KeyChar);
                    ConsoleBackSpace();
                    break;
            }
        }

        static void ProcessSendFile(Terminal terminal)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();

            var res = openFileDialog.ShowDialog();
            if (res == DialogResult.OK)
            {
                var content = File.ReadAllBytes(openFileDialog.FileName);
                terminal.ProcessInput(content);
            }
        }

        [STAThread]
        static void Main(string[] args)
        {
            //foreach (var item in args)
            //{
            //    Console.WriteLine(item);
            //}

            var ports = SerialPort.GetPortNames();

            PrintHeader(ports);
            ParserInput(args, ports, out string portname, out int baudrate);

            PrintFooter();


            Terminal terminal = new Terminal(portname, baudrate);

            Console.CancelKeyPress += delegate (object sender, ConsoleCancelEventArgs e) {
                e.Cancel = true;
                terminal.ProcessInput(KEY_ETX);
            };

            while (true)
            {
                var key = Console.ReadKey();

                System.Diagnostics.Debug.WriteLine(key.Key);

                if ((key.Modifiers & ConsoleModifiers.Control) != 0)
                {
                    System.Diagnostics.Debug.WriteLine("Ctrl+");

                    switch (key.Key)
                    {
                        case ConsoleKey.Oem6:
                            return;
                        case ConsoleKey.D:
                            ConsoleBackSpace();
                            ProcessSendFile(terminal);
                            break;
                        default:
                            break;
                    }

                    continue;
                }

                ProcessInput(terminal, key);

            }
        }
    }
}
