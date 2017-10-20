#include "ProcessHelper.h"
#include <sstream>
#include <pthread.h>
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

void StartProcessThread(string AppPath, string AppName){
	system(convertStrToChar(string("cd ") + AppPath + string("/Debug && sudo ./") + AppName));
}

ProcessHelper::ProcessHelper() {
	// TODO Auto-generated constructor stub

}

ProcessHelper::~ProcessHelper() {
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
	thread t1(&StartProcessThread, AppPath, procName);
	t1.detach();
	//system(convertStrToChar(string("cd ") + AppPath + string("/Debug && sudo ./") + procName));
	//StartProcessThread(AppPath, procName);
}

void ProcessHelper::CloseProcessByName(string procName){
	system(convertStrToChar(string("sudo kill -TERM -- -") + ConvertIntToString(GetProcIdByName(procName))));

}
