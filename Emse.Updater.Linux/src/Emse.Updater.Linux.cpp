#include "App.h"
#include "Helper/CommonHelper.h"
#include "Helper/XMLParserHelper.h"
#include "Helper/CurlHelper.h"
#include "Helper/ProcessHelper.h"
#include <stdio.h>
#include <stdlib.h>
#include <sstream>
#include <sys/time.h>
#include <unistd.h>

extern string Domain;
extern string AppPath;
extern string AppName;
extern string CurrentVersion;
extern string LatestVersion;

int main() {



	XMLParserHelper().ParseConfig();
	cout<<"Domain Adress: "<<Domain<<endl;
	cout<<"Application Path: "<<AppPath<<endl;
	cout<<"Application Name: "<<AppName<<endl;

	CurrentVersion = CommonHelper().ReadVersion();
	cout<<"Current Version: "<<CurrentVersion<<endl;

	int pid = ProcessHelper().GetProcIdByName(AppName);

	//TODO extract
	//TODO update
	//TODO delete temp
	//TODO start app

	while(true)
	{
		if(ProcessHelper().GetProcIdByName(AppName) == -1)
		{
			ProcessHelper().StartProcessByName(AppPath, AppName);
		}

		XMLParserHelper().ParseConfig();
		CurrentVersion = CommonHelper().ReadVersion();
		if(CurlHelper().CheckVersion(Domain))
		{
			if(CommonHelper().cmpVersion(CommonHelper().ConvertStringToConstChar(CurrentVersion), CommonHelper().ConvertStringToConstChar(LatestVersion)) == 0)
			{
				sleep(10);
				continue;
			}
			else
			{
				ProcessHelper().CloseProcessByName(AppName);

				if(CurlHelper().DownloadFiles(Domain))
				{
					CommonHelper().ExtractFiles();
					CommonHelper().UpdateFiles(CommonHelper().ConvertStringToConstChar(AppPath));
					CommonHelper().DeleteTempFiles();
					CommonHelper().WriteVersion(CommonHelper().ConvertStringToConstChar(LatestVersion));
				}
				else
				{
					cout<<"Download Fail"<<endl;
				}
				ProcessHelper().StartProcessByName(AppPath, AppName);
			}
		}
		else
		{
			cout<<"No connection..."<<endl;
		}

		sleep(10);
	}



	return 0;
}

