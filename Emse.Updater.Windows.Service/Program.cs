using System;
using System.Threading;
using System.Threading.Tasks;
using System.ServiceProcess;

namespace Emse.Updater.Windows.Service
{
    class Program : ServiceBase
    {
        static void Main(string[] args)
        {
            //if console app
            if (!Environment.UserInteractive)
                // running as service
                ServiceBase.Run(new Program());
            else
            {
                Program.Start(args);
            }
        }
        private static void Start(string[] args)
        {
            
            Task.Factory.StartNew(() =>
            {
                Thread.CurrentThread.IsBackground = true;
                Engine.Execute();
            });
            Console.ReadLine();
            Console.ReadKey(true);
            new Program().OnStop();
        }
        protected override void OnStart(string[] args)
        {
            //if windows service
            Program.Main(args);
            base.OnStart(args);
        }
        protected override void OnStop()
        {
            base.OnStop();
        }
    }
}
