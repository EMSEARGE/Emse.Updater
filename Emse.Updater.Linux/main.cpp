#include <iostream>
#include <boost/thread/pthread/thread_data.hpp>
#include "CommonHelper.h"
#include "ProcessHelper.h"
#include "CurlHelper.h"
#include "App.h"

int main() 
{
    App::Configuration = CommonHelper().GetConfiguration();

    cout<<"Domain Adress    : " << App::Configuration.Domain            << endl;
    cout<<"Home Path        : " << App::Configuration.AppPath           << endl;
    cout<<"Application Path : " << App::Configuration.AppPath           << endl;
    cout<<"Application Name : " << App::Configuration.AppName           << endl;
    cout<<"ThreadSleepCount : " << App::Configuration.ThreadSleepCount  << endl;

    App::CurrentVersion = CommonHelper().ReadVersion();
    cout<<"Current Version: "<<App::CurrentVersion<<endl;

    ProcessHelper().CloseProcessByName(App::Configuration.AppName);

    while(true)
    {
        if(ProcessHelper().GetProcIdByName(App::Configuration.AppName ) == -1)
        {
            ProcessHelper().StartProcessByName(App::Configuration.AppPath , App::Configuration.AppName );
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
                ProcessHelper().CloseProcessByName(App::Configuration.AppName );

                if(CurlHelper().DownloadFiles(App::Configuration.Domain ))
                {
                    CommonHelper().ExtractFiles();
                    CommonHelper().UpdateFiles(CommonHelper().ConvertStringToConstChar(App::Configuration.AppPath), CommonHelper().ConvertStringToConstChar(App::Configuration.HomePath));
                    CommonHelper().DeleteTempFiles();
                    CommonHelper().WriteVersion(CommonHelper().ConvertStringToConstChar(App::LatestVersion));
                    ProcessHelper().StartProcessByName(App::Configuration.AppPath , App::Configuration.AppName );
                }
                else
                {
                    cout<<"Download Fail"<<endl;
                }
            }
        }
        else
        {
            cout<<"No connection..."<<endl;
        }

        boost::this_thread::sleep_for(boost::chrono::seconds(App::Configuration.ThreadSleepCount));
    }

    cin.get();

    return 0;
}