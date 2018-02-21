#ifndef COMMONHELPER_H_
#define COMMONHELPER_H_

#include <string>
#include <iostream>
#include <zconf.h>
#include <dirent.h>
#include <unistd.h>
#include <stdlib.h>
#include <stdio.h>
#include <string.h>
#include "Configuration.h"

using namespace std;

class CommonHelper {

public:
    ConfigurationDto GetConfiguration();
    const char* ConvertStringToConstChar(string str);
    void WriteVersion(const char *version);
    const char *ReadVersion();
    int cmpVersion(const char *currentVersion, const char *latestVersion);
    void ExtractFiles();
    void UpdateFiles(const char *AppPath, const char *HomePath);
    void DeleteTempFiles();
    std::string CurrentDir();
};

#endif