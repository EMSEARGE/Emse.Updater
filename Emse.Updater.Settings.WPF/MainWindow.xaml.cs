using System;
using System.Linq;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Emse.Updater.DTO;
using Emse.Updater.Helper;

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

        public MainWindow()
        {
            
            InitializeComponent();
            Main = this;
            FillFields();
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
                sc = new ServiceController("EmseUpdater");
                ServiceInstalled = Helper.WindowsServiceHelper.IsServiceInstalled("EmseUpdater");
                if (ServiceInstalled)
                {
                    switch (sc.Status)
                    {
                        case ServiceControllerStatus.Running:
                            Application.Current.Dispatcher.Invoke(new Action(() =>
                            {
                                LabelServiceStatusContent.Content = "Runnig";
                                ButtonServiceNetStop.Visibility = Visibility.Visible;
                                ButtonServiceNetStart.Visibility = Visibility.Hidden;
                            }));
                            break;
                        case ServiceControllerStatus.Stopped:
                            Application.Current.Dispatcher.Invoke(new Action(() =>
                            {
                                LabelServiceStatusContent.Content = "Stopped";
                                ButtonServiceNetStart.Visibility = Visibility.Visible;
                                ButtonServiceNetStop.Visibility = Visibility.Hidden;
                            }));
                            break;
                        case ServiceControllerStatus.Paused:
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
                TextBoxSecondsBetweenLoopText.Text = setting.SecondsBetweenLoop.ToString();
                TextBoxCurrentVersionText.Text = setting.CurrentVersion;
                TextBoxDomainText.Text = setting.Domain;
                TextBoxUrlZipText.Text = setting.UrlZip;
                TextBoxFileExtension.Text = setting.FileExtension;
                TextBoxPathText.Text = setting.Path;
                TextBoxTempPathText.Text = setting.TempPath;
                TextBoxExeNameText.Text = setting.ExeName;
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
            if (sc.Status == ServiceControllerStatus.Stopped)
            {
                sc.Start();
            }
        }
        private void ButtonStopService_Click(object sender, RoutedEventArgs e)
        {
            if (sc.Status == ServiceControllerStatus.Running)
            {
                sc.Stop();
            }
        }

        public static bool IsServiceInstalled(string serviceName)
        {
            ServiceController[] services = ServiceController.GetServices();
            return services.Any(service => service.ServiceName == serviceName);
        }
    }
}
