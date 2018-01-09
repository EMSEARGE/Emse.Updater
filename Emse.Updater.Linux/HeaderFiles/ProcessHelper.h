#include <string>
#include <iostream>
#include <thread>
#include <zconf.h>
#include <dirent.h>
#include <fstream>
#include <unistd.h>
#include <stdlib.h>
#include <stdio.h>
#include <string.h>

using namespace std;

class ProcessHelper {
public:
    ProcessHelper();
    virtual ~ProcessHelper();
    int GetProcIdByName(string procName);
    void StartProcessByName(string AppPath, string procName);
    void CloseProcessByName(string procName);
};