#include <string>
#include <iostream>
#include <zconf.h>
#include <dirent.h>
#include <unistd.h>
#include <stdlib.h>
#include <stdio.h>

using namespace std;
class CurlHelper {
public:
	CurlHelper();
	virtual ~CurlHelper();
	bool DownloadFiles(string Domain);
	bool CheckVersion(string Domain);
};
