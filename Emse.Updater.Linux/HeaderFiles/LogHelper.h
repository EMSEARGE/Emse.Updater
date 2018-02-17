//
// Created by root on 16.02.2018.
//

#ifndef EMSE_UPDATER_LINUX_LOGHELPER_H
#define EMSE_UPDATER_LINUX_LOGHELPER_H

#include "spdlog.h"

#define INFOLOG 1
#define ERRORLOG 2
#define WARNINGLOG 3
#define CRITICALLOG 4

class LogHelper
{
public:
    void WriteLog(int logType, std::string log);
    void CheckLoggerDir();
};


#endif //EMSE_UPDATER_LINUX_LOGHELPER_H
