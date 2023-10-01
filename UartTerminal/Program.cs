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
using System.Runtime.InteropServices.WindowsRuntime;

namespace UartTerminal
{
    internal class Program
    {

        [STAThread]
        static void Main(string[] args)
        {
            AppManager.Instance.Run(args);

            return;

        }
    }
}
