#include <sstream>
#include <dirent.h>
#include <cstring>
#include "ProcessHelper.h"
#include <boost/exception/all.hpp>
#include <boost/thread.hpp>
#include <App.h>
#include <CommonHelper.h>

const char* convertStrToChar(string str)
{
    char *cstr = new char[str.length() + 1];
    strcpy(cstr, str.c_str());
    return cstr;
}

string ConvertIntToString(int value)
{
    static string Result;
    ostringstream convert;
    convert << value;
    Result = convert.str();
    return Result;
}

void StartProcessThread(string AppPath, string AppName)
{
    cout<<"Starting Application: "<<AppName<<endl;
    system(convertStrToChar(string ("cd ") + AppPath + string(" && sudo ./") + AppName));
}

ProcessHelper::ProcessHelper()
{
    // TODO Auto-generated constructor stub
}

ProcessHelper::~ProcessHelper()
{
    // TODO Auto-generated destructor stub
}

int ProcessHelper::GetProcIdByName(string procName)
{
    int pid = -1;

    // Open the /proc directory
    DIR *dp = opendir("/proc");
    if (dp != NULL)
    {
        // Enumerate all entries in directory until process found
        struct dirent *dirp;
        while (pid < 0 && (dirp = readdir(dp)))
        {
            // Skip non-numeric entries
            int id = atoi(dirp->d_name);
            if (id > 0)
            {
                // Read contents of virtual /proc/{pid}/cmdline file
                string cmdPath = string("/proc/") + dirp->d_name + "/cmdline";
                ifstream cmdFile(cmdPath.c_str());
                string cmdLine;
                getline(cmdFile, cmdLine);
                if (!cmdLine.empty())
                {
                    // Keep first cmdline item which contains the program path
                    size_t pos = cmdLine.find('\0');
                    if (pos != string::npos)
                        cmdLine = cmdLine.substr(0, pos);
                    // Keep program name only, removing the path
                    pos = cmdLine.rfind('/');
                    if (pos != string::npos)
                        cmdLine = cmdLine.substr(pos + 1);
                    // Compare against requested process name
                    if (procName == cmdLine)
                        pid = id;
                }
            }
        }
    }

    closedir(dp);
    return pid;
}

void ProcessHelper::StartProcessByName(string AppPath, string procName)
{
    boost::thread thread(&StartProcessThread, AppPath, procName);
    thread.detach();
}

void ProcessHelper::CloseProcessByName(string procName)
{
    const char *command = convertStrToChar(string("sudo killall -9 " +  App::Configuration.AppName));
    system(command);
}

void ProcessHelper::StartUpdateScreen()
{
    CloseKioskScreen();

    std::string url = "google-chrome "
                              "file://" + CommonHelper().CurrentDir() + "/updater.html "
                              "--user-data-dir=/tmp/chrome-data "
                              "--no-sandbox "
                              "--no-first-run "
                              "--kiosk "
                              "--incognito "
                              "--disable-pinch "
                              "--overscroll-history-navigation=0 "
                              "--fast --fast-start "
                              "--unsafely-treat-insecure-origin-as-secure= file://" + CommonHelper().CurrentDir() + "/updater.html "
                              "--allow-running-insecure-content "
                              "--check-for-update-interval=604800 "
                              "--extensions-update-frequency=604800 "
                              "--new-window "
                              "--window-position=0,0 "
                              "--disable-notifications "
                              "--enable-media-stream "
                              "--no-default-browser-check "
                              "--aggressive-cache-discard "
                              "--disk-cache-size=1 --disable-cache "
                              "--disk-cache-dir=/tmp/chrome-data "
                              "--media-cache-dir=/tmp/chrome "
                              "--disable-gpu-program-cache "
                              "--disable-gpu-shader-disk-cache "
                              "--disable-lru-snapshot-cache "
                              "--disable-application-cache "
                              "--use-fake-ui-for-media-stream "
                              "--ignore-certificate-errors "
                              "--test-type";

    std::thread t3((system), "sudo rm -rf /tmp/chrome-data");
    t3.detach();

    std::thread t4((system), "sudo rm -rf /tmp/chrome");
    t4.detach();

    std::thread t2((system), url.c_str());
    t2.detach();
}

void ProcessHelper::CloseKioskScreen()
{
    system("sudo killall -9 chrome");
}

