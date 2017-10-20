#include <string>
#include <iostream>
#include <zconf.h>
#include <dirent.h>
#include <unistd.h>
#include <stdlib.h>
#include <stdio.h>
#include <string.h>

using namespace std;

class CommonHelper {

public:
	const char* ConvertStringToConstChar(string str);
	string GetCurrentDir();
	unsigned int GetFingerPrintID();
	string GetFingerPrint();
	string ConvertIntToString(int value);
	void WriteVersion(const char *version);
	const char *ReadVersion();
	int cmpVersion(const char *currentVersion, const char *latestVersion);
	void ExtractFiles();
	void UpdateFiles(const char *AppPath);
	void DeleteTempFiles();
};

