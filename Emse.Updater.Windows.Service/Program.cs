using System;
using System.Diagnostics;
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
                new Program().OnStop();
            }
            Console.ReadLine();
        }
        private static void Start(string[] args)
        {
            LoggerAdapter.Instance.Debug("Started");
            Task.Factory.StartNew(() =>
            {
                Thread.CurrentThread.IsBackground = true;
                Program.Go();
            });
            new Program().OnStop();
        }

        protected static void Go()
        {
            LoggerAdapter.Instance.Debug("Go!");
            Engine.Execute();
        }
        protected override void OnStart(string[] args)
        {
            LoggerAdapter.Instance.Debug("Service on start");
            Program.Start(args);
            base.OnStart(args);
        }
        protected override void OnStop()
        {
            base.OnStop();
        }
    }
}
