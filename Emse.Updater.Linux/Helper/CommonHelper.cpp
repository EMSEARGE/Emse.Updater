#include <stdio.h>
#include <string.h>
#include <unistd.h>
#include <string>
#include <sstream>
#include <fstream>
#include <Configuration.h>
#include "../HeaderFiles/CommonHelper.h"

#define GetCurrentDir getcwd

#define SSTR( x ) static_cast< std::ostringstream & >( \
        ( std::ostringstream() << std::dec << x ) ).str()

using namespace std;

const char* CommonHelper::ConvertStringToConstChar(string str)
{
    char *cstr = new char[str.length() + 1];
    strcpy(cstr, str.c_str());
    return cstr;
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
}

void CommonHelper::UpdateFiles(const char *AppPath, const char *HomePath)
{
    cout<<"Updating..."<<endl;

    string cmd = string("rm -rf ") + AppPath;
    system(ConvertStringToConstChar(cmd));

    cmd = string("cd ") + AppPath + string("/.. && rm -rf tmp && mkdir tmp");
    system(ConvertStringToConstChar(cmd));

    cmd = string("cp -r TEMP.EmseQ.HS/. ") + HomePath + string("/tmp ");
    system(ConvertStringToConstChar(cmd));

    cmd = string("cd ") + HomePath;
    system(ConvertStringToConstChar(cmd));

    cmd = string("cd ") + HomePath + " && rm -rf EmseQ.Hardware.Service.Linux && mkdir EmseQ.Hardware.Service.Linux && cd EmseQ.Hardware.Service.Linux && cmake -G \"Unix Makefiles\" ../tmp && make";
    system(ConvertStringToConstChar(cmd));
}

void CommonHelper::DeleteTempFiles()
{
    cout<<"Deleting temp files"<<endl;

    system("rm -rf TEMP.EmseQ.HS");
    system("rm -rf tmp");
    system("rm -f tempDownloadedFile.zip");
}

ConfigurationDto CommonHelper::GetConfiguration()
{
    try
    {
        ConfigurationDto configuration;
        pugi::xml_document _doc;
        _doc.load_file("Configuration.xml");
        ConfigurationDtoXml::Loader loader;

        loader(_doc.root(), "Configuration", configuration);

        return configuration;
    }
    catch (...)
    {
        //Helper().WriteLog(ERRORLOG, "Could not read Configuration.xml");
        throw 1;
    }
}

std::string CommonHelper::CurrentDir()
{
    char cCurrentPath[FILENAME_MAX];

    if (!GetCurrentDir(cCurrentPath, sizeof(cCurrentPath)))
    {
        return "";
    }

    cCurrentPath[sizeof(cCurrentPath) - 1] = '\0'; /* not really required */

    return (std::string) cCurrentPath;
}
