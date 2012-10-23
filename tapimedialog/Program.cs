using System;
using System.Collections.Generic;
using System.Text;

namespace tapimedialog
{
    class Program
    {
        private static bool _quitRequested = false;

        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(CurrentDomain_ProcessExit);

            tapimedialog p = new tapimedialog();
            p.start();

            while (!_quitRequested)
            {
                System.Threading.Thread.Sleep(15000);
            }
            p.shutdown();

        }

        static void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            Console.WriteLine("ProcessExit");
            _quitRequested = true;
        }
                       
    }
}
