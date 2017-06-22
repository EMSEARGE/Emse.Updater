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
        }
        private static void Start(string[] args)
        {
            Helper.LogHelper.WriteLog("Service Started!");
            Task.Factory.StartNew(() =>
            {
                Thread.CurrentThread.IsBackground = true;
                Program.Go();
            });
            new Program().OnStop();
        }

        protected static void Go()
        {
            Helper.LogHelper.WriteLog("Go!");
            Engine.Execute();
        }
        protected override void OnStart(string[] args)
        {
            //if windows service
            Helper.LogHelper.WriteLog("Service on start");
            Program.Start(args);
            base.OnStart(args);
        }
        protected override void OnStop()
        {
            base.OnStop();
        }
    }
}
