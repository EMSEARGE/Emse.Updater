#include "CommonHelper.h"
#include <stdio.h>
#include <string.h>
#include <unistd.h>
#include <string>
#include <sstream>
#include <fstream>

#define SSTR( x ) static_cast< std::ostringstream & >( \
        ( std::ostringstream() << std::dec << x ) ).str()

using namespace std;

const char* CommonHelper::ConvertStringToConstChar(string str)
{
	char *cstr = new char[str.length() + 1];
	strcpy(cstr, str.c_str());
	return cstr;
}

string CommonHelper::GetCurrentDir()
{
	char cwd[1024];
	if (getcwd(cwd, sizeof(cwd)) != NULL) fprintf(stdout, "Current working dir: %s\n", cwd);
	string str(cwd);
	return str;
}

string CommonHelper::ConvertIntToString(int value)
{
	static string Result;
	ostringstream convert;
	convert << value;
	Result = convert.str();
	return Result;
}

const char *CommonHelper::ReadVersion()
{
	string line;
	ifstream myfile ("version.txt");
	if (myfile.is_open())
	{
		getline (myfile,line);
		myfile.close();
	}
	return ConvertStringToConstChar(line);
}

void CommonHelper::WriteVersion(const char *version)
{
	cout<<"Updating version.txt ..."<<endl;
	sleep(1);
	ofstream myfile ("version.txt");
	if (myfile.is_open())
	{
		myfile << version;
		myfile.close();
	}
}

int CommonHelper::cmpVersion(const char *currentVersion, const char *latestVersion)
{
    int i;
    int oct_v1[4], oct_v2[4];
    sscanf(currentVersion, "%d.%d.%d.%d", &oct_v1[0], &oct_v1[1], &oct_v1[2], &oct_v1[3]);
    sscanf(latestVersion, "%d.%d.%d.%d", &oct_v2[0], &oct_v2[1], &oct_v2[2], &oct_v2[3]);

    for (i = 0; i < 4; i++) {
        if (oct_v1[i] > oct_v2[i])
            return 1;
        else if (oct_v1[i] < oct_v2[i])
            return -1;
    }

    return 0;
}

void CommonHelper::ExtractFiles()
{
	cout<<"Extracting files..."<<endl;
	system("unzip -o -q tempDownloadedFile.zip -d TEMP.EmseQ.HS");
	sleep(1);
}

void CommonHelper::UpdateFiles(const char *AppPath)
{
	cout<<"Updating..."<<endl;

	string cmd = string("rm -rf ") + AppPath;
	system(ConvertStringToConstChar(cmd));

	cmd = string("cp -r TEMP.EmseQ.HS/. ") + AppPath;
	system(ConvertStringToConstChar(cmd));
	sleep(5);
	cmd = (string("sudo chmod -R 777 ") + AppPath);
	system(ConvertStringToConstChar(cmd));
}

void CommonHelper::DeleteTempFiles()
{
	cout<<"Deleting temp files"<<endl;
	sleep(1);
	system("rm -rf TEMP.EmseQ.HS");
	system("rm -f tempDownloadedFile.zip");
}
