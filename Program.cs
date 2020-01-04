using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TraySafe
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var assembly = typeof(Program).Assembly;
            var attribute = (GuidAttribute)assembly.GetCustomAttributes(typeof(GuidAttribute), true)[0];
            string appGuid = attribute.Value;

            using (Mutex mutex = new Mutex(false, "Global\\" + appGuid))
            {
                if (!mutex.WaitOne(0, false))
                {
                    MessageBox.Show("Instance of TraySafe already running");
                    return;
                }
                else if (!File.Exists("pin.tsf"))
                {
                    if (File.Exists("data.tsf") && File.Exists("labels.tsf"))
                    {
                        MessageBox.Show("The pin file seems to have been deleted manually. Removing storage files for safety measures.");
                        File.Delete("data.tsf");
                        File.Delete("labels.tsf");
                    }
                    Application.Run(new CreatePinForm());
                }
                else
                {
                    Application.Run(new LoginPinForm());
                }
            }
        }
    }
}
