using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
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
        public MainWindow()
        {
            InitializeComponent();
            Main = this;
            FillFields();
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
    }
}
