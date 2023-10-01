using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UartTerminal
{
    public class AppManager
    {
        const int PORT_NAME_MAX_LEN = 18;
        string[] ports;
        string portname;
        int baudrate;

        Terminal terminal;
        bool isEcho = false;
        bool isSendStopReq = true;

        ConsoleColor foreGroundColor = ConsoleColor.White;

        public static AppManager Instance { get; private set; } = new AppManager();

        void PrintHeader()
        {
            Console.ResetColor();
            ConsolePrintBanner("********************* UART TERMINAL v1.0 ***********************");
            ConsolePrintBanner("--- Available ports:");
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
            Console.ForegroundColor = this.foreGroundColor;
        }

        void PrintFooter()
        {
            ConsolePrintBanner($"--- UART Terminal on {portname} {baudrate},8,N,1---");
            ConsolePrintBanner("--- Quit: Ctrl+] | Help: Ctrl+H ---");
            Console.WriteLine();

        }

        void PrintHelp()
        {
            ConsolePrintInfo("\n--- UART Terminal (1.0) - help\n" +
                               "---\n" +
                               "--- Command:\n" +
                               "--- Ctrl+]  Exit program\n" +
                               "--- Ctrl+I  Show info\n" +
                               "--- Ctrl+U  Upload file\n" +
                               "--- Toggles:\n" +
                               "--- Ctrl+A  Data format Hex/Ascii\n" +
                               "--- Ctrl+E  Local echo on/off\n"
                               );
        }

        void PrintComTitle()
        {
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.Write("--- COM     : ");
            Console.ForegroundColor = ConsoleColor.White;
        }

        void PrintBaudrateTitle()
        {
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.Write("--- Baudrate: ");
            Console.ForegroundColor = ConsoleColor.White;
        }

        void PrintInfo()
        {
            string dataFormat = terminal.IsHexFormatEn ? "Hex" : "Ascii";
            string echoStt = this.isEcho ? "On" : "Off";
            ConsolePrintInfo($"\n--- Settings   : {portname} {baudrate}, 8, N, 1" +
                             $"\n--- Data format: {dataFormat}" +
                             $"\n--- Local echo : {echoStt}"
                             );
        }

        void ParserInput(string[] args)
        {
            portname = string.Empty;
            baudrate = 0;

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
                    ConsolePrintErr($"Error: Port <{portSelected}> is not existed");
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
                    ConsolePrintErr("Error: Read input baudrate");
                }
            }
        }

        void ConsolePrintErr(string msg)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(msg);
            Console.ForegroundColor = this.foreGroundColor;
        }

        void ConsolePrintInfo(string msg)
        {
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine(msg);
            Console.ForegroundColor = this.foreGroundColor;
        }

        void ConsolePrintBanner(string msg)
        {
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.WriteLine(msg);
            Console.ForegroundColor = this.foreGroundColor;
        }

        void ConsoleBackSpace()
        {
            Console.Write("\b \b");
        }

        void ProcessSendFile(Terminal terminal)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();

            var res = openFileDialog.ShowDialog();
            if (res == DialogResult.OK)
            {
                var content = File.ReadAllBytes(openFileDialog.FileName);
                terminal.ProcessInput(content);

                int progress = 0;
                this.isSendStopReq = false;
                while (!terminal.IsSendBufferEmpty())
                {
                    if (this.isSendStopReq)
                    {
                        ConsolePrintInfo("\n--- Cancel upload file ---");
                        break;
                    }

                    Task.Delay(100).Wait();

                    if (progress == 0)
                    {
                        ConsolePrintInfo($"\n--- Sending file {openFileDialog.FileName} ---");
                    }
                    else if (progress % 10 == 0)
                    {
                        Console.Write('#');
                    }

                    progress++;
                }
                

                ConsolePrintInfo($"\n--- File {openFileDialog.FileName} sent ---");
            }
            else
            {
                ConsolePrintInfo("\n--- Cancel upload file ---");
            }

        }

        void OnEchoToggle()
        {
            this.isEcho = !this.isEcho;
            string stt = this.isEcho ? "on" : "off";
            ConsolePrintInfo($"\n--- local echo {stt} ---");
        }

        void OnAsciiFormatChanged()
        {
            terminal.IsHexFormatEn = !terminal.IsHexFormatEn;

            string stt = terminal.IsHexFormatEn ? "Hex" : "Ascii";
            ConsolePrintInfo($"\n--- Data format {stt} ---");

            if (terminal.IsHexFormatEn)
            {
                this.foreGroundColor = ConsoleColor.DarkYellow;
                Console.ForegroundColor = this.foreGroundColor;
            }
            else
            {
                this.foreGroundColor = ConsoleColor.White;
                Console.ForegroundColor = this.foreGroundColor;
            }

        }
        AppManager()
        {
            ports = SerialPort.GetPortNames();
        }

        void ProcessEcho(ConsoleKeyInfo key)
        {
            switch (key.Key)
            {
                case ConsoleKey.Enter:
                    Console.Write("\n");
                    break;
                case ConsoleKey.LeftArrow:
                case ConsoleKey.UpArrow:
                case ConsoleKey.RightArrow:
                case ConsoleKey.DownArrow:
                case ConsoleKey.Backspace:
                    break;

                default:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Write(key.KeyChar);
                    Console.ForegroundColor = this.foreGroundColor;
                    break;
            }
        }

        public int Run(string[] args)
        {
            PrintHeader();
            ParserInput(args);
            PrintFooter();

            terminal = new Terminal(portname, baudrate);

            Console.CancelKeyPress += delegate (object sender, ConsoleCancelEventArgs e) {
                e.Cancel = true;

                if (this.isSendStopReq)
                {
                    terminal.ProcessInput(Terminal.KEY_ETX);
                    
                }
                else
                {
                    this.isSendStopReq = true;
                    
                }
            };

            while (true)
            {
                var key = Console.ReadKey(true);

                System.Diagnostics.Debug.WriteLine(key.Key);

                if ((key.Modifiers & ConsoleModifiers.Control) != 0)
                {
                    System.Diagnostics.Debug.WriteLine("Ctrl+");

                    switch (key.Key)
                    {
                        case ConsoleKey.Oem6:
                            return 0;
                        case ConsoleKey.A:
                            OnAsciiFormatChanged();
                            break;
                        case ConsoleKey.H:
                            PrintHelp();
                            break;
                        case ConsoleKey.E:
                            OnEchoToggle();
                            break;
                        case ConsoleKey.U:
                            ProcessSendFile(terminal);
                            break;
                        case ConsoleKey.I:
                            PrintInfo();
                            break;
                        default:
                            break;
                    }

                    continue;
                }

                if (this.isEcho)
                {
                    this.ProcessEcho(key);
                }

                terminal.ProcessInput(key);


            }

        }


    }
}
