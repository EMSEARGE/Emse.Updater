#include <iostream>
#include <boost/thread/pthread/thread_data.hpp>
#include "LogHelper.h"
#include "CommonHelper.h"
#include "CurlHelper.h"
#include "App.h"
#include "ProcessHelper.h"

int main() 
{
    try
    {
        LogHelper().CheckLoggerDir();
    }
    catch (const std::exception& ex)
    {
        LogHelper().WriteLog(INFOLOG, "Logger directory error!");
        exit(0);
    }
    catch (const std::string& ex)
    {
        LogHelper().WriteLog(INFOLOG, "Logger directory error!");
        exit(0);
    }
    catch ( ... )
    {
        LogHelper().WriteLog(INFOLOG, "Logger directory error!");
        exit(0);
    }

    App::Configuration = CommonHelper().GetConfiguration();

    cout<<"Domain Adress    : " << App::Configuration.Domain            << endl;
    cout<<"Application Path : " << App::Configuration.AppPath           << endl;
    cout<<"Application Name : " << App::Configuration.AppName           << endl;
    cout<<"ThreadSleepCount : " << App::Configuration.ThreadSleepCount  << endl;

    App::CurrentVersion = CommonHelper().ReadVersion();
    cout<<"Current Version: "<<App::CurrentVersion<<endl;

    LogHelper().WriteLog(INFOLOG, " Configuration has been read." );
    LogHelper().WriteLog(INFOLOG, " Domain Adress    :  " + App::Configuration.Domain);
    LogHelper().WriteLog(INFOLOG, " Application Path :  " + App::Configuration.AppPath);
    LogHelper().WriteLog(INFOLOG, " Application Name :  " + App::Configuration.AppName);
    LogHelper().WriteLog(INFOLOG, " ThreadSleepCount :  " + App::Configuration.ThreadSleepCount);

    std::cout << "Application closed: " + App::Configuration.AppName << std::endl;
    ProcessHelper().CloseProcessByName(App::Configuration.AppName);
    LogHelper().WriteLog(INFOLOG, "Process has been stopped: " + App::Configuration.AppName);

    while(true)
    {
        if(ProcessHelper().GetProcIdByName(App::Configuration.AppName ) == -1)
        {
            ProcessHelper().StartProcessByName(App::Configuration.AppPath , App::Configuration.AppName );
            LogHelper().WriteLog(INFOLOG, "Process has been started: " + App::Configuration.AppName);
        }

        App::Configuration = CommonHelper().GetConfiguration();
        App::CurrentVersion = CommonHelper().ReadVersion();

        if(CurlHelper().CheckVersion(App::Configuration.Domain))
        {
            if( CommonHelper().cmpVersion(CommonHelper().ConvertStringToConstChar(App::CurrentVersion), CommonHelper().ConvertStringToConstChar(App::LatestVersion)) == 0 )
            {
                boost::this_thread::sleep_for(boost::chrono::seconds(App::Configuration.ThreadSleepCount));
                continue;
            }
            else
            {
                LogHelper().WriteLog(INFOLOG, "Update process has been started. ");
                ProcessHelper().CloseProcessByName(App::Configuration.AppName );
                LogHelper().WriteLog(INFOLOG, "Process has been stopped: " + App::Configuration.AppName);

                std::cout << "Downloading update files." << std::endl;

                if(CurlHelper().DownloadFiles(App::Configuration.Domain ))
                {
                    LogHelper().WriteLog(INFOLOG, "Update files have been downloaded. ");

                    std::cout << "Extracting update files." << std::endl;
                    CommonHelper().ExtractFiles();
                    LogHelper().WriteLog(INFOLOG, "Update files have been extracted. ");

                    std::cout << "Updating application files." << std::endl;
                    CommonHelper().UpdateFiles(CommonHelper().ConvertStringToConstChar(App::Configuration.AppPath), CommonHelper().ConvertStringToConstChar(App::Configuration.HomePath));
                    LogHelper().WriteLog(INFOLOG, "Application files have been updated. ");

                    std::cout << "Deleting temp files." << std::endl;
                    CommonHelper().DeleteTempFiles();
                    LogHelper().WriteLog(INFOLOG, "Temp files have been deleted. ");

                    std::cout << "Updating version.txt" << std::endl;
                    CommonHelper().WriteVersion(CommonHelper().ConvertStringToConstChar(App::LatestVersion));
                    LogHelper().WriteLog(INFOLOG, "version.txt has been updated. ");

                    std::cout << "Starting process: " + App::Configuration.AppName << std::endl;
                    ProcessHelper().StartProcessByName(App::Configuration.AppPath , App::Configuration.AppName );
                    LogHelper().WriteLog(INFOLOG, "Process has been started: " + App::Configuration.AppName);
                }
                else
                {
                    cout<<"Download Fail"<<endl;
                    LogHelper().WriteLog(INFOLOG, "Download error");
                }
            }
        }
        else
        {
            cout<<"No connection..."<<endl;
            LogHelper().WriteLog(INFOLOG, "No internet connection.");
        }

        boost::this_thread::sleep_for(boost::chrono::seconds(App::Configuration.ThreadSleepCount));
    }

    cin.get();

    return 0;
}