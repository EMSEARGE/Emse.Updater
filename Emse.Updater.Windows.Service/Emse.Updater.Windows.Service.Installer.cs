using System.ComponentModel;
using System.Configuration.Install;
using System.ServiceProcess;


namespace Emse.Updater.Windows.Service
{
    [RunInstaller(true)]
    public class EmseUpdaterInstaller : Installer
    {
        public EmseUpdaterInstaller()
        {
            ServiceProcessInstaller serviceProcessInstaller = new ServiceProcessInstaller();
            ServiceInstaller serviceInstaller = new ServiceInstaller();

            //# Service Account Information
            serviceProcessInstaller.Account = ServiceAccount.LocalSystem;
            serviceProcessInstaller.Username = null;
            serviceProcessInstaller.Password = null;

            //# Service Information
            serviceInstaller.DisplayName = "Emse Updater";
            serviceInstaller.StartType = ServiceStartMode.Automatic;
            serviceInstaller.Description = "Emse Update Service";
            // This must be identical to the WindowsService.ServiceBase name
            // set in the constructor of WindowsService.cs
            serviceInstaller.ServiceName = "EmseUpdater";

            this.Installers.Add(serviceProcessInstaller);
            this.Installers.Add(serviceInstaller);
        }
    }
}
