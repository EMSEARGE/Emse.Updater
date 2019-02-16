using System;
using System.Linq;
using System.Security.Principal;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Emse.Updater.DTO;
using Emse.Updater.Helper;
using IWshRuntimeLibrary;

namespace Emse.Updater.Settings.WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static MainWindow Main { get; set; }
        public static bool ServiceInstalled { get; set; }
        public static ServiceController sc = new ServiceController("EmseUpdater");
        public static string CurrentUser = Environment.UserName;
        public MainWindow()
        {
            sc = new ServiceController("EmseUpdater");
            InitializeComponent();
            Main = this;
            FillFields();
            CreateShortcut();
            Task.Factory.StartNew(() =>
            {
                Thread.CurrentThread.IsBackground = true;
                CheckWindowsService();
            });
        }
        private void CheckWindowsService()
        {
            while (true)
            {
                sc.Dispose();
                sc = new ServiceController("EmseUpdater");
                ServiceInstalled = Helper.WindowsServiceHelper.IsServiceInstalled("EmseUpdater");
                if (ServiceInstalled)
                {
                    Application.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        ButtonUnRegisterService.Visibility = Visibility.Visible;
                        ButtonRegisterService.Visibility = Visibility.Hidden;
                    }));
                    switch (sc.Status)
                    {
                        case ServiceControllerStatus.Running:
                            Thread.Sleep(500);
                            Application.Current.Dispatcher.Invoke(new Action(() =>
                            {
                                LabelServiceStatusContent.Content = "Running";
                                ButtonServiceNetStop.Visibility = Visibility.Visible;
                                ButtonServiceNetStart.Visibility = Visibility.Hidden;
                            }));
                            break;
                        case ServiceControllerStatus.Stopped:
                            Thread.Sleep(500);
                            Application.Current.Dispatcher.Invoke(new Action(() =>
                            {
                                LabelServiceStatusContent.Content = "Stopped";
                                ButtonServiceNetStart.Visibility = Visibility.Visible;
                                ButtonServiceNetStop.Visibility = Visibility.Hidden;
                            }));
                            break;
                        case ServiceControllerStatus.Paused:
                            Thread.Sleep(500);
                            Application.Current.Dispatcher.Invoke(new Action(() =>
                            {
                                LabelServiceStatusContent.Content = "Paused";
                            }));
                            break;
                    }
                }
                else
                {
                    Application.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        ButtonUnRegisterService.Visibility = Visibility.Hidden;
                        ButtonRegisterService.Visibility = Visibility.Visible;
                        LabelServiceStatusContent.Content = "Service Not Found!";
                        ButtonServiceNetStop.Visibility = Visibility.Hidden;
                        ButtonServiceNetStart.Visibility = Visibility.Hidden;
                    }));
                }
                Thread.Sleep(500);
            }
        }
        public void FillFields()
        {
            SettingDto setting = Helper.JsonHelper.JsonReader();

            Main.Dispatcher.Invoke(new Action(() =>
            {
                if (IsLocalAdmin() && IsAdministrator())
                {
                    LabelCurrentUserRoleContent.Content = "Administrator";
                }
                else
                {
                    LabelCurrentUserRoleContent.Content = "Not Admin";
                    MessageBox.Show("Administrator hesabı ile giriş yapmalısınız.");
                    Environment.Exit(1);
                }
                LabelCurrentUserContent.Content = CurrentUser;
                TextBoxSecondsBetweenLoopText.Text = setting.SecondsBetweenLoop.ToString();
                TextBoxCurrentVersionText.Text = setting.CurrentVersion;
                TextBoxDomainText.Text = setting.Domain;
                TextBoxUrlZipText.Text = setting.UrlZip;
                TextBoxFileExtension.Text = setting.FileExtension;
                TextBoxPathText.Text = setting.Path;
                TextBoxTempPathText.Text = setting.TempPath;
                TextBoxExeNameText.Text = setting.ExeName;
                TextBoxFilesToKeep.Text = setting.FilesToKeep;
            }));
        }
        private void ButtonKaydet_Click(object sender, RoutedEventArgs e)
        {
            int secondsBetweenLoop = 10;
            try
            {
                secondsBetweenLoop = Convert.ToInt32(TextBoxSecondsBetweenLoopText.Text);
                if (secondsBetweenLoop < 10)
                {
                    secondsBetweenLoop = 10;
                    TextBoxSecondsBetweenLoopText.Text = secondsBetweenLoop.ToString();
                }
            }
            catch
            {
                TextBoxSecondsBetweenLoopText.Text = secondsBetweenLoop.ToString();
            }
            try
            {
                SettingDto settings = new SettingDto
                {
                    SecondsBetweenLoop = secondsBetweenLoop,
                    CurrentVersion = TextBoxCurrentVersionText.Text,
                    Domain = TextBoxDomainText.Text,
                    UrlZip = TextBoxUrlZipText.Text,
                    FileExtension = TextBoxFileExtension.Text,
                    Path = TextBoxPathText.Text,
                    TempPath = TextBoxTempPathText.Text,
                    ExeName = TextBoxExeNameText.Text,
                    FilesToKeep = TextBoxFilesToKeep.Text,

                    UpdateStatus = true

                };
                Helper.JsonHelper.JsonWriter(settings);

                LogHelper.WriteLog("JsonWriter file as Writed");


            }
            catch (Exception ex)
            {
                Emse.Updater.Helper.LogHelper.WriteLog(ex.Message);
            }
        }
        private void ButtonStartService_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sc.Status == ServiceControllerStatus.Stopped)
                {
                    sc.Start();
                }
                Thread.Sleep(500);
            }
            catch (Exception)
            {
                // ignored
            }
        }
        private void ButtonStopService_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sc.Status == ServiceControllerStatus.Running)
                {
                    sc.Stop();
                }
                Thread.Sleep(500);
            }
            catch (Exception ex)
            {
                // ignored
            }
        }
        private void ButtonRegisterService_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!ServiceInstalled)
                {
                    Helper.WindowsServiceHelper.RegisterWindowsService("EmseUpdater");
                }
                Thread.Sleep(500);
                if (sc.Status == ServiceControllerStatus.Stopped)
                {
                    sc.Start();
                }
            }
            catch (Exception ex)
            {
                // ignored
            }
        }
        private void ButtonUnRegisterService_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sc.Status == ServiceControllerStatus.Running)
                {
                    sc.Stop();
                }
                Thread.Sleep(500);
                if (ServiceInstalled)
                {
                    Helper.WindowsServiceHelper.UnRegisterWindowsService("EmseUpdater");
                }
            }
            catch (Exception)
            {
                // ignored
            }
        }
        public static bool IsAdministrator()
        {
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }
        public static bool IsLocalAdmin()
        {
            WindowsIdentity windowsIdentity = WindowsIdentity.GetCurrent();

            // Get the built-in administrator account.
            SecurityIdentifier sid = new SecurityIdentifier(WellKnownSidType.BuiltinAdministratorsSid, null);

            // Compare to the current user.
            bool isBuiltInAdmin = (windowsIdentity.User == sid);

            SecurityIdentifier localAdminGroupSid = new SecurityIdentifier(
            WellKnownSidType.BuiltinAdministratorsSid, null);
            return  windowsIdentity.Groups.Select(g => (SecurityIdentifier)g.Translate(typeof(SecurityIdentifier))).Any(s => s == localAdminGroupSid);
        } 
        private void CreateShortcut()
        {
            object shDesktop = (object)"Desktop";
            WshShell shell = new WshShell();
            string shortcutAddress = (string)shell.SpecialFolders.Item(ref shDesktop) + @"\Emse.Updater.Settings.lnk";
            IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(shortcutAddress);
            shortcut.Description = "New shortcut for a Emse.Updater.Settings";
            shortcut.Hotkey = "Ctrl+Shift+N";
            //shortcut.TargetPath = @"C:\Emse.Updater\Emse.Updater.Settings.exe";
            shortcut.TargetPath = System.AppDomain.CurrentDomain.BaseDirectory + PathHelper.CurrentExeLocation();
            shortcut.Save();
        }
    }
}
