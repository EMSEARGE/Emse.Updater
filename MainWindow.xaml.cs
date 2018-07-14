using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Emse.Updater.DTO;
using Emse.Updater.Helper;
using Microsoft.Win32;

namespace Emse.Updater
{
    public partial class MainWindow : Window
    {
        public static MainWindow Main { get; set; }

        public MainWindow()
        {
            InitializeComponent(); 
            App.MainWindowStatus = true;
            Main = this;    
            FillFields();

            Task.Factory.StartNew(() =>
            {
                Thread.CurrentThread.IsBackground = true;
                App.MainWindowThreadReadLog();
            });
        }
        

        public void FillFields()
        {
            SettingDto setting = Helper.JsonHelper.JsonReader();

            
            Emse.Updater.MainWindow.Main.Dispatcher.Invoke(new Action(() =>
            {
                TextBoxSecondsBetweenLoopText.Text = setting.SecondsBetweenLoop.ToString();
                TextBoxCurrentVersionText.Text = setting.CurrentVersion;
                TextBoxDomainText.Text = setting.Domain;
                TextBoxUrlZipText.Text = setting.UrlZip;
                TextBoxFileExtension.Text = setting.FileExtension;
                TextBoxPathText.Text = setting.Path;
                TextBoxTempPathText.Text = setting.TempPath;
                TextBoxExeNameText.Text = setting.ExeName;
                CheckBoxUpdateStatus.IsChecked = true;
                if (setting.UpdateStatus)
                {
                    CheckBoxUpdateStatus.IsChecked = true;
                    App.UpdateStatus = true;
                }

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

        private void CheckBoxUpdateStatus_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                CheckBox checkBox = sender as CheckBox;
                if (checkBox != null && checkBox.IsChecked != null && (bool) checkBox.IsChecked)
                {
                    App.UpdateStatus = true;
                    LogHelper.WriteLog("Emse Updater Enabled");
                }
                else
                {
                    App.UpdateStatus = false;
                    LogHelper.WriteLog("Emse Updater Disabled");

                }
            }
            catch (Exception ex)
            {
                Emse.Updater.Helper.LogHelper.WriteLog(ex.Message);
                LogHelper.WriteLog("Emse Updater CheckBox Not Working");
            }
        }

        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            Process[] process = Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName);

            if (process.Length > 0)
            {
                if (string.IsNullOrEmpty(process[0].MainWindowTitle))
                {
                    try
                    {
                        Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System", "EnableLUA", 0);
                        Thread.Sleep(2000);
                        Emse.Updater.Helper.WindowsHelper.ExitWindows(WindowsHelper.ExitWindowsType.ForceRestart, WindowsHelper.ShutdownReason.FlagPlanned, true);
                    }
                    catch (Exception ex)
                    {
                        // ignored
                    }
                }
            }
        }

        #region Log text to TextBoxLog and limit lines count number
        public void WriteLogLineListToMainWindow(List<string> logLineList)
        {
            if (logLineList != null)
            {
                TextBoxLog.Text = string.Empty;

                foreach (var logLine in logLineList)
                {
                    if (string.IsNullOrWhiteSpace(TextBoxLog.Text))
                    {
                        TextBoxLog.Text = logLine;
                    }
                    else
                    {
                        TextBoxLog.Text = TextBoxLog.Text + Environment.NewLine + logLine;
                    }
                }
            }
        }
        #endregion

        #region Validating Numaric input in text box
        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }
        #endregion


    }
}
